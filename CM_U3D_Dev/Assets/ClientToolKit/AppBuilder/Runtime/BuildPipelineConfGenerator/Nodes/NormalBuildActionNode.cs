using System;
using System.Security.Cryptography;
using UnityEngine;
using XNode;

[Serializable]
public class NormalBuildActionNode : Node
{
	[Input]
	public NamespaceTypesNode.NamespaceTypesOutput actionTypes;

	//[HideInInspector]
    public int actionIdx;

    [HideInInspector]
    public string actionName;


	[Output] public string actionFullName;

	// Use this for initialization
	protected override void Init() {
		base.Init();
	}

	// Return the correct value of an output port when requested
	public NamespaceTypesNode.NamespaceTypesOutput GetValue()
    {
		return this.GetInputValue<NamespaceTypesNode.NamespaceTypesOutput>("actionTypes");
    }

    public override object GetValue(NodePort port)
    {
        var types = this.GetInputValue<NamespaceTypesNode.NamespaceTypesOutput>("actionTypes");

        if (types == null)
        {
			return null;
        }

		actionFullName = types.typeNames[actionIdx];

		return actionFullName;
    }
}