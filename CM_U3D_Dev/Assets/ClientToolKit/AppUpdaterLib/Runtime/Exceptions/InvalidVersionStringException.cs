using System;

namespace MTool.AppUpdaterLib.Runtime.Exceptions
{
    public class InvalidVersionStringException : Exception
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        public InvalidVersionStringException() : base($"Invalid version null number !")
        {

        }

        public InvalidVersionStringException(string versionStsr) : base($"Invalid version number : {versionStsr} !")
        {
        }


        #endregion

        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        #endregion

    }
}
