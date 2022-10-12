using UnityEngine;
using UnityEditor;

namespace MTool.AppBuilder.Editor.Core
{
	public abstract class MyBaseAssitWindow
	{
		private bool mDrawSceneView = false;

		public bool drawSceneView
		{
			set { this.mDrawSceneView = value; }
			get { return this.mDrawSceneView; }
		}

		public string windowName = "UNKNOW";

		/// <summary>
		/// 宿主的unity的EditorWindow子类的实例
		/// </summary>
		protected MyBaseEditorWindow mParasitisUnityWin;


		public MyBaseAssitWindow(MyBaseEditorWindow mMotorWin)
		{
			this.mParasitisUnityWin = mMotorWin;
		}


		public virtual void OnGUI() { }

		public virtual void OnSceneGUI(SceneView sceneView) { }

		public virtual void Reset() { }

		public virtual void OnWinOpen(params System.Object[] args) { }


		public virtual void OnWinClose() { }


		public virtual void OnDestroy() { }


		public virtual void Update()
		{

		}

		public virtual void ShowNotification(string content)
		{
			this.mParasitisUnityWin.ShowNotification(new GUIContent(content));
		}

		public virtual void ShowNotification(GUIContent content)
		{
			this.mParasitisUnityWin.ShowNotification(new GUIContent(content));
		}


		public virtual void RemoveNotification()
		{
			this.mParasitisUnityWin.RemoveNotification();
		}

	}

}
