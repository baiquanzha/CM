using System;

namespace MTool.AppBuilder.Editor.Builds.Exceptions
{
    public class MakeAppPatchException : Exception 
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

        public MakeAppPatchException(string baseVersion, string curVersion)
            : base($"Make app patch error ! Your want to make a patch that verison is {curVersion} , but the " +
                   $"base verison is {baseVersion} !")
        {

        }

        #endregion

        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        #endregion

    }
}
