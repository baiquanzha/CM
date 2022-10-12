using System;
using MTool.AppBuilder.Runtime.BuildPipelineConfGenerator;
using XNode;

[Serializable]
public class ConfigResultDisplayNode : Node {

	[Input]public FilterOutput[] filterOutputs;

    [Input] public string configName;

	// Use this for initialization
	protected override void Init() {
		base.Init();
	}


    public object GetValue()
    {
        return GetInputValues<FilterOutput>("filterOutputs");
    }

}