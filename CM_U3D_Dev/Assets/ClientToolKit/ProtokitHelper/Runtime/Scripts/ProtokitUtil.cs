using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using Google.Protobuf;
using MTool.Framework.Base;
using MTool.Core.ResourcePools;

namespace ProtokitHelper
{
    public class ProtokitUtil : Singleton<ProtokitUtil>
    {
        private readonly DateTime StartTime = new DateTime(1970, 1, 1);
        private TimeSpan timeSpan = new TimeSpan();
        private int requestCount = 0;
        private int sequenceCount = 0;
        private Dictionary<string, MessageParser> ProtoParserMap = new Dictionary<string, MessageParser>();
        private ResourcePool<RequestBatchRecord> ReqBatchPool = new ResourcePool<RequestBatchRecord>(() => new RequestBatchRecord(), RequestBatchRecord.Init, null);
        private Dictionary<string, string> httpHeader = new Dictionary<string, string>();

        public void Init()
        {
            InitProtoParser();
            InitHttpCommonHeader();
        }

        public void InitHttpCommonHeader()
        {
            httpHeader.Clear();
            httpHeader.Add("Content-Type", "application/x-protobuf");
            httpHeader.Add("Connection", "keep-alive");
        }

        /// <summary>
        /// 设置自定义Http头内容，如果传入value为空，则会移除头中对应的Key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetCustomHeaderContent(string key, string value)
        {
            if (!string.IsNullOrEmpty(key))
            {
                if (!string.IsNullOrEmpty(value))
                    httpHeader[key] = value;
                else
                    httpHeader.Remove(key);
            }
        }

        private List<string> filterAssemblyList = new List<string>();
        public void AddFilterAssemblyName(string assembly)
        {
            filterAssemblyList.Add(assembly);
        }

        public void InitProtoParser()
        {
            ProtoParserMap.Clear();
            var ti = typeof(IMessage);
            var assemble = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in assemble)
            {
                var asmName = asm.GetName().Name;
                bool bFilter = false;
                for (int i = 0; i < filterAssemblyList.Count; i++)
                {
                    if (asmName.Equals(filterAssemblyList[i]))
                    {
                        bFilter = true;
                        break;
                    }
                }
                if (bFilter) continue;
                if (asmName.Equals("GoogleProtobuf") || asmName.Equals("Google.Protobuf") || asmName.StartsWith("UnityEngine") || asmName.StartsWith("UnityEditor")) continue;
                foreach (var t in asm.GetTypes())
                {
                    if (!ti.IsAssignableFrom(t) || !t.IsClass || t.IsAbstract || t.IsGenericType) continue;
                    var prop = t.GetProperty("Parser", BindingFlags.Public | BindingFlags.Static);
                    if (prop == null) continue;
                    if (!(prop.GetValue(null, null) is MessageParser parser)) continue;
                    var p = (IMessage)Activator.CreateInstance(t);
                    ProtoParserMap[p.Descriptor.FullName] = parser;
                    //var typeFullName = t.FullName;
                    //UnityEngine.Debug.Log("add parser:" + p.Descriptor.FullName + ", type full name:" + typeFullName + ", assembly:" + asmName);
                }
            }
        }

        public MessageParser GetParser(string uri)
        {
            if (ProtoParserMap.ContainsKey(uri))
                return ProtoParserMap[uri];
            return null;
        }

        public void AddParser(string uri, MessageParser parser)
        {
            if (!ProtoParserMap.ContainsKey(uri))
                ProtoParserMap.Add(uri, parser);
        }

        public string NextPassthrough()
        {
            timeSpan = (DateTime.UtcNow - StartTime);
            return string.Concat(timeSpan.ToString(), "_", NextRequestId().ToString());
        }

        public int NextRequestId()
        {
            if (requestCount >= 2147483647)
                requestCount = 1;
            requestCount++;
            return requestCount;
        }

        public int GetSequenceId()
        {
            if (sequenceCount >= 2147483647)
                sequenceCount = 1;
            sequenceCount++;
            return sequenceCount;
        }

        public RequestBatchRecord GetRequestBatchRecord()
        {
            return ReqBatchPool.Obtain();
        }

        public void RecycleRequestBatch(RequestBatchRecord item)
        {
            ReqBatchPool.Recycle(item);
        }

