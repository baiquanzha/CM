//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using MTool.AppBuilder.Editor.Builds.BuildInfos;
//using MTool.AppBuilder.Editor.Builds.Contexts;
//using UnityEngine;
//using MTool.Core.Pipeline;
//#if DEBUG_FILE_CRYPTIONSaveBaseVersionBuildInfoActionSaveBaseVersionBuildInfoAction
//using File = MTool.Core.IO.File;
//#else
//using File = System.IO.File;
//#endif

//namespace MTool.AppBuilder.Editor.Builds.Actions.ResPack
//{
//    public class SaveBaseVersionBuildInfoAction : BaseBuildFilterAction
//    {
//        //--------------------------------------------------------------
//        #region Fields
//        //--------------------------------------------------------------

//        #endregion

//        //--------------------------------------------------------------
//        #region Properties & Events
//        //--------------------------------------------------------------

//        #endregion

//        //--------------------------------------------------------------
//        #region Creation & Cleanup
//        //--------------------------------------------------------------

//        #endregion

//        //--------------------------------------------------------------
//        #region Methods
//        //--------------------------------------------------------------

       

//        public override bool Test(IFilter filter, IPipelineInput input)
//        {
//            return true;
//        }

//        public override void Execute(IFilter filter, IPipelineInput input)
//        {
//            this.Save(filter,input);
//            this.State = ActionState.Completed;
//        }


//        private void Save(IFilter filter, IPipelineInput input)
//        {
//            var appBuildContext = AppBuildContext;

//            var lastBuildInfo = appBuildContext.GetLastBuildInfo();
//            if (lastBuildInfo == null) // 
//            {
//                if(appBuildContext.makeVersionMode != Contexts.AppBuildContext.MakeVersionMode.MakeBaseVersion)
//                    throw new InvalidOperationException($"The last build info is not exist ! Make version error , mode ：{appBuildContext.makeVersionMode} .");
//                lastBuildInfo = new LastBuildInfo();
//                lastBuildInfo.baseVersionInfo = appBuildContext.AppInfoManifest;
//                lastBuildInfo.versionInfo= appBuildContext.AppInfoManifest;
//            }
//            else
//            {
//                lastBuildInfo.versionInfo = appBuildContext.AppInfoManifest;
//            }

//            appBuildContext.SaveLastBuildInfo(lastBuildInfo);
//        }

//        #endregion


//    }
//}
