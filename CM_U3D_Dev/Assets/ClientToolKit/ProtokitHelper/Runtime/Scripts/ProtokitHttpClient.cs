using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using Google.Protobuf;
using gen.netutils;
using MTool.Framework.Base;
using MTool.LoggerModule.Runtime;
using UnityEngine;
using UnityEngine.Networking;
using ILogger = MTool.LoggerModule.Runtime.ILogger;

namespace ProtokitHelper
{
    public delegate void DelegateRecvProto(int sequenceId, string passThrough, string uri, byte[] data);
    public delegate void DelegateRecvPacketFinish(int sequenceId);
    public delegate void DelegateHttpResponseFailed(int statusCode, string statusName);
    public delegate void DelegateRecvProtoBytes(string uri, byte[] data);
    public delegate void DelegateRecvCommonError(int code, string message);
    public delegate void DelegateResponseHandler(IMessage message, Dictionary<string, string> headers, Dictionary<string, string> metadata);
    public delegate void DelegateHttpRequestError(int statusCode);

    public class ProtokitHttpClient : Singleton<ProtokitHttpClient>
    {
        /// <summary>
        /// 超时时间(毫秒)
        /// </summary>
        private int timeOutms = 15000;
        /// <summary>
        /// 消息预期返回时间
        /// </summary>
        private float timeExpect = 0.5f;
        //private ResourcePool<HttpProtoRequest> RequestPool = new ResourcePool<HttpProtoRequest>(() => new HttpProtoRequest(), HttpProtoRequest.Init, null);
        private Queue<HttpProtoRequest> SequentialRequests = new Queue<HttpProtoRequest>(256);
        private Queue<HttpRawProto> SequentialRawProtos = new Queue<HttpRawProto>(1024);
        private Dictionary<string, Action<IMessage>> ReqeustMsgHandlerMap = new Dictionary<string, Action<IMessage>>();
        private Dictionary<string, Action<IMessage>> MessageHandlersMap = new Dictionary<string, Action<IMessage>>();
        private Dictionary<string, DelegateRecvProtoBytes> RequestBytesHandlerMap = new Dictionary<string, DelegateRecvProtoBytes>();
        private HttpProtoRequest CurRequest;
        private HttpProtoRequest LastFailedRequest;
        private Dictionary<int, RequestBatchRecord> RequestBatchRecordMap = new Dictionary<int, RequestBatchRecord>();
        private Dictionary<string, DelegateResponseHandler> RequestResponseHandlerMap = new Dictionary<string, DelegateResponseHandler>();
        private Dictionary<int, Dictionary<string, string>> ResponseHeaderMap = new Dictionary<int, Dictionary<string, string>>();
        private Dictionary<int, Dictionary<string, string>> ResponseMetadataMap = new Dictionary<int, Dictionary<string, string>>();
        private Dictionary<string, DelegateHttpRequestError> RequestErrorHandlerMap = new Dictionary<string, DelegateHttpRequestError>();
        /// <summary>
        /// 是否允许请求
        /// </summary>
        private bool EnableSendRequest = false;
        /// <summary>
        /// 消息最大合并数
        /// </summary>
        public int MaxBatchCount { get; private set; } = 1;
        /// <summary>
        /// 重发次数
        /// </summary>
        private int MaxRetry = 3;
        /// <summary>
        /// 消息接收
        /// </summary>
        public event DelegateRecvProto Evt_RecvMsg;
        /// <summary>
        /// 消息包解析完成
        /// </summary>
        public event DelegateRecvPacketFinish Evt_RecvPackFinish;
        /// <summary>
        /// 请求返回了底层错误
        /// </summary>
        public event DelegateRecvCommonError Evt_RecvCommonError;
        /// <summary>
        /// 返回了错误的状态码
        /// </summary>
        public event DelegateHttpResponseFailed Evt_RspFailed;
        /// <summary>
        /// Update轮询
        /// </summary>
        public event Action Evt_Update;
        /// <summary>
        /// 开始发送请求
        /// </summary>
        public event Action Evt_ReqBegin;
        /// <summary>
        /// 一次请求结束
        /// </summary>
        public event Action Evt_ReqEnd;
        /// <summary>
        /// 请求失败
        /// </summary>
        public event Action Evt_ReqFailed;
        /// <summary>
        /// 请求超出预期时间
        /// </summary>
        public event Action Evt_ReqTimeOutExpect;

