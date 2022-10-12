using System;
using System.Text.RegularExpressions;
using MTool.AppUpdaterLib.Runtime.Exceptions;

namespace MTool.AppUpdaterLib.Runtime
{
    public class Version
    {
        #region Enum

        public enum VersionCompareResult
        {
            UnKnow = -1,

            LowerForMajor,//Major更低

            LowerForMinor,//Major一样，Minor更低

            LowerForPatch,//Major,Minor一样，Patch更低

            Equal,//版本号一样 

            HigherForPatch,//Major，Minor一样，Patch更高

            HigherForMinor,//Major一样，Minor更高

            HigherForMajor, //Major更高
        }


        #endregion

        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        //public const string PATTERN = "^((([1-9][0-9]{0,4})|[0]).){2}(([1-9][0-9]{0,4})|[0])$";
        public const string PATTERN = "^([0-9]{0,4}.){2}[0-9]{0,4}$";//Support 6.3.000


        /// <summary>
        /// 主版本号(必须是数字)
        /// </summary>
        public string Major { private set; get; }

        /// <summary>
        /// 次版本号（必须是数字)
        /// </summary>
        public string Minor { private set; get; }

        /// <summary>
        /// 修订号(必须是数字)
        /// </summary>
        public string Patch = "1";

        /// <summary>
        /// 指示当前版本号是否有效
        /// </summary>
        private bool mIsValid = false;

        #endregion

        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        public int MajorNum
        {
            get
            {
                if(!this.mIsValid)
                    throw new InvalidVersionNumGetOperationException("Major");
                return Convert.ToInt32(this.Major);
            }
        }

        public int MinorNum
        {
            get
            {
                if (!this.mIsValid)
                    throw new InvalidVersionNumGetOperationException("Minor");
                return Convert.ToInt32(this.Minor);
            }
        }

        public int PatchNum
        {
            get
            {
                if (!this.mIsValid)
                    throw new InvalidVersionNumGetOperationException("Patch");
                return Convert.ToInt32(this.Patch);
            }
        }

        #endregion

        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        public Version(string versionStr)
        {
            this.Prase(versionStr);
        }

        public Version()
        {
            this.mIsValid = true;
        }

        #endregion

        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        /// <param name="versionStr"></param>
        private void Prase(string versionStr)
        {
            if (string.IsNullOrEmpty(versionStr))
            {
                throw new InvalidVersionStringException();
            }
            var regex = new Regex(PATTERN);
            var m = regex.Match(versionStr);

            if (!m.Success)
            {
                throw new InvalidVersionStringException(versionStr);
            }

            string[] splits = versionStr.Split('.');
            this.Major = splits[0];
            this.Minor = splits[1];
            this.Patch = splits[2];

            this.mIsValid = true;
        }

        public VersionCompareResult CompareTo(Version other)
        {
            if (this.MajorNum > other.MajorNum)
            {
                return VersionCompareResult.HigherForMajor;
            }
            
            if (this.MajorNum < other.MajorNum)
            {
                return VersionCompareResult.LowerForMajor;
            }

            if (this.MinorNum > other.MinorNum)
            {
                return VersionCompareResult.HigherForMinor;
            }

            if (this.MinorNum < other.MinorNum)
            {
                return VersionCompareResult.LowerForMinor;
            }

            if (this.PatchNum > other.PatchNum)
            {
                return VersionCompareResult.HigherForPatch;
            }

            if (this.PatchNum < other.PatchNum)
            {
                return VersionCompareResult.LowerForPatch;
            }

            return VersionCompareResult.Equal;
        }

        public string GetVersionString()
        {
            if (!this.mIsValid)
                throw new InvalidOperationForVersionException("GetVersionString");
            return $"{this.Major}.{this.Minor}.{this.Patch}";
        }

        public void IncrementOneForPatch()
        {
            if (!this.mIsValid)
                throw new InvalidOperationForVersionException("IncrementOneForPatch");
            var patchNum = this.PatchNum + 1;
            this.Patch = patchNum.ToString();
        }


        public Version Clone()
        {
            if (!this.mIsValid)
                throw new InvalidOperationForVersionException("Clone");
            Version result = new Version();
            result.Major = this.Major;
            result.Minor = this.Minor;
            result.Patch = this.Patch;
            return result;
        }

        public override string ToString()
        {
            if (!this.mIsValid)
            {
                return "Invalid Version!";
            }
            return $"Version : {this.Major}.{this.Minor}.{this.Patch}";
        }

        #endregion
    }
}
