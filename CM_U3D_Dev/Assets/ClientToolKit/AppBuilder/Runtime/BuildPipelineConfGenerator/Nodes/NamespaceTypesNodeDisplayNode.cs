using XNode;

public class NamespaceTypesNodeDisplayNode : Node {


	//[Input] public AppBuilderTypes typeFullNames;


	// Use this for initialization
	protected override void Init() {
		base.Init();
	}

	// Return the correct value of an output port when requested
	public string[] GetValue()
    {
		//var  types = this.GetInputValue<AppBuilderTypes>("typeFullNames"); // Replace this 

		//return types?.fullNames;

		return null;
    }
}