        public HttpClient HttpAgent { get; } = new HttpClient();

        /// <summary>
        /// 是否允许使用queue模式合并请求
        /// </summary>
        public bool EnableBatchRequest { get; private set; }

        /// <summary>
        /// 是否允许自动重发
        /// </summary>
        public bool EnableAutoRetry { get; private set; }

        /// <summary>
        /// user token
        /// </summary>
        public string HttpToken { get; private set; }

        /// <summary>
        /// Http方案
        /// </summary>
        public HttpScheme Scheme { get; private set; } = HttpScheme.HttpClient;

        /// <summary>
        /// 设置消息发送UnityWebRequest.useHttpContinue
        /// </summary>
        public bool UseHttpContinue { get; private set; } = true;

        private MonoBehaviour Mono;

        private StringBuilder sb = new StringBuilder();

        private static readonly Lazy<ILogger> Logger = new Lazy<ILogger>(() => LoggerManager.GetLogger("ProtokitHttpClient"));

        /// <summary>
        /// 是否输出消息收发日志
        /// </summary>
        public bool EnableProtoLog = true;

        public void Init(bool queueMode, bool autoRetry, int batchLimit = 1)
        {
            EnableSendRequest = true;
            EnableBatchRequest = queueMode;
            EnableAutoRetry = autoRetry;
            MaxBatchCount = batchLimit;
        }

        public void SetHttpToken(string token)
        {
            HttpToken = token;
        }

        /// <summary>
        /// 设置超时时间（单位:毫秒）
        /// </summary>
        /// <param name="ms"></param>
        public void SetTimeOut(int ms)
        {
            timeOutms = ms;
        }

        /// <summary>
        /// 设置重试次数
        /// </summary>
        /// <param name="count"></param>
        public void SetRetryLimit(int count)
        {
            MaxRetry = count;
        }

        /// <summary>
        /// 设置消息预期返回时间
        /// </summary>
        /// <param name="time"></param>
        public void SetExpectTime(float time)
        {
            timeExpect = time;
        }

        /// <summary>
        /// 设置UnityWebRequest.useHttpContinue
        /// </summary>
        /// <param name="v"></param>
        public void SetUseHttpContinue(bool v)
        {
            UseHttpContinue = v;
        }

        public void SwitchSchemeHttpClient()
        {
            Scheme = HttpScheme.HttpClient;
        }

        public void SwitchSchemeUnityWebRequest(MonoBehaviour mono)
        {
            Scheme = HttpScheme.UnityWebRequest;
            Mono = mono;
        }

        public void PostRequestMsg(string url, IMessage msg, Action<IMessage> callback = null, bool waitResponse = true, DelegateHttpRequestError errCallback = null)
        {
            HttpRawProto httpRawProto = new HttpRawProto
            {
                Url = url,
                RawProto = ProtokitUtil.Instance.GetRawPorto(msg),
                Method = HttpMethod.Post
            };
            if (callback != null)
            {
                ReqeustMsgHandlerMap.Add(httpRawProto.RawProto.Passthrough, callback);
            }
            if (errCallback != null)
                RequestErrorHandlerMap.Add(httpRawProto.RawProto.Passthrough, errCallback);
            SequentialRawProtos.Enqueue(httpRawProto);
        }

        public void PostRequestHandleResponse(string url, IMessage msg, DelegateResponseHandler callback, DelegateHttpRequestError errCallback = null)
        {
            HttpRawProto httpRawProto = new HttpRawProto
            {
                Url = url,
                RawProto = ProtokitUtil.Instance.GetRawPorto(msg),
                Method = HttpMethod.Post
            };
            if (callback != null)
                RequestResponseHandlerMap.Add(httpRawProto.RawProto.Passthrough, callback);
            if (errCallback != null)
                RequestErrorHandlerMap.Add(httpRawProto.RawProto.Passthrough, errCallback);
            SequentialRawProtos.Enqueue(httpRawProto);
        }

