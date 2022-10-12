using System.Text;
using UnityEngine;

/// <summary>
/// 平台相关的应该全部扔到这里
/// </summary>
public class PlatformUtil {
    private static StringBuilder mTempSb = new StringBuilder();

    public static bool IsEditor() {
#if UNITY_EDITOR
        return true;
#else
		return false;
#endif
    }

    public static string GetPlatformName() {
		return GetPlatformForAssetBundles(Application.platform);
    }

    static string GetPlatformForAssetBundles(RuntimePlatform platform) {
        switch (platform) {
            case RuntimePlatform.Android:
                return "Android";
            case RuntimePlatform.IPhonePlayer:
                return "iOS";
            case RuntimePlatform.WebGLPlayer:
                return "WebGL";
            case RuntimePlatform.WindowsPlayer:
                return "Windows";
            case RuntimePlatform.OSXPlayer:
                return "OSX";
            // Add more build targets for your own.
            // If you add more targets, don't forget to add the same platforms to GetPlatformForAssetBundles(RuntimePlatform) function.
            default:
                return null;
        }
    }

    public static string GetStreamingAssetsPath(string path) {
        mTempSb.Clear();

#if UNITY_ANDROID && !UNITY_EDITOR
            mTempSb.Append(Application.streamingAssetsPath);
            mTempSb.Append("/");
#elif UNITY_IPHONE && !UNITY_EDITOR
            mTempSb.Append("file://");
            mTempSb.Append(Application.streamingAssetsPath);
            mTempSb.Append("/");
#elif UNITY_EDITOR
        mTempSb.Append("file://");
        mTempSb.Append(Application.streamingAssetsPath);
        mTempSb.Append("/");
#endif

#if APPEND_PLATFORM_NAME
            mTempSb.Append(Utility.GetPlatformName());
            mTempSb.Append("/");
#endif

        mTempSb.Append(path);
        return mTempSb.ToString();
    }
}