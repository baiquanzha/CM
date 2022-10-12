namespace MTool.AppUpdaterLib.Runtime.ResManifestParser
{
    public abstract class BaseResManifestParser
    {
        public abstract VersionManifest Parse(string manifestContent);

        public abstract string Serialize(VersionManifest manifest);

        public abstract void WriteToAppInfo(string resVersion , string resVersionNum = null);

        public abstract UpdateResourceType GetUpdateResourceType();

    }
}
