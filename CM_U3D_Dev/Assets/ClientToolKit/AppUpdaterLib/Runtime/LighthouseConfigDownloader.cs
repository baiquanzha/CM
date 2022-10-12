using MTool.LoggerModule.Runtime;
using System;
using System.Text;
using ILogger = MTool.LoggerModule.Runtime.ILogger;

namespace MTool.AppUpdaterLib.Runtime
{
    internal sealed class LighthouseConfigDownloader : IDisposable
    {
        #region Enums

        public enum InnerState
        {
            Idle,

            ReqLighthouseConfigFromCdn,

            RequestingCdn,

            RequestLighthouseConfigFromOss,

            RequestingOss,

            ReqLighthouseConfigFailure,
        }

        #endregion

        //--------------------------------------------------------------

        #region Fields

        //--------------------------------------------------------------

        private static readonly ILogger s_mLogger = LoggerManager.GetLogger("LighthouseConfigDownloader");

        private static Encoding m_Encoding = new UTF8Encoding(false, true);

        private InnerState mState = InnerState.Idle;

        /// <summary>
        /// 需要请求的lighthouse配置的id
        /// </summary>
        private string mTargetLighthouseId = string.Empty;

        /// <summary>
        /// 请求结果返回
        /// </summary>
        private Action<bool, string> mReqLighthouseConfigEvent = null;

        private AppUpdaterContext mContext;

        /// <summary>
        /// http下载组件
        /// </summary>
        private HttpRequest mHttpComponnent;

        #endregion

        //--------------------------------------------------------------

        #region Properties & Events

        //--------------------------------------------------------------

        public FileServerType CurRequestFileServerType { private set; get; } = FileServerType.CDN;

        #endregion

        //--------------------------------------------------------------

        #region Creation & Cleanup

        //--------------------------------------------------------------

        public LighthouseConfigDownloader(AppUpdaterContext context, HttpRequest httpComponnent)
        {
            this.mContext = context;
            this.mHttpComponnent = httpComponnent;
        }

        #endregion

        //--------------------------------------------------------------

        #region Methods

        //--------------------------------------------------------------

        public void Update()
        {
            switch (mState)
            {
                case InnerState.ReqLighthouseConfigFromCdn:
                    this.RequestLighthouseConfig(FileServerType.CDN);
                    break;
                case InnerState.RequestLighthouseConfigFromOss:
                    this.RequestLighthouseConfig(FileServerType.OSS);
                    break;
                case InnerState.ReqLighthouseConfigFailure:
                    break;
            }
        }


        public void StartGetLighthouseConfigFromRemote(Action<bool,string> callback, string lighthouseId = null)
        {
            this.Clear();

            this.mReqLighthouseConfigEvent = callback;

            this.mTargetLighthouseId = lighthouseId;

            this.mState = InnerState.ReqLighthouseConfigFromCdn;
        }

        public void StartGetLighthouseConfigFromOss(Action<bool, string> callback)
        {
            this.Clear();
            this.mReqLighthouseConfigEvent = callback;
            this.mState = InnerState.RequestLighthouseConfigFromOss;
        }

        /// <summary>
        /// 请求lighthouse配置
        /// </summary>
        /// <param name="from">
        /// 请求的出处。
        /// 0 ：CDN
        /// 1 ： 
        /// </param>
        private void RequestLighthouseConfig(FileServerType fileServerType)
        {
            string lighthouseUrl = null;

            if (fileServerType == FileServerType.CDN)
            {
                lighthouseUrl = this.mContext.GetLighthouseUrl(this.mTargetLighthouseId, fileServerType);
            }
            else if(fileServerType == FileServerType.OSS)
            {
                /*
                 * 如果是oss访问，则只访问默认的lighthouse，然后对比lighthouseId
                 */
                lighthouseUrl = this.mContext.GetLighthouseUrl(null, fileServerType);
            }
            
            this.CurRequestFileServerType = fileServerType;
            s_mLogger.Info($"Request lighthouseUrl : {lighthouseUrl}");

            this.mState = (fileServerType == FileServerType.CDN) ? InnerState.RequestingCdn : InnerState.RequestingOss;

            this.mHttpComponnent.Load(lighthouseUrl, OnLighthouseConfigResponseRet);
        }

        private void OnLighthouseConfigResponseRet(byte[] netData)
        {
            if (netData == null || netData.Length == 0)
            {
                switch (mState)
                {
                    case InnerState.RequestingCdn:
                        this.mState = InnerState.RequestLighthouseConfigFromOss;
                        break;
                    case InnerState.RequestingOss:
                        this.mState = InnerState.ReqLighthouseConfigFailure;
                        this.mReqLighthouseConfigEvent?.Invoke(false,null);
                        this.Clear();
                        break;
                }
            }
            else
            {
                string lighthouseContents = m_Encoding.GetString(netData);
                this.mReqLighthouseConfigEvent?.Invoke(true,lighthouseContents);
            }
        }


        public void Clear()
        {
            this.mState = InnerState.Idle;
            this.mTargetLighthouseId = string.Empty;
            this.mReqLighthouseConfigEvent = null;
            this.CurRequestFileServerType = FileServerType.CDN;
        }

        public void Dispose()
        {
            this.mState = InnerState.Idle;
            this.CurRequestFileServerType = FileServerType.CDN;
            this.mTargetLighthouseId = string.Empty;
            this.mReqLighthouseConfigEvent = null;
            this.mContext = null;
            this.mHttpComponnent = null;
        }

        #endregion
    }


}
