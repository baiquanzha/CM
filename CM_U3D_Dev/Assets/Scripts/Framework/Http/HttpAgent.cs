using UnityEngine;
using System;
using System.Text;
using System.Net.Http;
using System.Collections.Generic;

namespace MTool.Framework.Http
{
    public class HttpAgent
    {
        private HttpClient httpClient = new HttpClient();
        private StringBuilder getUrlBuilder = new StringBuilder();

        public void SetTimeOutSecond(float t)
        {
            if (t > 15)
                httpClient.Timeout = TimeSpan.FromSeconds(t);
            else
                httpClient.Timeout = TimeSpan.FromSeconds(15);
        }

        internal void Post(string url, byte[] postData, IDictionary<string, string> header, Action<byte[]> callback)
        {
            PostBytes(url, postData, header, callback);
        }

        private async void PostBytes(string url, byte[] postData, IDictionary<string, string> header, Action<byte[]> callback)
        {
            try
            {
                ByteArrayContent content = new ByteArrayContent(postData);
                if (header != null)
                {
                    foreach (var p in header)
                    {
                        if (p.Key.Equals("Content-Type"))
                        {
                            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(p.Value);
                        }
                        else
                        {
                            content.Headers.TryAddWithoutValidation(p.Key, p.Value);
                        }
                    }
                }
                HttpResponseMessage res = await httpClient.PostAsync(url, content);
                if (res.IsSuccessStatusCode)
                {
                    byte[] retData = await res.Content.ReadAsByteArrayAsync();
                    callback?.Invoke(retData);
                }
                else
                {
                    Debug.LogWarningFormat("http post response not success, code:{0}, url:{1}", res.StatusCode.ToString(), url);
                    callback?.Invoke(null);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat("http post throw exception:{0}, url:{1} StackTrace: {2}", e.Message, url,e.StackTrace);
                callback?.Invoke(null);
            }
        }

        internal void Get(string url, IDictionary<string, string> args, IDictionary<string, string> header, Action<byte[]> callback)
        {
            GetByArgs(url, args, header, callback);
        }

        private async void GetByArgs(string url, IDictionary<string, string> args, IDictionary<string, string> header, Action<byte[]> callback)
        {
            try
            {
                string reqUrl = BuildRequestUrl(url, args);
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, reqUrl);
                if (header != null)
                {
                    foreach (var p in header)
                    {
                        requestMessage.Headers.TryAddWithoutValidation(p.Key, p.Value);
                    }
                }
                HttpResponseMessage res = await httpClient.SendAsync(requestMessage);
                if (res.IsSuccessStatusCode)
                {
                    byte[] retData = await res.Content.ReadAsByteArrayAsync();
                    callback?.Invoke(retData);
                }
                else
                {
                    Debug.LogWarningFormat("http get response not success, code:{0}, url:{1}", res.StatusCode.ToString(), reqUrl);
                    callback?.Invoke(null);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat("http get throw exception:{0}, url:{1}", e.Message, url);
                callback?.Invoke(null);
            }
        }


        private string BuildRequestUrl(string url, IDictionary<string, string> args)
        {
            if (args == null || args.Count == 0)
                return null;
            getUrlBuilder.Length = 0;
            getUrlBuilder.Append(url);
            getUrlBuilder.Append('?');
            var i = 0;
            foreach (var p in args)
            {
                getUrlBuilder.Append(p.Key);
                getUrlBuilder.Append('=');
                getUrlBuilder.Append(p.Value);
                if (i < args.Count - 1)
                {
                    getUrlBuilder.Append('&');
                }
                i++;
            }
            return getUrlBuilder.ToString();
        }

        internal void Dispose()
        {
            httpClient.Dispose();
        }
    }
}