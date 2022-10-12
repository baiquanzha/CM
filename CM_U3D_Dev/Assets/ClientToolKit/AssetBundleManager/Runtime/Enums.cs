namespace MTool.AssetBundleManager.Runtime
{
    public enum ResourceLoadInitError
    {
        LoadResManifestFailure,     //加载built in res_android.json失败

        LoadDataManifestFailure,    //加资built in res_data.json失败

        LoadFileListFailure,    //加资file_list.x失败

        UnKnow
    }
}