        public void PostRequestBytes(string url, byte[] sendData, bool waitResponse = true)
        {
            HttpProtoRequest request = new HttpProtoRequest
            {
                SendData = sendData,
                Url = url,
                Method = HttpMethod.Post
            };
            SequentialRequests.Enqueue(request);
        }

        /// <summary>
        /// 注册对某一类型消息的监听函数
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="handler"></param>
        public void RegisterMessageHandler(string uri, Action<IMessage> handler)
        {
            if (MessageHandlersMap.ContainsKey(uri))
                MessageHandlersMap[uri] += handler;
            else
                MessageHandlersMap.Add(uri, handler);
        }

        /// <summary>
        /// 移除对某一类型消息的监听函数
        /// </summary>
        /// <param name="uri">消息名称</param>
        /// <param name="handler">消息处理回调</param>
        public void RemoveMessageHandler(string uri, Action<IMessage> handler)
        {
            if (MessageHandlersMap.ContainsKey(uri))
            {
                MessageHandlersMap[uri] -= handler;
                if (MessageHandlersMap[uri] == null)
                    MessageHandlersMap.Remove(uri);
            }
        }

        public void Update()
        {
            Evt_Update?.Invoke();
            //UpdateSequentialRawProto();
            UpdateSequentialRequest();
        }

        private void UpdateSequentialRawProto()
        {
            if (EnableBatchRequest)
            {
                if (SequentialRawProtos.Count > 0)
                {
                    HttpProtoRequest request = new HttpProtoRequest();
                    RawPacket rp = new RawPacket
                    {
                        Version = 1,
                        SequenceID = ProtokitUtil.Instance.GetSequenceId()
                    };
                    request.SequenceId = rp.SequenceID;
                    RequestBatchRecord batchRecordItem = ProtokitUtil.Instance.GetRequestBatchRecord();
                    batchRecordItem.SequenceID = rp.SequenceID;
                    int batchCount = 0;
                    while (SequentialRawProtos.Count > 0 && batchCount < MaxBatchCount)
                    {
                        var httpRawProto = SequentialRawProtos.Peek();
                        if (batchCount == 0)
                        {
                            request.Url = httpRawProto.Url;
                            request.Method = httpRawProto.Method;
                            httpRawProto = SequentialRawProtos.Dequeue();
                            request.PassthroughList.Add(httpRawProto.RawProto.Passthrough);
                        }
                        else
                        {
                            //如果单个请求地址或者方法与合并请求的地址或方法不同不同，则不合并
                            if (!request.Url.Equals(httpRawProto.Url) || !request.Method.Equals(httpRawProto.Method))
                                break;
                            else
                            {
                                httpRawProto = SequentialRawProtos.Dequeue();
                                request.PassthroughList.Add(httpRawProto.RawProto.Passthrough);
                            }
                        }
                        RawAny rawAny = new RawAny
                        {
                            Uri = httpRawProto.RawProto.MsgUri,
                            Raw = httpRawProto.RawProto.MsgRaw,
                            PassThrough = httpRawProto.RawProto.Passthrough
                        };
                        rp.RawAny.Add(rawAny);
                        batchRecordItem.Requests.Add(rawAny.PassThrough, rawAny.Uri);
                        batchRecordItem.SendLog.Enqueue($"[HTTP][Send] {httpRawProto.RawProto.ProtoLog}");
                        batchCount++;
                    }
                    RequestBatchRecordMap.Add(batchRecordItem.SequenceID, batchRecordItem);
                    using (var stream = new MemoryStream())
                    {
                        rp.WriteTo(stream);
                        byte[] data = stream.ToArray();
                        request.SendData = data;
                    }
                    SequentialRequests.Enqueue(request);
                }
            }
            else
            {
                while (SequentialRawProtos.Count > 0)
                {
                    HttpProtoRequest request = new HttpProtoRequest();
                    RawPacket rp = new RawPacket
                    {
                        Version = 1,
                        SequenceID = ProtokitUtil.Instance.GetSequenceId()
                    };
                    request.SequenceId = rp.SequenceID;
                    RequestBatchRecord batchRecordItem = ProtokitUtil.Instance.GetRequestBatchRecord();
                    batchRecordItem.SequenceID = rp.SequenceID;
                    var httpRawProto = SequentialRawProtos.Dequeue();
                    request.PassthroughList.Add(httpRawProto.RawProto.Passthrough);
                    RawAny rawAny = new RawAny
                    {
                        Uri = httpRawProto.RawProto.MsgUri,
                        Raw = httpRawProto.RawProto.MsgRaw,
                        PassThrough = httpRawProto.RawProto.Passthrough
                    };
                    rp.RawAny.Add(rawAny);
                    batchRecordItem.Requests.Add(rawAny.PassThrough, rawAny.Uri);
                    batchRecordItem.SendLog.Enqueue($"[HTTP][Send] {httpRawProto.RawProto.ProtoLog}");
                    RequestBatchRecordMap.Add(batchRecordItem.SequenceID, batchRecordItem);
                    using (var stream = new MemoryStream())
                    {
                        rp.WriteTo(stream);
                        byte[] data = stream.ToArray();
                        request.Url = httpRawProto.Url;
                        request.Method = httpRawProto.Method;
                        request.SendData = data;
                    }
                    SequentialRequests.Enqueue(request);
                }
            }
        }

