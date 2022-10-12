using System;

namespace MTool.AppUpdaterLib.Runtime.Exceptions
{
    public class InvalidVersionNumGetOperationException : Exception
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

        #endregion

        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        public InvalidVersionNumGetOperationException(string propertyName) : base($"Get versoin property failure , property name : {propertyName}!")
        {

        }

        #endregion

    }
}
