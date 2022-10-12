using System;
using MTool.AppBuilder.Runtime.BuildPipelineConfGenerator;
using MTool.AppBuilder.Runtime.Configuration;
using UnityEngine;
using XNode;

[Serializable]
public class MultiBuildFilterNode : Node
{

    [Input]
    public NamespaceTypesNode.NamespaceTypesOutput filterTypes;

    [HideInInspector]
    public int filterIdx;

    [HideInInspector]
    public string filterName;

    [Input] public string[] actions;

    [Output] public FilterOutput json;

	// Use this for initialization
	protected override void Init() {
		base.Init();
	}

    public NamespaceTypesNode.NamespaceTypesOutput GetValue()
    {
        return this.GetInputValue<NamespaceTypesNode.NamespaceTypesOutput>("filterTypes");
    }

    // Return the correct value of an output port when requested
    public override object GetValue(NodePort port)
    {
        var val = GetValue();

        if (val == null)
        {
            return null;
        }

        var fullName = val.typeNames[filterIdx];
        AppBuildFilterInfo info = new AppBuildFilterInfo();
        info.TypeFullName = fullName;

        info.Action.IsActionQueue = true;

        var actions = GetInputValues<string>("actions");

        if (actions != null)
        {
            for (int i = 0; i < actions.Length; i++)
            {
                var actionInfo = new AppBuildActionInfo();
                actionInfo.TypeFullName = actions[i];
                info.Action.Childs.Add(actionInfo);
            }
        }
      

        json = new FilterOutput();
        json.info = info;

        return json; // Replace this
	}
}