using System;

namespace MTool.AppUpdaterLib.Runtime.Utilities
{
    public static class TimeUtility
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        public static readonly DateTime EpochTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        #endregion

        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// 获取但其那
        /// </summary>
        /// <returns></returns>
        public static long GetCurrentTimeSeconds()
        {
            var now = DateTime.Now;

            var diff = now - EpochTime;

            long totalSeconds = 0;

            unchecked
            {
                totalSeconds = (long)diff.TotalSeconds;
            }

            return (long)totalSeconds;
        }

        #endregion

    }
}
