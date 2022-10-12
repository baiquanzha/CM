namespace MTool.Core.Pipeline
{
    public interface IPipelineInput
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        string Desc { get; }

        #endregion

        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        void SetData(string key, System.Object arg);

        T GetData<T>(string key,T defaultVal);

        #endregion

        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        #endregion
    }
}
