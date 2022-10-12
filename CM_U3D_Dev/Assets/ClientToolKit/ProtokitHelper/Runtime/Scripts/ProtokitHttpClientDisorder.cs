using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using MTool.Framework.Base;
using MTool.LoggerModule.Runtime;
using gen.netutils;
using UnityEngine;
using UnityEngine.Networking;
using ILogger = MTool.LoggerModule.Runtime.ILogger;

namespace ProtokitHelper
{
    public class ProtoKitHttpClientDisorder : Singleton<ProtoKitHttpClientDisorder>
    {
        /// <summary>
        /// 超时时间(毫秒)
        /// </summary>
        private int timeOutms = 15000;

        /// <summary>
        /// 消息预期返回时间
        /// </summary>
        private float timeExpect = 1.0f;

        /// <summary>
        /// user token
        /// </summary>
        public string HttpToken { get; private set; }

        /// <summary>
        /// 是否允许使用queue模式合并请求
        /// </summary>
        public bool EnableBatchRequest { get; private set; } = false;

        /// <summary>
        /// 消息最大合并数
        /// </summary>
        public int MaxBatchCount { get; private set; } = 1;

        /// <summary>
        /// 设置消息发送UnityWebRequest.useHttpContinue
        /// </summary>
        public bool UseHttpContinue { get; private set; } = true;

        private Dictionary<string, Action<IMessage>> ReqeustMsgHandlerMap = new Dictionary<string, Action<IMessage>>();
        private Dictionary<string, Action<IMessage>> MessageHandlersMap = new Dictionary<string, Action<IMessage>>();
        private Dictionary<int, byte[]> ResponseDataMap = new Dictionary<int, byte[]>();
        private Queue<HttpProtoRequest> RequestQueue = new Queue<HttpProtoRequest>();
        private Queue<HttpRawProto> ProtoQueue = new Queue<HttpRawProto>();

        private HttpClient HttpAgent { get; } = new HttpClient();

        private MonoBehaviour Mono;

        public HttpScheme Scheme { get; private set; } = HttpScheme.HttpClient;

        public HttpPipelineState PipelineState { get; private set; } = HttpPipelineState.NoSession;

        private StringBuilder sb = new StringBuilder();

        private static readonly Lazy<ILogger> Logger = new Lazy<ILogger>(() => LoggerManager.GetLogger("ProtoKitHttpClientDisorder"));

        /// <summary>
        /// 是否输出消息收发日志
        /// </summary>
        public bool EnableProtoLog = true;

        /// <summary>
        /// 消息包解析完成
        /// </summary>
        public event DelegateRecvPacketFinish Evt_RecvPackFinish;

        /// <summary>
        /// 返回了错误的状态码
        /// </summary>
        public event DelegateHttpResponseFailed Evt_RspFailed;

        /// <summary>
        /// 消息接收
        /// </summary>
        public event DelegateRecvProto Evt_RecvMsg;

        /// <summary>
        /// 请求返回了底层错误
        /// </summary>
        public event DelegateRecvCommonError Evt_RecvCommonError;

        /// <summary>
        /// 请求超出预期时间
        /// </summary>
        public event Action Evt_ReqTimeOutExpect;

        /// <summary>
        /// 一次请求结束
        /// </summary>
        public event Action Evt_ReqEnd;

        /// <summary>
        /// Update轮询
        /// </summary>
        public event Action Evt_Update;

        private string SessionKey = string.Empty;

        private string ServerKey = string.Empty;

        private string Cookies = string.Empty;

