using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace MTool.Editor.UploadUtilitis.WinSCP
{
    public sealed partial class WinSCPConfig
    {

        public static WinSCPConfig GetInstance()
        {
            WinSCPConfig cfg = AssetDatabase.LoadAssetAtPath<WinSCPConfig>("Assets/Standard Assets/AppBuilder/Editor/UploadUtilitis/WinSCP/WinSCPConfig.asset");
            return cfg;
        }
    }
}
