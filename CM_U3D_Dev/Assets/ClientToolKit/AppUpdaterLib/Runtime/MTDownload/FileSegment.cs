using System;
using System.IO;

namespace MTool.AppUpdaterLib.Runtime.MTDownload
{
    public enum SegmentState
    {
        Idle,
        Connecting,
        Downloading,
        Paused,
        Finished,
        Error,
    }

    public class FileSegment
    {
        private long mStartPos;
        private long mEndPos;
        private readonly int mIndex;
        private readonly string mUrl;
        private readonly long mInitialStartPos;
        private Exception mLastError;
        private SegmentState mState;
        private bool mStarted;
        private DateTime mLastReception = DateTime.MinValue;
        private DateTime mLastErrorTime = DateTime.MinValue;
        private double mRate;
        private long mStart;
        private TimeSpan mLeft = TimeSpan.Zero;
        private int mRetriedCount = 0;
        private Stream mInputStream;
        private Stream mOutputStream;

        public static int MaxRetryCount = 3;
        public static int RetryDelay = 1;   // 1 second

        public SegmentState State => mState;

        public FileSegment(int index, long startPos, long endPos)
        {
            mIndex = index;
            mStartPos = startPos;
            mInitialStartPos = startPos;
            mEndPos = endPos;
        }

        public void SetState(SegmentState state)
        {
            mState = state;
            switch (state)
            {
                case SegmentState.Downloading:
                    Start();
                    break;

                case SegmentState.Connecting:
                case SegmentState.Paused:
                case SegmentState.Finished:
                case SegmentState.Error:
                    mRate = 0.0;
                    mLeft = TimeSpan.Zero;
                    break;
            }
        }

        public void Start()
        {
            mStart = mStartPos;
            mLastReception = DateTime.Now;
            mStarted = true;
        }

        public long LeftSize
        {
            get { return mEndPos <= 0 ? 0 : mEndPos - mStartPos; }
        }

        public long TotalSize
        {
            get { return mEndPos <= 0 ? 0 : mEndPos - mInitialStartPos; }
        }

        public long Downloaded
        {
            get { return mStartPos - mInitialStartPos; }
        }

        public float Progress
        {
            get { return mEndPos <= 0 ? 0 : (float)Downloaded / TotalSize; }
        }

        public Stream InputStream
        {
            get { return mInputStream; }
            set { mInputStream = value; }
        }

        public Stream OutputStream
        {
            get { return mOutputStream; }
            set { mOutputStream = value; }
        }

        public Exception LastError
        {
            get { return mLastError; }
            set
            {
                mLastError = value;
                if (value != null)
                    mLastErrorTime = DateTime.Now;
                else
                    mLastErrorTime = DateTime.MinValue;
            }
        }

        public DateTime LastErrorTime => mLastErrorTime;

        public long StartPos
        {
            get { return mStartPos; }
            set { mStartPos = value; }
        }

        public long EndPos
        {
            get { return mEndPos; }
            set { mEndPos = value; }
        }

        public int RetriedCount
        {
            get { return mRetriedCount; }
            set { mRetriedCount = value; }
        }

        public bool CanRetry
        {
            get { return mRetriedCount < MaxRetryCount; }
        }

        public void IncreaseStartPos(long size)
        {
            lock (this)
            {
                DateTime now = DateTime.Now;
                mStartPos += size;

                if (mStarted)
                {
                    TimeSpan ts = now - mLastReception;
                    if (ts.TotalSeconds == 0)
                        return;

                    // bytes per seconds
                    mRate = ((double)(mStartPos - mStart)) / ts.TotalSeconds;

                    if (mRate > 0.0)
                        mLeft = TimeSpan.FromSeconds(LeftSize / mRate);
                    else
                        mLeft = TimeSpan.MaxValue;
                }
                else
                {
                    mStart = mStartPos;
                    mLastReception = now;
                    mStarted = true;
                }
            }
        }
    }
}