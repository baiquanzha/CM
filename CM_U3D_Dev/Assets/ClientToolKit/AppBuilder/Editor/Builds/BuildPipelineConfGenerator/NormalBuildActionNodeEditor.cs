using System;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

[CustomNodeEditor(typeof(NormalBuildActionNode))]
class NormalBuildActionNodeEditor : NodeEditor
{

    public override void OnHeaderGUI()
    {
        var node = target as NormalBuildActionNode;
        var types = node.GetValue();

        string headName;
        if (types == null)
        {
            node.actionIdx = 0;
            headName = "Unknow Action";
        }
        else
        {
            if (node.actionIdx >= types.typeNames.Length)
            {
                node.actionIdx = 0;
                node.actionName = "Unknow action";
            }
            headName = $"{node.actionName}  [Action]";
        }
        GUILayout.Label(headName, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
    }

    public override int GetWidth()
    {
        return 300;
    }

    public override void OnBodyGUI()
    {
        base.OnBodyGUI();

        var node = target as NormalBuildActionNode;
        var types = node.GetValue();
        if (types != null && types.typeNames != null && types.typeNames.Length > 0)
        {
            if (node.actionIdx >= types.typeNames.Length)
            {
                node.actionIdx = 0;
                node.actionName = "Unknow action";
            }

            string[] typeNames = new string[types.typeNames.Length];
            for (int i = 0; i < typeNames.Length; i++)
            {
                var fullName = types.typeNames[i];
                int idx = fullName.LastIndexOf(".", StringComparison.Ordinal);
                if (idx != -1)
                {
                    typeNames[i] = fullName.Substring(idx + 1);
                }
                else
                {
                    typeNames[i] = fullName;
                }
            }

            node.actionIdx = EditorGUILayout.Popup(node.actionIdx, typeNames);
            node.actionName = typeNames[node.actionIdx];
        }
        else
        {
            node.actionIdx = 0;
            node.actionName = "Unknow action";
        }
    }

}