        public RawProto GetRawPorto(IMessage msg, Dictionary<string, string> metadata = null)
        {
            using (var stream = new MemoryStream())
            {
                msg.WriteTo(stream);
                var rawProto = new RawProto
                {
                    MsgUri = msg.Descriptor.FullName,
                    MsgRaw = ByteString.CopyFrom(stream.ToArray()),
                    Passthrough = NextPassthrough()
                };
                if (metadata != null && metadata.Count > 0)
                {
                    rawProto.Metadata = metadata;
                    rawProto.ProtoLog = $"name:{msg.Descriptor.FullName}, body:{msg}, metadata content:{GetMetadataString(metadata)}";
                }
                else
                    rawProto.ProtoLog = $"name:{msg.Descriptor.FullName}, body:{msg}";
                return rawProto;
            }
        }

        public HttpProtoRequest GetHttpProtoRequest(string url, byte[] data, HttpMethod method)
        {
            HttpProtoRequest request = new HttpProtoRequest
            {
                SendData = data,
                Url = url,
                Method = method
            };
            return request;
        }

        public Dictionary<string, string> GetHttpRequestHeader(HttpProtoRequest request, string httpToken)
        {
            httpHeader["x-fun-user-token"] = httpToken;
            httpHeader["x-fun-request-id"] = request.SequenceId.ToString();
            httpHeader["Content-Length"] = request.SendData.Length.ToString();
            return httpHeader;
        }

        private StringBuilder sb = new StringBuilder();
        private string GetMetadataString(Dictionary<string, string> metadata)
        {
            sb.Length = 0;
            sb.Append("{");
            sb.Append($"count is {metadata.Count}");
            var e = metadata.GetEnumerator();
            while (e.MoveNext())
            {
                sb.Append(", k-v pair:[");
                sb.Append($"{e.Current.Key}:{e.Current.Value}");
                sb.Append("]");
            }
            sb.Append("}");
            return sb.ToString();
        }
    }

    public sealed class RawProto
    {
        public string MsgUri;
        public ByteString MsgRaw;
        public string Passthrough;
        public string ProtoLog;
        public Dictionary<string, string> Metadata;
    }

    public sealed class RequestBatchRecord
    {
        public int SequenceID;
        /// <summary>
        /// key:passthrough, value:msg uri
        /// </summary>
        public Dictionary<string, string> Requests = new Dictionary<string, string>(32);
        /// <summary>
        /// 发送日志
        /// </summary>
        public Queue<string> SendLog = new Queue<string>();

        public static void Init(RequestBatchRecord item)
        {
            item.SequenceID = 0;
            item.Requests.Clear();
            item.SendLog.Clear();
        }

        public void TraceLog(MTool.LoggerModule.Runtime.ILogger logger)
        {
            while (SendLog.Count > 0)
            {
                var log = SendLog.Dequeue();
                logger?.Debug(log);
            }
        }
    }

    public enum ProtoHttpRequestState
    {
        None = 0,
        Reqeusting,
        Reqeusted,
        Responsed,
    }

    public class HttpRawProto
    {
        public string Url;
        public RawProto RawProto;
        public HttpMethod Method;
    }

    public class HttpProtoRequest
    {
        /// <summary>
        /// 请求地址
        /// </summary>
        public string Url;
        /// <summary>
        /// Http方法
        /// </summary>
        public HttpMethod Method;
        /// <summary>
        /// 请求状态
        /// </summary>
        public ProtoHttpRequestState State;
        /// <summary>
        /// 发送数据
        /// </summary>
        public byte[] SendData = new byte[0];
        /// <summary>
        /// 请求开始时间
        /// </summary>
        public float StartTime;
        /// <summary>
        /// 请求结束时间
        /// </summary>
        public float EndTime;
        /// <summary>
        /// 请求次数
        /// </summary>
        public int RequestCount;
        /// <summary>
        /// 返回状态码
        /// </summary>
        public int ResultCode;
        /// <summary>
        /// 标记是否已经调用过超出预期时间的委托
        /// </summary>
        public bool RequestOutExpectTimeInvoked;
        /// <summary>
        /// 请求序列号
        /// </summary>
        public int SequenceId;
        /// <summary>
        /// 请求包含的所有协议的Passthrough，对于非Queue模式，List中只有一个协议的Passthrough
        /// </summary>
        public List<string> PassthroughList = new List<string>(10);

        //public static void Init(HttpProtoRequest item)
        //{
        //    item.Url = string.Empty;
        //    item.State = ProtoHttpRequestState.None;
        //    item.SendData = new byte[0];
        //    item.StartTime = 0;
        //    item.EndTime = 0;
        //    item.RequestCount = 0;
        //    item.ResultCode = 0;
        //    item.RequestOutExpectTimeInvoked = false;
        //    item.SequenceId = 0;
        //}
    }
}