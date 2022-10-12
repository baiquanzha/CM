using MTool.AppUpdaterLib.Runtime.Managers;
using MTool.AppUpdaterLib.Runtime.ResManifestParser;
using System;
using UnityEngine;

namespace MTool.AppUpdaterLib.Runtime.Diagnostics
{
    internal class AppUpdaterDebugView : MonoBehaviour
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private AppUpdaterService mService;

        //private static readonly ILog s_mLogger = LogManager.GetLogger("AppUpdaterDebugView");

        #endregion

        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        private void Awake()
        {
            this.mService = gameObject.GetComponent<AppUpdaterService>();

            if (this.mService == null)
            {
                this.enabled = false;
                throw new InvalidOperationException("GetComponent('AppUpdaterService')");
            }
        }

        private void Start()
        {
            float width = Screen.width * .8f;
            float height = Screen.height * .8f;
            this.mWindPos = new Rect((Screen.width - width) / 2, (Screen.height - height) / 2, width, height);
        }

        #endregion

        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private Vector2 mScrollPos0 = Vector2.zero;
        private Vector2 mScrollPos1 = Vector2.zero;
        private void OnGUI()
        {
            this.DrawGUI();
        }

        public bool Show = true ;
        private Rect mWindPos;
        public void DrawGUI()
        {
            if (!Show)
            {
                return;
            }

            this.mWindPos = GUILayout.Window(1, this.mWindPos, DrawMainWin, "AppUpdater Debug Window");

        }

        private void DrawMainWin(int id)
        {
            GUILayout.BeginVertical();

            GUILayout.BeginVertical("Box");
            GUILayout.Label($"组件版本 : {AppUpdaterVersion.GetVersionInfo()}");
            GUILayout.Label($"客户端唯一id : {AppUpdaterManager.ClientUniqueId}");
#if DEBUG_APP_UPDATER

            GUILayout.Label($"版本 : {AppVersionManager.GetAppVersionDesc()}");
            
#endif
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");

            GUILayout.BeginHorizontal();
#if DEBUG_APP_UPDATER
            if (GUILayout.Button("启动热更",GUILayout.Width(Screen.width * (1/8.0f))))
            {

                AppUpdaterManager.AppUpdaterStartUpdateAgain();

            }
#endif
            if (GUILayout.Button("关闭调试窗口", GUILayout.Width(Screen.width * (1 / 8.0f))))
            {
                Show = false;
            }

            //if (GUILayout.Button("打开日志窗口", GUILayout.Width(Screen.width * (1 / 8.0f))))
            //{
            //    var reporter = FindObjectOfType<Reporter>();
            //    if (reporter != null)
            //        reporter.doShow();
            //}

            GUILayout.EndHorizontal();
            

            GUILayout.EndVertical();


            this.mScrollPos0 = GUILayout.BeginScrollView(mScrollPos0);
            GUILayout.BeginVertical("Box");

            GUILayout.Label("一般信息");
            GUILayout.Label(VersionManifestParser.Pools.ToString());
            GUILayout.Label(this.mService.Context.ToString());
            GUILayout.EndVertical();

            GUILayout.EndScrollView();

            GUILayout.BeginVertical("Box");
            GUILayout.Label("运行时信息 : ");
            this.mScrollPos1 = GUILayout.BeginScrollView(this.mScrollPos1);

            GUILayout.Label(this.mService.Context.InfoStringBuilder.ToString());

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, Screen.width, Screen.height));
        }

#endregion

    }
}
