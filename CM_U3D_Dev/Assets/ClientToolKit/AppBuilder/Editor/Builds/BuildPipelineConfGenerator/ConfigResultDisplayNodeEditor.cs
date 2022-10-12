using System.Text;
using MTool.AppBuilder.Editor.Builds;
using UnityEngine;
using UnityEditor;
using XNodeEditor;
using System.IO;
using MTool.AppBuilder.Runtime.BuildPipelineConfGenerator;
using MTool.AppBuilder.Runtime.Configuration;
using YamlDotNet;

[CustomNodeEditor(typeof(ConfigResultDisplayNode))]
public class ConfigResultDisplayNodeEditor : NodeEditor
{

    public override void OnBodyGUI()
    {
        base.OnBodyGUI();

        var node = target as ConfigResultDisplayNode;

        if (node.GetValue() is FilterOutput[] val && val != null)
        {
            EditorGUILayout.LabelField($"val length : {val.Length} .");

            if (GUILayout.Button("Export config"))
            {
                if (string.IsNullOrEmpty(node.configName.Trim()))
                {
                    this.window.ShowNotification(new GUIContent ("请输入要导出的配置名！"));
                    return;
                }
                this.ExportConfig(val,node.configName);
            }
        }
        else
        {
            EditorGUILayout.LabelField($"filter output is null .");
        }
    }

    public override int GetWidth()
    {
        return 300;
    }

    private void ExportConfig(FilterOutput[] outputs, string configName)
    {


        AppBuildProcessConfig config = new AppBuildProcessConfig();

        for (int i = 0; i < outputs.Length ; i++)
        {
            var filterOutput = outputs[i];
            AppBuildFilterInfo info = new AppBuildFilterInfo();
            info.TypeFullName = filterOutput.info.TypeFullName;
            info.Action = filterOutput.info.Action;
            config.Filters.Add(info);
        }


        string yaml = YAMLSerializationHelper.Serialize(config);
        var appBuildConfig = AppBuildConfig.GetAppBuildConfigInst();
        string appBuildCacheFolder = appBuildConfig.AppBuildConfigFolder;
        string pathFolder = appBuildCacheFolder;

        pathFolder = EditorUtils.OptimazePath(pathFolder);
        if (!Directory.Exists(pathFolder))
            Directory.CreateDirectory(pathFolder);

        string path = $"{pathFolder}/{configName}.yaml";

        if (File.Exists(path))
        {
            bool result = EditorUtility.DisplayDialog("保存编译配置","当前已经存在此配置文件，是否覆盖？","OK", "No");
            if (!result)
            {
                return;
            }
        }

        File.WriteAllText(path, yaml, new UTF8Encoding(false, true));
        Debug.Log($"Write \"{path}\" success ! Info : \r\n" + yaml);
        
        AssetDatabase.Refresh();
    }

}