        public void Init(bool queueMode, int batchLimit = 1)
        {
            EnableBatchRequest = queueMode;
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

        public void PostRequestMsg(string url, IMessage msg, Action<IMessage> callback)
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
            if (string.IsNullOrEmpty(SessionKey))
                SessionKey = Guid.NewGuid().ToString();
            if (PipelineState == HttpPipelineState.NoSession)
            {
                HttpProtoRequest request = GetHttpProtoRequest(httpRawProto);
                RequestQueue.Enqueue(request);
                if (EnableProtoLog)
                    Logger.Value?.DebugFormat("[HTTP][Send] {0}", httpRawProto.RawProto.ProtoLog);
                if (Scheme == HttpScheme.HttpClient)
                    SendRequestAsync(request);
                else if (Scheme == HttpScheme.UnityWebRequest)
                    SendRequestCoroutine(request);
            }
            else
            {
                ProtoQueue.Enqueue(httpRawProto);
            }
            //Trace.Instance.debug("[HTTP][Send] {0}", httpRawProto.RawProto.ProtoLog);
            //HttpProtoRequest request = GetHttpProtoRequest(httpRawProto);
            //RequestQueue.Enqueue(request);
            //if (Scheme == HttpScheme.HttpClient)
            //    SendRequestAsync(request);
            //else if (Scheme == HttpScheme.UnityWebRequest)
            //    SendRequestCoroutine(request);
        }

        public void PostRequestBytes(string url, int sequenceId, byte[] data)
        {
            HttpProtoRequest request = new HttpProtoRequest
            {
                SequenceId = sequenceId,
                SendData = data,
                Url = url,
                Method = HttpMethod.Post
            };
            RequestQueue.Enqueue(request);
            if (PipelineState == HttpPipelineState.NoSession)
            {
                if (Scheme == HttpScheme.HttpClient)
                    SendRequestAsync(request);
                else if (Scheme == HttpScheme.UnityWebRequest)
                    SendRequestCoroutine(request);
            }
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

        private HttpProtoRequest GetHttpProtoRequest(HttpRawProto httpRawProto)
        {
            HttpProtoRequest request = new HttpProtoRequest();
            RawPacket rp = new RawPacket
            {
                Version = 1,
                SequenceID = ProtokitUtil.Instance.GetSequenceId()
            };
            request.SequenceId = rp.SequenceID;
            RawAny rawAny = new RawAny
            {
                Uri = httpRawProto.RawProto.MsgUri,
                Raw = httpRawProto.RawProto.MsgRaw,
                PassThrough = httpRawProto.RawProto.Passthrough
            };
            rp.RawAny.Add(rawAny);
            using (var stream = new MemoryStream())
            {
                rp.WriteTo(stream);
                byte[] data = stream.ToArray();
                request.Url = httpRawProto.Url;
                request.Method = httpRawProto.Method;
                request.SendData = data;
            }
            return request;
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
                req.Content.Headers.TryAddWithoutValidation("x-pipeline-sequence-id", request.SequenceId.ToString());
                req.Content.Headers.TryAddWithoutValidation("x-pipeline-session-key", SessionKey);
                if (!string.IsNullOrEmpty(ServerKey))
                    req.Content.Headers.TryAddWithoutValidation("x-pipeline-server-key", ServerKey);
                if (!string.IsNullOrEmpty(Cookies))
                    req.Content.Headers.TryAddWithoutValidation("Cookie", Cookies);
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeOutms);
                if (PipelineState == HttpPipelineState.NoSession)
                    PipelineState = HttpPipelineState.CreateSession;
                HttpResponseMessage rsp = await HttpAgent.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationTokenSource.Token);
                request.ResultCode = (int)rsp.StatusCode;
                if (rsp.IsSuccessStatusCode)
                {
                    byte[] retData = await rsp.Content.ReadAsByteArrayAsync();
                    request.State = ProtoHttpRequestState.Responsed;
                    request.EndTime = Time.realtimeSinceStartup;
                    if (string.IsNullOrEmpty(ServerKey))
                    {
                        rsp.Headers.TryGetValues("x-pipeline-server-key", out IEnumerable<string> list);
                        var e = list.GetEnumerator();
                        e.MoveNext();
                        ServerKey = e.Current;
                    }
                    if (string.IsNullOrEmpty(Cookies))
                    {
                        rsp.Headers.TryGetValues("set-cookie", out IEnumerable<string> list);
                        var e = list.GetEnumerator();
                        e.MoveNext();
                        Cookies = e.Current;
                    }
                    if (PipelineState == HttpPipelineState.CreateSession)
                        PipelineState = HttpPipelineState.ExistSession;
                    //OnResponseSuccess(retData);
                    ResponseDataMap.Add(request.SequenceId, retData);
                }
                else
                {
                    request.State = ProtoHttpRequestState.Reqeusted;
                    OnResponseFailed(request.SequenceId, request.ResultCode, rsp.StatusCode.ToString());
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
                OnResponseFailed(request.SequenceId, request.ResultCode, exceptionType);
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
                OnResponseFailed(request.SequenceId, request.ResultCode, exceptionType);
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
            webReq.SetRequestHeader("x-pipeline-sequence-id", request.SequenceId.ToString());
            webReq.SetRequestHeader("x-pipeline-session-key", SessionKey);
            if (!string.IsNullOrEmpty(ServerKey))
                webReq.SetRequestHeader("x-pipeline-server-key", ServerKey);
            if (!string.IsNullOrEmpty(Cookies))
                webReq.SetRequestHeader("Cookie", Cookies);
            webReq.downloadHandler = new DownloadHandlerBuffer();
            if (PipelineState == HttpPipelineState.CreateSession)
                PipelineState = HttpPipelineState.ExistSession;
            yield return webReq.SendWebRequest();
            request.ResultCode = (int)webReq.responseCode;
            if (webReq.isNetworkError || webReq.isHttpError)
            {
                request.State = ProtoHttpRequestState.Reqeusted;
                OnResponseFailed(request.SequenceId, request.ResultCode, webReq.error);
                Logger.Value?.Warn($"request failed. request url:{request.Url}, statusCode:{request.ResultCode}, requestCount:{request.RequestCount}, error:{webReq.error}");
            }
            else
            {
                byte[] retData = webReq.downloadHandler.data;
                request.State = ProtoHttpRequestState.Responsed;
                request.EndTime = Time.realtimeSinceStartup;
                if (string.IsNullOrEmpty(ServerKey))
                    ServerKey = webReq.GetResponseHeader("x-pipeline-server-key");
                if (string.IsNullOrEmpty(Cookies))
                    Cookies = webReq.GetResponseHeader("set-cookie");
                if (PipelineState == HttpPipelineState.CreateSession)
                    PipelineState = HttpPipelineState.ExistSession;
                //OnResponseSuccess(retData);
                ResponseDataMap.Add(request.SequenceId, retData);
            }
        }

