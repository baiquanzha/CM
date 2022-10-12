using System;

namespace MTool.AppBuilder.Runtime.Exceptions
{
    public class BuildAppVersionException : Exception
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

        public BuildAppVersionException(string targetVersion , string lastVersion) 
            : base($"Target version : {targetVersion}  lastVersion : {lastVersion} , pleause to config your target" +
                   $" verison , it is need to big or equal your last build version!")
        {

        }


        #endregion

        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        #endregion

    }
}
