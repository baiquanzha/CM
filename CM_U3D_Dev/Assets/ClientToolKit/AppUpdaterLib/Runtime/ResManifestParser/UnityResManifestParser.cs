using MTool.AppUpdaterLib.Runtime.Managers;
using UnityEngine;

namespace MTool.AppUpdaterLib.Runtime.ResManifestParser
{
    internal class UnityResManifestParser : BaseResManifestParser
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

        public override VersionManifest Parse(string manifestContent)
        {
            var manifest = VersionManifestParser.Parse(manifestContent);

            return manifest;
        }

        public override string Serialize(VersionManifest manifest)
        {
            var content = VersionManifestParser.Serialize(manifest);

            return content;
        }

        public override void WriteToAppInfo(string resVersion, string resVersionNum = null)
        {
            AppVersionManager.AppInfo.unityDataResVersion = resVersion;

            if (!string.IsNullOrEmpty(resVersionNum))
            {
                Version version = new Version(AppVersionManager.AppInfo.version);
                version.Patch = resVersionNum;
                AppVersionManager.AppInfo.version = version.GetVersionString();
            }
            AppVersionManager.SaveCurrentAppInfo();
        }

        public override UpdateResourceType GetUpdateResourceType()
        {
            return UpdateResourceType.NormalResource;
        }

        #endregion


    }
}
