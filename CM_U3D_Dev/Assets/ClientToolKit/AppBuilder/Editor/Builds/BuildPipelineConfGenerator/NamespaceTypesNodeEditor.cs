using System;
using System.Collections.Generic;
using MTool.AppBuilder.Editor.Builds.BuildPipelineConfGenerator;
using UnityEngine;
using XNodeEditor;

[CustomNodeEditor(typeof(NamespaceTypesNode))]
class NamespaceTypesNodeEditor : NodeEditor
{
    public override void OnBodyGUI()
    {
        base.OnBodyGUI();

        var node = target as NamespaceTypesNode;

        if (GUILayout.Button("Refresh"))
        {
            var assembly = this.GetType().Assembly;
            List<string> fullNames = new List<string>();

            var baseType = BuildNodeBaseTypeDic.Data[node.nodeType];
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(baseType))
                {
                    fullNames.Add(type.FullName);
                }
            }
            node.outPut.typeNames = fullNames.ToArray();
        }
    }

    public override int GetWidth()
    {
        
        return base.GetWidth();
    }

    
}
