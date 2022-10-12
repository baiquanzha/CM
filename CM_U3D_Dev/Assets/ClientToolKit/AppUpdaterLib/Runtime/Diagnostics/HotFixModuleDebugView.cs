//using UnityEngine;

//namespace MTool.AppUpdaterLib.Runtime.Diagnostics
//{
//    public sealed class HotFixModuleDebugView
//    {

//        private AppUpdaterFsmOwner _mAppUpdaterFsmOwner;
//        //public HotFixModuleDebugView(AppUpdaterFsmOwner appUpdaterFsmOwner)
//        //{
//        //    this._mAppUpdaterFsmOwner = appUpdaterFsmOwner;

//        //    this.InitializeComponent();
//        //}


//        private void InitializeComponent()
//        {
//            float width = Screen.width * .8f;
//            float height = Screen.height * .8f;
//            this.mWindPos = new Rect((Screen.width - width) / 2, (Screen.height - height) / 2, width, height);
//        }

//        public bool Show { set; get; }
//        private Vector2 mScrollPos0 = Vector2.zero;
//        private Vector2 mScrollPos1 = Vector2.zero;

//        private Rect mWindPos;
//        public void DrawGUI()
//        {
//            if (!Show)
//            {
//                return;
//            }
            
//            this.mWindPos = GUILayout.Window(1, this.mWindPos, DrawMainWin, "Hot Fix Debug Window");

//            this.DrawOtherWindow();
//        }

//        private void DrawMainWin(int id)
//        {
//            GUILayout.BeginVertical();

//            GUILayout.BeginVertical("Box");
//            GUILayout.Label("State Info : ");

//            GUILayout.Label(this._mAppUpdaterFsmOwner.FSM.CurrentState.ToString());

//            GUILayout.EndVertical();


//            this.mScrollPos0 = GUILayout.BeginScrollView(mScrollPos0);
//            GUILayout.BeginVertical("Box");

//            GUILayout.Label("HotFix Context : ");

//            GUILayout.Label(this._mAppUpdaterFsmOwner.Context.ToString());
//            GUILayout.EndVertical();

//            GUILayout.EndScrollView();

//            GUILayout.BeginVertical("Box");
//            GUILayout.Label("Cache Info : ");
//            this.mScrollPos1 = GUILayout.BeginScrollView(this.mScrollPos1);

//            GUILayout.Label(this._mAppUpdaterFsmOwner.Context.InfoStringBuilder.ToString());            

//            GUILayout.EndScrollView();
//            GUILayout.EndVertical();

//            GUILayout.EndVertical();
//            GUI.DragWindow(new Rect(0, 0, Screen.width, Screen.height));

//        }


//        public bool ShowAskAppVersionUpdateWin { set; get; } = false;
//        public bool ShowAskResUpdateWin { set; get; } = false;

//        private void DrawOtherWindow()
//        {
//            if (this.ShowAskAppVersionUpdateWin)
//            {
//                GUILayout.Window(10, new Rect((Screen.width - 600) / 2, (Screen.height - 300) / 2, 600, 300), minVerConfirm, "+检测版本更新+");
//            }

//            if (this.ShowAskResUpdateWin)
//            {
//                GUILayout.Window(11, new Rect((Screen.width - 600) / 2, (Screen.height - 300) / 2, 600, 300), netConfirm, "+检测资源更新+");
//            }
//        }

//        void minVerConfirm(int windowid)
//        {
//            GUILayout.Label("热更新系统检测有新版本，是否更新？", GUILayout.Height(100));
//            GUILayout.BeginHorizontal();
//            if (GUILayout.Button("下载最新版本", GUILayout.Height(100)))
//            {
//                //this._mAppUpdaterFsmOwner.PerformAppUpdateOperation();
//                this.ShowAskAppVersionUpdateWin = false;
//            }
//            if (GUILayout.Button("跳过最新版本", GUILayout.Height(100)))
//            {
//                //this._mAppUpdaterFsmOwner.SkipAppUpdateOperation();
//                this.ShowAskAppVersionUpdateWin = false;
//            }
//            GUILayout.EndHorizontal();
//        }

//        private static string[] SizeName = new string[] { "B", "KB", "MB", "GB", "TB" };
//        public string GetSizeStr(double size)
//        {
//            string result = null;
//            int sizeIndex = 0;
//            while (size > 1024)
//            {
//                size = size / 1024.0f;
//                sizeIndex++;
//            }
//            result = string.Concat(size.ToString("f2"), SizeName[sizeIndex]);
//            return result;
//        }

//        void netConfirm(int windowid)
//        {
//            GUILayout.Label(string.Format("热更新系统检测到需要下载{0}资源，是否更新？", GetSizeStr(this._mAppUpdaterFsmOwner.Context.TotalDownloadSize)), GUILayout.Height(100));
//            GUILayout.BeginHorizontal();
//            if (GUILayout.Button("下载最新资源", GUILayout.Height(100)))
//            {
//                //this._mAppUpdaterFsmOwner.PerformHotFixOperation();

//                this.ShowAskResUpdateWin = false;
//            }
//            if (GUILayout.Button("跳过下载最新资源", GUILayout.Height(100)))
//            {
//                //this._mAppUpdaterFsmOwner.SkipHotFixOperation();
//                this.ShowAskResUpdateWin = false;
//            }
//            GUILayout.EndHorizontal();
//        }
//    }
//}
