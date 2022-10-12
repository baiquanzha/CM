using UnityEngine;
//using XLua;

//[LuaCallCSharp]
public class SingletonObject<T> : MonoBehaviour where T : MonoBehaviour
{
	protected static T instance;
	
	public static T Get(){
		return instance;
	}

	public static void ImmediateDestroy() {
		instance = null;
	}
	
	protected virtual void Awake() {
		instance = this as T;
	}
	
	protected virtual void OnDestroy() {
		instance = null;
	}
}