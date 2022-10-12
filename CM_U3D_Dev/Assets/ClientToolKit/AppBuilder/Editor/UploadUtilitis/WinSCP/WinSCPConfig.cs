using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace MTool.Editor.UploadUtilitis.WinSCP
{

    [CreateAssetMenu(fileName = "WinSCPConfig", menuName = "Test/CreateWinSCPConfig", order = 1 )]
    public sealed partial class WinSCPConfig : ScriptableObject
    {
        public string serverInfoName = "wyb314.txt";

        public string serverDesDir = "ftp/GameRes/Files/ABs";

        public string targetUploadEngineRelativePath = "/../Tools/SFTPUploadEngine/SFTPUploadEngine.exe";

        public string targetUploadEngineWorkDirectoryRelativePath = "/../Tools/SFTPUploadEngine/";


        public string targetAmazonS3UploadEngineRelativePath = "/../Tools/PythonUpload/AmazonS3/UploaderForUnityEditor.bat";

        public string targetAmazonS3UploadEngineWorkDirectoryRelativePath = "/../Tools/PythonUpload/AmazonS3/";
    }
}
