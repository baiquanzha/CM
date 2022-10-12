using System;
using System.Collections.Generic;

namespace MTool.AppUpdaterLib.Runtime.MTDownload
{
    [Serializable]
    public struct Segment
    {
        private readonly long mStartPosition;
        private readonly long mEndPosition;

        public long StartPosition
        {
            get { return mStartPosition; }
        }

        public long EndPosition
        {
            get { return mEndPosition; }
        }

        public Segment(long mStartPos, long endPos)
        {
            this.mEndPosition = endPos;
            this.mStartPosition = mStartPos;
        }
    }

    public class SegmentCalculator
    {
        public static long MinSegmentSize = 100000;

        public static Segment[] GetSegments(int segmentCount, long remoteFileSize)
        {
            long minSize = MinSegmentSize;
            long segmentSize = remoteFileSize / segmentCount;

            while (segmentCount > 1 && segmentSize < minSize)
            {
                segmentCount--;
                segmentSize = remoteFileSize / segmentCount;
            }

            long startPosition = 0;

            List<Segment> segments = new List<Segment>();

            for (int i = 0; i < segmentCount; i++)
            {
                if (segmentCount - 1 == i)
                {
                    segments.Add(new Segment(startPosition, remoteFileSize));
                }
                else
                {
                    segments.Add(new Segment(startPosition, startPosition + (int)segmentSize));
                }

                startPosition = segments[segments.Count - 1].EndPosition;
            }

            return segments.ToArray();
        }
    }
}