namespace MTool.Core.Pipeline
{

    /// <summary>
    /// The processing state
    /// </summary>
    public enum ProcessState : byte
    {
        /// <summary>
        /// The being processed data was processed completely
        /// </summary>
        Completed,
        /// <summary>
        /// The processor is cancled
        /// </summary>
        Cancled,
        /// <summary>
        /// The processor is in error state
        /// </summary>
        Error
    }

    public struct ProcessResult
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        public ProcessState State { private set; get; }


        public string Message { private set; get; }

        #endregion

        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        public static ProcessResult Create(ProcessState state)
        {
            ProcessResult result = new ProcessResult();
            result.State = state;

            return result;
        }

        public static ProcessResult Create(ProcessState state, string message)
        {
            ProcessResult result = new ProcessResult();
            result.State = state;
            result.Message = message;

            return result;
        }

        #endregion

        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        #endregion

    }
}
