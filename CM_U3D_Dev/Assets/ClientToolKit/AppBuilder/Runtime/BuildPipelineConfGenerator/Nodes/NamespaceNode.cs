using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using XNode;

[Serializable]
public class NamespaceNode : Node
{

    [Input] public string nameSpaceInput;

    [Output] public string nameSpace;

	// Use this for initialization
	protected override void Init() {
		base.Init();
		
	}

	// Return the correct value of an output port when requested
	public override object GetValue(NodePort port) 
    {
		
		return nameSpaceInput;
	}
}