        private void UpdateSequentialRequest()
        {
            if (!EnableSendRequest)
                return;
            if (CurRequest != null)
            {
                if (CurRequest.State == ProtoHttpRequestState.Reqeusting)
                {
                    if (!CurRequest.RequestOutExpectTimeInvoked && IsRequestOutExpectTime(CurRequest))
                    {
                        CurRequest.RequestOutExpectTimeInvoked = true;
                        OnResponseOutExpectTime();
                    }
                }
                else if (CurRequest.State == ProtoHttpRequestState.Reqeusted)
                {
                    OnRequestEnd();
                    if (EnableAutoRetry && CurRequest.RequestCount <= MaxRetry)
                    {
                        OnRequestBegin();
                        if (Scheme == HttpScheme.HttpClient)
                            SendRequestAsync(CurRequest);
                        else if (Scheme == HttpScheme.UnityWebRequest)
                            SendRequestCoroutine(CurRequest);
                    }
                    else
                    {
                        LastFailedRequest = CurRequest;
                        CurRequest = null;
                        EnableSendRequest = false;
                        OnRequestFailed(LastFailedRequest);
                    }
                }
                else if (CurRequest.State == ProtoHttpRequestState.Responsed)
                {
                    OnRequestEnd();
                    //RequestPool.Recycle(CurRequest);
                    CurRequest = null;
                }
            }
            else
            {
                UpdateSequentialRawProto();
                if (SequentialRequests.Count > 0)
                {
                    CurRequest = SequentialRequests.Dequeue();
                    OnRequestBegin();
                    if (Scheme == HttpScheme.HttpClient)
                        SendRequestAsync(CurRequest);
                    else if (Scheme == HttpScheme.UnityWebRequest)
                        SendRequestCoroutine(CurRequest);
                }
            }
        }

        private bool IsRequestOutExpectTime(HttpProtoRequest request)
        {
            if (request.EndTime == 0)
            {
                if (Time.realtimeSinceStartup - request.StartTime > timeExpect)
                    return true;
            }
            else
            {
                if (request.EndTime - request.StartTime > timeExpect)
                    return true;
            }
            return false;
        }

