using System.Collections.Generic;
using MTool.Core.Pipeline;

namespace MTool.Core.Pipeline
{
    public abstract class BasePipelineInput : IPipelineInput
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly Dictionary<string, System.Object> _dataDic = new Dictionary<string, object>();

        #endregion

        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        public abstract string Desc { set; get; }

        #endregion

        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        public virtual T GetData<T>(string key, T defaultVal)
        {
            System.Object obj;

            if (this._dataDic.TryGetValue(key, out obj))
            {
                return (T)obj;
            }

            return defaultVal;
        }

        public virtual void SetData(string key, object arg)
        {
            if (!this._dataDic.ContainsKey(key))
            {
                this._dataDic.Add(key, arg);
            }
            else
            {
                this._dataDic[key] = arg;
            }
        }

        #endregion

    }
}