        public void Update()
        {
            Evt_Update?.Invoke();
            UpdateSendRequest();
            while (RequestQueue.Count > 0)
            {
                HttpProtoRequest topRequest = RequestQueue.Peek();
                if (ResponseDataMap.ContainsKey(topRequest.SequenceId))
                {
                    RequestQueue.Dequeue();
                    OnResponseSuccess(ResponseDataMap[topRequest.SequenceId]);
                }
                else
                    break;
            }
            var e = RequestQueue.GetEnumerator();
            while (e.MoveNext())
            {
                var req = e.Current;
                if (req.State == ProtoHttpRequestState.Reqeusting)
                {
                    if (!req.RequestOutExpectTimeInvoked && IsRequestOutExpectTime(req))
                    {
                        req.RequestOutExpectTimeInvoked = true;
                        OnResponseOutExpectTime();
                    }
                }
            }
        }

        private void UpdateSendRequest()
        {
            if (EnableBatchRequest)
            {
                if (ProtoQueue.Count > 0)
                {
                    HttpProtoRequest request = new HttpProtoRequest();
                    RawPacket rp = new RawPacket
                    {
                        Version = 1,
                        SequenceID = ProtokitUtil.Instance.GetSequenceId()
                    };
                    request.SequenceId = rp.SequenceID;
                    int batchCount = 0;
                    while (ProtoQueue.Count > 0 && batchCount < MaxBatchCount)
                    {
                        var httpRawProto = ProtoQueue.Peek();
                        if (batchCount == 0)
                        {
                            request.Url = httpRawProto.Url;
                            request.Method = httpRawProto.Method;
                            httpRawProto = ProtoQueue.Dequeue();
                        }
                        else
                        {
                            if (!request.Url.Equals(httpRawProto.Url) || !request.Method.Equals(httpRawProto.Method))
                                break;
                            else
                                httpRawProto = ProtoQueue.Dequeue();
                        }
                        RawAny rawAny = new RawAny
                        {
                            Uri = httpRawProto.RawProto.MsgUri,
                            Raw = httpRawProto.RawProto.MsgRaw,
                            PassThrough = httpRawProto.RawProto.Passthrough
                        };
                        rp.RawAny.Add(rawAny);
                        batchCount++;
                        if (EnableProtoLog)
                            Logger.Value?.DebugFormat("[HTTP][Send] {0}", httpRawProto.RawProto.ProtoLog);
                    }
                    using (var stream = new MemoryStream())
                    {
                        rp.WriteTo(stream);
                        byte[] data = stream.ToArray();
                        request.SendData = data;
                    }
                    RequestQueue.Enqueue(request);
                }
            }
            else
            {
                while (ProtoQueue.Count > 0)
                {
                    var httpRawProto = ProtoQueue.Dequeue();
                    if (EnableProtoLog)
                        Logger.Value?.DebugFormat("[HTTP][Send] {0}", httpRawProto.RawProto.ProtoLog);
                    HttpProtoRequest request = GetHttpProtoRequest(httpRawProto);
                    RequestQueue.Enqueue(request);
                }
            }
            if (PipelineState == HttpPipelineState.ExistSession)
            {
                var e = RequestQueue.GetEnumerator();
                while (e.MoveNext())
                {
                    var sendReq = e.Current;
                    if (e.Current.State == ProtoHttpRequestState.None)
                    {
                        if (Scheme == HttpScheme.HttpClient)
                            SendRequestAsync(sendReq);
                        else if (Scheme == HttpScheme.UnityWebRequest)
                            SendRequestCoroutine(sendReq);
                    }
                }
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
            for (int i = 0; i < rp.RawAny.Count; i++)
            {
                string name = rp.RawAny[i].Uri;
                byte[] rawdata = rp.RawAny[i].Raw.ToByteArray();
                string passthrough = rp.RawAny[i].PassThrough;
                RecvMsg(rp.SequenceID, passthrough, name, rawdata);
            }
            OnRequestEnd();
            Evt_RecvPackFinish?.Invoke(rp.SequenceID);
            //ClearSequenceHandler(rp.SequenceID);
        }

        private void OnResponseFailed(int requestId, int statusCode, string statusName)
        {
            if (RequestQueue.Count > 0)
            {
                HttpProtoRequest topRequest = RequestQueue.Peek();
                if (topRequest.SequenceId == requestId)
                    RequestQueue.Dequeue();
            }
            OnRequestEnd();
            Evt_RspFailed?.Invoke(statusCode, statusName);
        }

        /// <summary>
        /// 超出预期请求时间，可能需要显示转圈界面，当请求结束时关闭转圈界面
        /// </summary>
        private void OnResponseOutExpectTime()
        {
            Evt_ReqTimeOutExpect?.Invoke();
        }

        private void OnRequestEnd()
        {
            Evt_ReqEnd?.Invoke();
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
                        Logger.Value?.Warn($"ProtokitHttpClient can't find request parser, uri={uri}, sequenceId={sequenceId}, passthrough={passthrough}");
                    ReqeustMsgHandlerMap.Remove(passthrough);
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
                sb.AppendFormat(", sequenceId:{0}, passthrough:{1}", sequenceId, passthrough);
                if (EnableProtoLog)
                    Logger.Value?.Debug(sb.ToString());
                Evt_RecvMsg?.Invoke(sequenceId, passthrough, uri, data);
            }
            catch (Exception e)
            {
                Logger.Value?.Error($"ProtokitHttpClient RecvMsg catch exception : {e.Message}, sequenceId={sequenceId}, passthrough={passthrough}, uri={uri}, stack:{e.StackTrace}");
            }
        }

        private void RecvCommonError(int sequenceId, string passthrough, ErrorResponse msg)
        {
            Evt_RecvCommonError?.Invoke(msg.Code, msg.Message);
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

        public void Stop()
        {
            HttpAgent.CancelPendingRequests();
            ReqeustMsgHandlerMap.Clear();
            ResponseDataMap.Clear();
            RequestQueue.Clear();
            ProtoQueue.Clear();
        }
    }

    public enum HttpPipelineState
    {
        NoSession,
        CreateSession,
        ExistSession
    }
}
