using System;

namespace MTool.AppBuilder.Runtime.Exceptions
{
    public class InvalidOperationForVersionException : Exception
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
        public InvalidOperationForVersionException(string opMethod) : base($"Invalid method call ! method name : {opMethod}!")
        {
        }

        #endregion

        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------




        #endregion

    }
}
