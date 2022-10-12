namespace MTool.AppUpdaterLib.Runtime.Helps
{
    internal class ErrorTypeHelper
    {
        public static string GetErrorString(AppUpdaterErrorType type)
        {
            switch (type)
            {
                case AppUpdaterErrorType.LoadBuiltinAppInfoFailure:
                    return "Load builtin appinfo.x failure!";
                case AppUpdaterErrorType.ParseBuiltinAppInfoFailure:
                    return "Parse builtin appinfo.x failure !";
                case AppUpdaterErrorType.ParseLocalAppInfoFailure:
                    return "Parse local appinfo.x failure !";
                case AppUpdaterErrorType.DownloadLighthouseFailure:
                    return "Download lighthouse config failure!";
                case AppUpdaterErrorType.DownloadLighthouseConfigInvalid:
                    return "Download lighthouse config is invalid!";
                case AppUpdaterErrorType.ParseLighthouseConfigError:
                    return "Parser lighthouse config error!";
                case AppUpdaterErrorType.RequestGetVersionFailure:
                    return "Request verison check data from remote http serve failure!";
                case AppUpdaterErrorType.RequestResManifestFailure:
                    return "Request resource manifest that from remote file server failure!";
                case AppUpdaterErrorType.DownloadFileFailure:
                    return "Download file that from remote file server failure!";
                case AppUpdaterErrorType.ParseLocalResManifestFailure:
                    return "Parse local resource manifest failure!";
                case AppUpdaterErrorType.ParseRemoteResManifestFailure:
                    return "Parse remote resource manifest failure!";
                case AppUpdaterErrorType.RequestDataResVersionFailure:
                    return "Request table data resource version failure!";
                case AppUpdaterErrorType.RequestUnityResVersionFailure:
                    return "Request unity resource data version failure!";
                case AppUpdaterErrorType.RequestAppRevisionNumIsSmallToLocal:
                    return "The revision that receive is small to local revision .";
                case AppUpdaterErrorType.RequestAppRevisionNumFailure:
                    return "Request app revision number failure!";
                case AppUpdaterErrorType.DiskIsNotEnoughToDownPatchFiles:
                    return "The disk available space is not enough to download path files !";
                case AppUpdaterErrorType.LighthouseConfigServersIsUnReachable:
                    return "The servers data from lighthouse config is not reachable!";
                case AppUpdaterErrorType.AppBuiltInVersionNumNotCompatibleToExternal:
                    return "The builtIn version is not compatible to external!";
                case AppUpdaterErrorType.DeleteExternalStorageFilesFailure:
                    return "Delete the files that form external storage failure!";
                case AppUpdaterErrorType.UserGiveUpDownload:
                    return "User give up download";
                case AppUpdaterErrorType.LoadBuiltinResManifestFailure:
                    return "Load builtin res_{platform}.json failure!";
                case AppUpdaterErrorType.LoadBuiltinDataManifestFailure:
                    return "Load builtin res_data.json failure";
                default:
                    return "Error ! Unknow appupdater error! ";
            } 
        }
    }
}
