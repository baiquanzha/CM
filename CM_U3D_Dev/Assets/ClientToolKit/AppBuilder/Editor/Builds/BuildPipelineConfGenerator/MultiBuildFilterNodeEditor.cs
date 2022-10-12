using System;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

[NodeEditor.CustomNodeEditorAttribute(typeof(MultiBuildFilterNode))]
class MultiBuildFilterNodeEditor : NodeEditor
{

    public override void OnHeaderGUI()
    {
        var node = target as MultiBuildFilterNode;
        var types = node.GetValue();
        string headName;
        if (types == null)
        {
            headName = "Unknow Filter";
        }
        else
        {
            if (node.filterIdx >= types.typeNames.Length)
            {
                node.filterIdx = 0;
                node.filterName = "Unknow Filter";
            }
            headName = $"{node.filterName}  [FILTER]";
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

        var node = target as MultiBuildFilterNode;
        var types = node.GetValue();
        if (types != null && types.typeNames != null && types.typeNames.Length > 0)
        {
            if (node.filterIdx >= types.typeNames.Length)
            {
                node.filterIdx = 0;
                node.filterName = "Unknow Filter";
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

            node.filterIdx = EditorGUILayout.Popup(node.filterIdx, typeNames);
            node.filterName = typeNames[node.filterIdx];
        }
        else
        {
            node.filterIdx = 0;
            node.filterName = "Unknow Filter";
        }

    }
}