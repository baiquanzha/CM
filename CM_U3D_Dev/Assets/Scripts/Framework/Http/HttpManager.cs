using System;
using System.Collections.Generic;

namespace MTool.Framework.Http
{
    public class HttpManager : FrameworkModule
    {
        public HttpAgent HttpAgent { get; private set; }

        private Dictionary<string, string> HttpArgs = new Dictionary<string, string>(64);
        private Dictionary<string, string> HttpHeads = new Dictionary<string, string>(32);

        internal override void Init()
        {
            HttpAgent = new HttpAgent();
        }

        internal override void Update(float elapseTime, float realElapseTime)
        {
        }

        internal override void LateUpdate()
        {
        }

        internal override void Shutdown()
        {
            HttpAgent?.Dispose();
        }

        /// <summary>
        /// 发送HttpPost请求(CS层使用)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="header"></param>
        /// <param name="postData"></param>
        /// <param name="callback"></param>
        public void PostHttpRequest(string url, Dictionary<string, string> header, byte[] postData, Action<byte[]> callback)
        {
            HttpAgent.Post(url, postData, header, callback);
        }

        /// <summary>
        /// 发送HttpGet请求(CS层使用)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="header"></param>
        /// <param name="args"></param>
        /// <param name="callback"></param>
        public void GetHttpRequest(string url, Dictionary<string, string> header, Dictionary<string, string> args, Action<byte[]> callback)
        {
            HttpAgent.Get(url, args, header, callback);
        }
    }
}