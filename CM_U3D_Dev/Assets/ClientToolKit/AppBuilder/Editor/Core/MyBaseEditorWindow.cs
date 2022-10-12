using System;
using System.Collections.Generic;
using UnityEditor;

namespace MTool.AppBuilder.Editor.Core
{
	public abstract class MyBaseEditorWindow : EditorWindow
	{

		private int mEditorHash;

		public int editorHash
		{
			get { return this.mEditorHash; }
		}



		protected MyBaseAssitWindow mCurrentWin;

		public MyBaseAssitWindow currentWin
		{
			get { return mCurrentWin; }
		}



		protected Dictionary<Type, MyBaseAssitWindow> mWinDic = new Dictionary<Type, MyBaseAssitWindow>();

		public Dictionary<Type, MyBaseAssitWindow> winDic
		{
			get
			{
				return this.mWinDic;
			}
		}


		#region Messages

		#region ScriptableObject 的 Messages(EditorWindow inherit by ScriptableObject)

		public virtual void OnEnable() { }


		public virtual void OnDisable() { }

		/// <summary>
		/// Called when the window is closed
		/// </summary>
		public virtual void OnDestroy()
		{

#if UNITY_2019
            SceneView.duringSceneGui -= this.OnSceneGUI;
#else
			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
#endif



			if (this.mWinDic != null && this.mWinDic.Count > 0)
			{
				foreach (var kvp in this.mWinDic)
				{
					kvp.Value.OnDestroy();
				}
				this.mWinDic.Clear();
			}
		}

#endregion


		public virtual void OnGUI()
		{
			if (this.mCurrentWin != null)
			{
				this.mCurrentWin.OnGUI();
			}
		}

		public virtual void OnSceneGUI(SceneView sceneView)
		{
			if (this.mWinDic != null && this.mWinDic.Count > 0)
			{
				foreach (var kvp in this.mWinDic)
				{
					if (kvp.Value == this.mCurrentWin)
					{
						kvp.Value.OnSceneGUI(sceneView);

						continue;
					}
					if (kvp.Value.drawSceneView)
					{
						kvp.Value.OnSceneGUI(sceneView);
					}
				}
			}
		}



		/// <summary>
		/// Called when the window gets keyboard focus
		/// </summary>
		public virtual void OnFocus()
		{
			this.mEditorHash = GetHashCode();

#if UNITY_2019
            SceneView.duringSceneGui -= this.OnSceneGUI;
			SceneView.duringSceneGui += this.OnSceneGUI;
#else
			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
#endif
		}


		/// <summary>
		/// Called when the window loses keyboard focus
		/// </summary>
		public virtual void OnLostFocus()
		{

		}

		/// <summary>
		/// Called 100 times per second on all visible windows
		/// </summary>
		public virtual void Update()
		{
			if (this.mWinDic != null && this.mWinDic.Count > 0)
			{

				if (this.mCurrentWin != null)
				{
					this.mCurrentWin.Update();
				}
			}
		}

		/// <summary>
		/// OnInspectorUpdate is called at 10 frames per second to give the inspector a chance to update
		/// </summary>
		public virtual void OnInspectorUpdate()
		{
			this.Repaint();
		}



#endregion



		public void OpenWindow<T>(params System.Object[] args) where T : MyBaseAssitWindow
		{

			T win = GetAssitlWindow<T>();

			if (this.mCurrentWin != win)
			{
				if (this.mCurrentWin != null)
				{
					this.mCurrentWin.OnWinClose();
				}

				this.mCurrentWin = win;

				win.OnWinOpen(args);
			}


		}


		public void OpenWindow(Type type, System.Object arg = null)
		{
			MyBaseAssitWindow win = GetAssitlWindow(type);

			if (this.mCurrentWin != win)
			{
				this.mCurrentWin.OnWinClose();

				this.mCurrentWin = win;

				win.OnWinOpen(arg);
			}

		}


		public T GetAssitlWindow<T>() where T : MyBaseAssitWindow
		{
			T win = null;
			Type keyType = typeof(T);
			win = GetAssitlWindow(keyType) as T;

			return win;
		}

		public MyBaseAssitWindow GetAssitlWindow(Type winType)
		{
			MyBaseAssitWindow win = null;

			if (this.mWinDic.ContainsKey(winType))
			{
				win = this.mWinDic[winType];
			}

			return win;
		}


		public bool AddWindow(MyBaseAssitWindow win)
		{
			if (win == null)
			{

				return false;
			}
			Type type = win.GetType();

			this.winDic.Add(type, win);

			return true;
		}



		public void RemoveWindow<T>() where T : MyBaseAssitWindow
		{
			Type winType = typeof(T);

			if (this.mWinDic.ContainsKey(winType))
			{
				MyBaseAssitWindow win = this.mWinDic[winType];

				win.OnDestroy();

				this.mWinDic.Remove(winType);

				if (this.mCurrentWin == win) { this.mCurrentWin = null; }

			}
			else
			{
				UnityEngine.Debug.LogError("The window areaType that name is : " + winType.Name + " is not exist in the winDir! ------> MyBaseEditorWindow");
			}

		}


		public void RemoveWindow(Type winType)
		{

			if (this.mWinDic.ContainsKey(winType))
			{

				MyBaseAssitWindow win = this.mWinDic[winType];

				win.OnDestroy();

				this.mWinDic.Remove(winType);

				if (this.mCurrentWin == win) { this.mCurrentWin = null; }
			}
			else
			{
				UnityEngine.Debug.LogError("The window areaType that name is : " + winType.Name + " is not exist in the winDir! ------> MyBaseEditorWindow");
			}
		}
	}

}