        private async void SendRequestAsync(HttpProtoRequest request)
        {
            try
            {
                request.State = ProtoHttpRequestState.Reqeusting;
                request.RequestOutExpectTimeInvoked = false;
                request.StartTime = Time.realtimeSinceStartup;
                request.EndTime = 0;
                request.ResultCode = 0;
                request.RequestCount++;
                HttpRequestMessage req = new HttpRequestMessage(request.Method, request.Url)
                {
                    Content = new ByteArrayContent(request.SendData)
                };
                var Header = ProtokitUtil.Instance.GetHttpRequestHeader(request, HttpToken);
                if (Header != null)
                {
                    foreach (var p in Header)
                    {
                        if (p.Key.Equals("Content-Type"))
                        {
                            req.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(p.Value);
                        }
                        else
                        {
                            req.Content.Headers.TryAddWithoutValidation(p.Key, p.Value);
                        }
                    }
                }
                if (EnableProtoLog && RequestBatchRecordMap.ContainsKey(request.SequenceId))
                    RequestBatchRecordMap[request.SequenceId].TraceLog(Logger.Value);
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeOutms);
                HttpResponseMessage rsp = await HttpAgent.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationTokenSource.Token);
                request.ResultCode = (int)rsp.StatusCode;
                if (rsp.IsSuccessStatusCode)
                {
                    byte[] retData = await rsp.Content.ReadAsByteArrayAsync();
                    Dictionary<string, string> headers = new Dictionary<string, string>();
                    var pe = rsp.Headers.GetEnumerator();
                    while (pe.MoveNext())
                    {
                        var headerKey = pe.Current.Key;
                        var headerValues = pe.Current.Value;
                        var ve = headerValues.GetEnumerator();
                        ve.MoveNext();
                        if (!string.IsNullOrEmpty(ve.Current))
                            headers.Add(headerKey, ve.Current);
                    }
                    ResponseHeaderMap[request.SequenceId] = headers;
                    request.State = ProtoHttpRequestState.Responsed;
                    request.EndTime = Time.realtimeSinceStartup;
                    ClearRequestErrorHandler(request);
                    OnResponseSuccess(retData);
                }
                else
                {
                    request.State = ProtoHttpRequestState.Reqeusted;
                    OnResponseFailed(request.ResultCode, rsp.StatusCode.ToString());
                    Logger.Value?.Warn($"request failed. request url:{request.Url}, statusCode:{request.ResultCode}, requestCount:{request.RequestCount}");
                }
            }
            catch (Exception e)
            {
                request.State = ProtoHttpRequestState.Reqeusted;
                string exceptionType = "UnknownError";
                if (e is ArgumentNullException)
                {
                    exceptionType = "ArgumentNull";
                }
                else if (e is InvalidOperationException)
                {
                    exceptionType = "InvalidOperation";
                }
                else if (e is HttpRequestException)
                {
                    if (e.InnerException is WebException)
                    {
                        WebException webException = e.InnerException as WebException;
                        exceptionType = webException.Status.ToString();
                        request.ResultCode = (int)webException.Status;
                    }
                    else
                    {
                        HttpRequestException httpRequestException = e as HttpRequestException;
                        request.ResultCode = httpRequestException.HResult;
                    }
                }
                else if (e is TaskCanceledException)
                {
                    request.ResultCode = (int)HttpStatusCode.RequestTimeout;
                    exceptionType = "RequestTimeout";
                }
                OnResponseFailed(request.ResultCode, exceptionType);
                Logger.Value?.Warn($"request throw exception:{e.Message}, url:{request.Url}, ExceptionType:{exceptionType}, statuCode:{request.ResultCode}, requestCount:{request.RequestCount} , StackTrace:{e.StackTrace}");
            }
        }

        private void SendRequestCoroutine(HttpProtoRequest request)
        {
            try
            {
                Mono.StartCoroutine(PostProtoCoroutine(request));
            }
            catch (Exception e)
            {
                request.State = ProtoHttpRequestState.Reqeusted;
                string exceptionType = "UnknownError";
                if (e is ArgumentNullException)
                {
                    exceptionType = "ArgumentNull";
                }
                else if (e is InvalidOperationException)
                {
                    exceptionType = "InvalidOperation";
                }
                else if (e is HttpRequestException)
                {
                    if (e.InnerException is WebException)
                    {
                        WebException webException = e.InnerException as WebException;
                        exceptionType = webException.Status.ToString();
                        request.ResultCode = (int)webException.Status;
                    }
                    else
                    {
                        HttpRequestException httpRequestException = e as HttpRequestException;
                        request.ResultCode = httpRequestException.HResult;
                    }
                }
                else if (e is TaskCanceledException)
                {
                    request.ResultCode = (int)HttpStatusCode.RequestTimeout;
                    exceptionType = "RequestTimeout";
                }
                OnResponseFailed(request.ResultCode, exceptionType);
                Logger.Value?.Warn($"request throw exception:{e.Message}, url:{request.Url}, ExceptionType:{exceptionType}, statuCode:{request.ResultCode}, requestCount:{request.RequestCount} , StackTrace:{e.StackTrace}");
            }
        }

        private IEnumerator PostProtoCoroutine(HttpProtoRequest request)
        {
            request.State = ProtoHttpRequestState.Reqeusting;
            request.RequestOutExpectTimeInvoked = false;
            request.StartTime = Time.realtimeSinceStartup;
            request.EndTime = 0;
            request.ResultCode = 0;
            request.RequestCount++;
            UnityWebRequest webReq = new UnityWebRequest(request.Url, "POST")
            {
                uploadHandler = new UploadHandlerRaw(request.SendData)
            };
            webReq.timeout = timeOutms / 1000;
            webReq.useHttpContinue = UseHttpContinue;
            var Header = ProtokitUtil.Instance.GetHttpRequestHeader(request, HttpToken);
            if (Header != null)
            {
                foreach (var p in Header)
                {
                    if (!p.Key.Equals("Connection") && !p.Key.Equals("Content-Length") && !string.IsNullOrEmpty(p.Value))
                    {
                        webReq.SetRequestHeader(p.Key, p.Value);
                    }
                }
            }
            webReq.downloadHandler = new DownloadHandlerBuffer();
            if (EnableProtoLog && RequestBatchRecordMap.ContainsKey(request.SequenceId))
                RequestBatchRecordMap[request.SequenceId].TraceLog(Logger.Value);
            yield return webReq.SendWebRequest();
            request.ResultCode = (int)webReq.responseCode;
            if (webReq.isNetworkError || webReq.isHttpError)
            {
                request.State = ProtoHttpRequestState.Reqeusted;
                OnResponseFailed(request.ResultCode, webReq.error);
                Logger.Value?.Warn($"request failed. request url:{request.Url}, statusCode:{request.ResultCode}, requestCount:{request.RequestCount}, error:{webReq.error}");
            }
            else
            {
                byte[] retData = webReq.downloadHandler.data;
                Dictionary<string, string> headers = webReq.GetResponseHeaders();
                ResponseHeaderMap[request.SequenceId] = headers;
                request.State = ProtoHttpRequestState.Responsed;
                request.EndTime = Time.realtimeSinceStartup;
                ClearRequestErrorHandler(request);
                OnResponseSuccess(retData);
            }
        }

        private void OnResponseSuccess(byte[] data)
        {
            RawPacket rp;
            try
            {
                rp = RawPacket.Parser.ParseFrom(data);
            }
            catch (Exception e)
            {
                Logger.Value?.ErrorFormat("Exception throw :{0} on parsing data, stack:{1}", e.Message, e.StackTrace);
                return;
            }
            if (rp.Metadata.Count > 0)
            {
                if (rp.Metadata.Count % 2 == 0)
                {
                    Dictionary<string, string> metadata = new Dictionary<string, string>();
                    for (int i = 0; i < rp.Metadata.Count; i = i + 2)
                    {
                        string key = rp.Metadata[i];
                        string value = rp.Metadata[i + 1];
                        metadata[key] = value;
                    }
                    ResponseMetadataMap[rp.SequenceID] = metadata;
                }
                else
                    Logger.Value?.Warn("metadata key-value is not in pair.");
            }
            for (int i = 0; i < rp.RawAny.Count; i++)
            {
                string name = rp.RawAny[i].Uri;
                byte[] rawdata = rp.RawAny[i].Raw.ToByteArray();
                string passthrough = rp.RawAny[i].PassThrough;
                RecvMsg(rp.SequenceID, passthrough, name, rawdata);
            }
            Evt_RecvPackFinish?.Invoke(rp.SequenceID);
            ResponseHeaderMap.Remove(rp.SequenceID);
            if (ResponseMetadataMap.ContainsKey(rp.SequenceID))
                ResponseMetadataMap.Remove(rp.SequenceID);
            ClearSequenceHandler(rp.SequenceID);
        }

        private void OnResponseFailed(int statusCode, string statusName)
        {
            Evt_RspFailed?.Invoke(statusCode, statusName);
        }

        /// <summary>
        /// 超出预期请求时间，可能需要显示转圈界面，当请求结束时关闭转圈界面
        /// </summary>
        private void OnResponseOutExpectTime()
        {
            Evt_ReqTimeOutExpect?.Invoke();
        }

        /// <summary>
        /// 请求开始
        /// </summary>
        private void OnRequestBegin()
        {
            Evt_ReqBegin?.Invoke();
        }

        /// <summary>
        /// 请求结束
        /// </summary>
        private void OnRequestEnd()
        {
            Evt_ReqEnd?.Invoke();
        }

        /// <summary>
        /// 请求达到重试次数上限仍未成功，可弹窗提示用户确认网络状况后手动重试(调用Retry)，或者停止HttpClient的所有请求(调用Stop)后退回登录界面
        /// </summary>
        private void OnRequestFailed(HttpProtoRequest request)
        {
            if (request != null)
            {
                for (int i = 0; i < request.PassthroughList.Count; i++)
                {
                    var passthrough = request.PassthroughList[i];
                    if (RequestErrorHandlerMap.ContainsKey(passthrough))
                    {
                        RequestErrorHandlerMap[passthrough].Invoke(request.ResultCode);
                        RequestErrorHandlerMap.Remove(passthrough);
                    }
                }
            }
            Evt_ReqFailed?.Invoke();
        }

        private void RecvMsg(int sequenceId, string passthrough, string uri, byte[] data)
        {
            try
            {
                sb.Length = 0;
                sb.AppendFormat("[HTTP][Recv] name:{0}", uri);
                if (MessageHandlersMap.ContainsKey(uri))
                {
                    var parser = ProtokitUtil.Instance.GetParser(uri);
                    if (parser != null)
                    {
                        var message = parser.ParseFrom(data);
                        sb.AppendFormat(", body:{0}", message);
                        MessageHandlersMap[uri].Invoke(message);
                    }
                    else
                        Logger.Value?.Warn($"can't find message parser, uri={uri}");
                }
                if (ReqeustMsgHandlerMap.ContainsKey(passthrough))
                {
                    var parser = ProtokitUtil.Instance.GetParser(uri);
                    if (parser != null)
                    {
                        var message = parser.ParseFrom(data);
                        sb.AppendFormat(", body:{0}", message);
                        ReqeustMsgHandlerMap[passthrough].Invoke(message);
                    }
                    else
                        Logger.Value?.Warn($"Can't find request parser, uri={uri}, sequenceId={sequenceId}, passthrough={passthrough}");
                    ReqeustMsgHandlerMap.Remove(passthrough);
                }
                if (RequestResponseHandlerMap.ContainsKey(passthrough))
                {
                    var parser = ProtokitUtil.Instance.GetParser(uri);
                    if (parser != null)
                    {
                        var message = parser.ParseFrom(data);
                        sb.AppendFormat(", body:{0}", message);
                        Dictionary<string, string> headers = null;
                        if (ResponseHeaderMap.ContainsKey(sequenceId))
                            headers = ResponseHeaderMap[sequenceId];
                        Dictionary<string, string> metadata = new Dictionary<string, string>();
                        if (ResponseMetadataMap.ContainsKey(sequenceId))
                            metadata = ResponseMetadataMap[sequenceId];
                        RequestResponseHandlerMap[passthrough].Invoke(message, headers, metadata);
                    }
                    else
                        Logger.Value?.Warn($"Can't find request parser, uri={uri}, sequenceId={sequenceId}, passthrough={passthrough}");
                    RequestResponseHandlerMap.Remove(passthrough);
                }
                if (RequestBytesHandlerMap.ContainsKey(passthrough))
                {
                    RequestBytesHandlerMap[passthrough].Invoke(uri, data);
                    RequestBytesHandlerMap.Remove(passthrough);
                }
                if (uri.Equals(ErrorResponse.Descriptor.FullName))
                {
                    ErrorResponse errMsg = ErrorResponse.Parser.ParseFrom(data);
                    sb.AppendFormat(", body:{0}", errMsg);
                    if (!errMsg.LogicException)
                    {
                        RecvCommonError(sequenceId, passthrough, errMsg);
                    }
                }
                if (EnableProtoLog)
                    Logger.Value?.Debug(sb.ToString());
                Evt_RecvMsg?.Invoke(sequenceId, passthrough, uri, data);
            }
            catch (Exception e)
            {
                Logger.Value?.Error($"RecvMsg catch exception : {e.Message}, sequenceId={sequenceId}, passthrough={passthrough}, uri={uri}, stack={e.StackTrace}");
            }
        }

        private void RecvCommonError(int sequenceId, string passthrough, ErrorResponse msg)
        {
            string ErrReqUri = string.Empty;
            if (RequestBatchRecordMap.ContainsKey(sequenceId))
            {
                if (RequestBatchRecordMap[sequenceId].Requests.ContainsKey(passthrough))
                    ErrReqUri = RequestBatchRecordMap[sequenceId].Requests[passthrough];
            }
            Logger.Value?.Warn($"receive common error. sequenceId={sequenceId}, passthrough={passthrough}, errorCode={msg.Code}, errorMsg={msg.Message}, errorReq={ErrReqUri}");
            Evt_RecvCommonError?.Invoke(msg.Code, msg.Message);
        }

        private void ClearSequenceHandler(int sequenceId)
        {
            if (RequestBatchRecordMap.ContainsKey(sequenceId))
            {
                var batch = RequestBatchRecordMap[sequenceId];
                var e = batch.Requests.GetEnumerator();
                while (e.MoveNext())
                {
                    ReqeustMsgHandlerMap.Remove(e.Current.Key);
                }
                RequestBatchRecordMap.Remove(sequenceId);
                ProtokitUtil.Instance.RecycleRequestBatch(batch);
            }
        }

        public void Stop()
        {
            HttpAgent.CancelPendingRequests();
            SequentialRequests.Clear();
            SequentialRawProtos.Clear();
            ReqeustMsgHandlerMap.Clear();
            RequestBatchRecordMap.Clear();
            RequestResponseHandlerMap.Clear();
            ResponseHeaderMap.Clear();
            ResponseMetadataMap.Clear();
            RequestErrorHandlerMap.Clear();
            CurRequest = null;
            LastFailedRequest = null;
            EnableSendRequest = false;
        }

        public bool Retry()
        {
            if (LastFailedRequest == null)
            {
                Logger.Value?.Warn("手动重发请求失败，找不到上次发送失败的请求");
                return false;
            }
            else if (CurRequest != null)
            {
                Logger.Value?.Warn("手动重发请求失败，当前已有新的请求被发送过");
                return false;
            }
            else
            {
                CurRequest = LastFailedRequest;
                EnableSendRequest = true;
                OnRequestBegin();
                if (Scheme == HttpScheme.HttpClient)
                    SendRequestAsync(CurRequest);
                else if (Scheme == HttpScheme.UnityWebRequest)
                    SendRequestCoroutine(CurRequest);
                return true;
            }
        }

        private void ClearRequestErrorHandler(HttpProtoRequest request)
        {
            if (request != null)
            {
                for (int i = 0; i < request.PassthroughList.Count; i++)
                {
                    var passthrough = request.PassthroughList[i];
                    if (RequestErrorHandlerMap.ContainsKey(passthrough))
                        RequestErrorHandlerMap.Remove(passthrough);
                }
            }
        }
    }

    public enum HttpScheme
    {
        HttpClient = 1,
        UnityWebRequest = 2,
    }
}