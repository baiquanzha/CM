#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace MTool.AppUpdaterLib.Runtime
{
    public sealed class Utility
    {
		//--------------------------------------------------------------
		#region Fields
		//--------------------------------------------------------------
        
        public const string AssetBundlesOutputPath = "AssetBundles";

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

		public static string GetPlatformName()
		{
#if UNITY_EDITOR
			return GetPlatformForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
#else
			return GetPlatformForAssetBundles(Application.platform);
#endif
		}

#if UNITY_EDITOR
		private static string GetPlatformForAssetBundles(BuildTarget target)
		{
			switch (target)
			{
				case BuildTarget.Android:
					return "Android";
				case BuildTarget.iOS:
					return "iOS";
				case BuildTarget.WebGL:
					return "WebGL";
				case BuildTarget.StandaloneWindows:
				case BuildTarget.StandaloneWindows64:
					return "Windows";
#if !UNITY_2019_1_OR_NEWER
				case BuildTarget.StandaloneLinux:
#endif
				case BuildTarget.StandaloneLinux64:
#if !UNITY_2019_1_OR_NEWER
				case BuildTarget.StandaloneLinuxUniversal:
#endif
					return "Linux";
				default:
					return null;
			}
		}
#endif

		private static string GetPlatformForAssetBundles(RuntimePlatform platform)
		{
			switch (platform)
			{
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
				case RuntimePlatform.LinuxPlayer:
					return "Linux";
				default:
					return null;
			}
		}

		/// <summary>
		/// 获取设备唯一ID
		/// </summary>
		/// <returns></returns>
        public static string GetDeviceUniqueID()
        {
            return SystemInfo.deviceUniqueIdentifier;
        }

		#endregion

	}
}
