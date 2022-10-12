using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTool.Editor.UploadUtilitis
{
    public struct UploadTask
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        public string Source, Target;
        //public bool IsFolder;

        #endregion

        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        public UploadTask(string source, string target)
        {
            this.Source = source;
            this.Target = target;
        }

        #endregion

        //--------------------------------------------------------------

        #region Methods

        //--------------------------------------------------------------

        #endregion

    }
}
