using System;
using MTool.AppBuilder.Runtime.BuildPipelineConfGenerator;
using XNode;

public class NamespaceTypesNode : Node
{

    public BuildNodeType nodeType = BuildNodeType.Action;

	[Output] public NamespaceTypesOutput outPut;

	protected override void Init() {
		base.Init();
	}

	public override object GetValue(NodePort port)
    {
		return outPut;
	}

	[Serializable]
    public class NamespaceTypesOutput
    {
        public string[] typeNames;
	}


}