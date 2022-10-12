using System;

namespace MTool.AppUpdaterLib.Runtime.MTDownload
{
    [Serializable]
    public class RemoteFileInfo
    {
        private bool mAcceptRanges;
        private long mFileSize;
        private DateTime mLastModified = DateTime.MinValue;

        private string mimeType;

        public string MimeType
        {
            get { return mimeType; }
            set { mimeType = value; }
        }

        public bool AcceptRanges
        {
            get { return mAcceptRanges; }
            set { mAcceptRanges = value; }
        }

        public long FileSize
        {
            get { return mFileSize; }
            set { mFileSize = value; }
        }

        public DateTime LastModified
        {
            get { return mLastModified; }
            set { mLastModified = value; }
        }
    }
}