using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MTool.AssetBundleManager.Runtime;

#if UNITY_EDITOR
public class MenuCommands
{
    const string kSimulationMode = "MTool/AssetBundleManager/Simulation Mode";

    [MenuItem(kSimulationMode)]
    public static void ToggleSimulationMode()
    {
        AssetBundleManager.SimulateAssetBundleInEditor = !AssetBundleManager.SimulateAssetBundleInEditor;
    }

    [MenuItem(kSimulationMode, true)]
    public static bool ToggleSimulationModeValidate()
    {
        Menu.SetChecked(kSimulationMode, AssetBundleManager.SimulateAssetBundleInEditor);
        return true;
    }
}
#endif

