using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MTool.AppUpdaterLib.Runtime;
using MTool.Core.Functional;
using MTool.LoggerModule.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using ILogger = MTool.LoggerModule.Runtime.ILogger;
using Object = UnityEngine.Object;

namespace MTool.AssetBundleManager.Runtime.Diagnostics
{
    public class AssetBundleManagerDebugView : MonoBehaviour
    {
        private Rect mWindPos;
        private StringBuilder mBaseInfoSb = new StringBuilder();

        private bool mInitialize = false;

        private ABConfigHandle mAbConfigHandle;

        private static readonly Lazy<LoggerModule.Runtime.ILogger> s_mLogger = new Lazy<ILogger>(() =>
            LoggerManager.GetLogger("AssetBundleManager"));

        public void Init(ABConfigHandle handle)
        {
            if (this.mInitialize)
                return;
            this.mAbConfigHandle = handle;
            this.InitializeComponent();

            this.mInitialize = true;
        }


        private void InitializeComponent()
        {
            float width = Screen.width * .8f;
            float height = Screen.height * .8f;
            this.mWindPos = new Rect((Screen.width - width) / 2, (Screen.height - height) / 2, width, height);
        }


        public void DrawGUI()
        {
            this.GrabResourceSystemInfo();

            this.mWindPos = GUILayout.Window(100, this.mWindPos, DrawWin, "AB Debug View");
        }

        private Vector2 mAbScrollPos = Vector2.zero;
        private Vector2 mAbScrollPos1 = Vector2.zero;

        private Vector2 mAbRequestScrollPos = Vector2.zero;
        private Vector2 mAbRequestScrollPos1 = Vector2.zero;

        private ABItem mCurSelectedAbItem = null;
        private ABRequest mCurSelectedAbRequest = null;


        private string[] names = { "ABInfo", "ABRequestInfo" };
        private int selectedAbViewIdx = 0;

        private static readonly Dictionary<string, Object> mLoadedDic = new Dictionary<string, Object>();

        private void DrawWin(int id)
        {
            GUILayout.BeginVertical();

            GUILayout.BeginVertical("Box");
            GUILayout.Label(this.mBaseInfoSb.ToString());
            GUILayout.EndVertical();

#if UNITY_EDITOR
            if (!AssetBundleManager.SimulateAssetBundleInEditor)
#else
#endif
            {
                GUILayout.Space(5);
                this.selectedAbViewIdx = GUILayout.SelectionGrid(selectedAbViewIdx, names, 2);
                GUILayout.Space(5);
                GUILayout.BeginVertical("Box", GUILayout.Height(Screen.height * 0.4f));
                GUILayout.BeginHorizontal("Box");

                switch (this.selectedAbViewIdx)
                {
                    case 0:
                        this.DrawAbItemInfo();
                        break;
                    case 1:
                        this.DrawRequestInfo();
                        break;
                    default:
                        break;
                }

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
           

            GUILayout.BeginHorizontal("Box");

            if (GUILayout.Button("Load TanksExample Single"))
            {
                AssetBundleManager.LoadScene("tanksexample");
            }

            if (GUILayout.Button("UnLoad tanksexample"))
            {
#pragma warning disable 618
                AssetBundleManager.UnLoadScene("tanksexample");
#pragma warning restore 618
            }

            if (GUILayout.Button("Load TanksExample Single Async"))
            {
                AssetBundleManager.LoadSceneAsync("tanksexample");
            }

            if (GUILayout.Button("UnLoad tanksexample Async"))
            {
                AssetBundleManager.UnLoadSceneAsync("tanksexample");
            }

            if (GUILayout.Button("Load TanksExample Additive"))
            {
                AssetBundleManager.LoadScene("tanksexample", LoadSceneMode.Additive);
            }

            if (GUILayout.Button("Async Load TanksExample Additive"))
            {
                AssetBundleManager.LoadSceneAsync("tanksexample", LoadSceneMode.Additive);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("Box");

            if (GUILayout.Button("Load testscene Single"))
            {
                AssetBundleManager.LoadScene("testscene");
            }

            if (GUILayout.Button("UnLoad testscene"))
            {
#pragma warning disable 618
                AssetBundleManager.UnLoadScene("testscene");
#pragma warning restore 618
            }

            if (GUILayout.Button("Load testscene Single Async"))
            {
                AssetBundleManager.LoadSceneAsync("testscene");
            }

            if (GUILayout.Button("UnLoad testscene Async"))
            {
                AssetBundleManager.UnLoadSceneAsync("testscene");
            }

            if (GUILayout.Button("Load testscene Additive"))
            {
                AssetBundleManager.LoadScene("testscene", LoadSceneMode.Additive);
            }

            if (GUILayout.Button("Async Load testscene Additive"))
            {
                AssetBundleManager.LoadSceneAsync("testscene", LoadSceneMode.Additive);
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, Screen.width, Screen.height));

        }

        private void DrawAbItemInfo()
        {
            this.mAbScrollPos = GUILayout.BeginScrollView(this.mAbScrollPos, GUILayout.Width(Screen.width * 0.5f));

            this.mAbConfigHandle?.ABItemList.ForeachCall(v =>
            {
                if (GUILayout.Button(v.Key))
                    this.mCurSelectedAbItem = v.Value;
            });

            GUILayout.EndScrollView();

            GUILayout.Space(10);

            GUILayout.BeginVertical(GUILayout.Width(Screen.width * 0.2f));

            if (this.mCurSelectedAbItem == null)
                GUILayout.Label("Pleause select a ab item");
            else
            {
                GUILayout.BeginHorizontal();

                var itemName = this.mCurSelectedAbItem.Table.Name;
                if (itemName.StartsWith("abscene/"))
                {
                    if (GUILayout.Button("Load Scene"))
                    {
                        
                        AssetBundleManager.LoadScene(GetSceneName(this.mCurSelectedAbItem.Table.Name));
                    }

                    if (GUILayout.Button("Unload Scene"))
                    {
#pragma warning disable 618
                        AssetBundleManager.UnLoadScene(GetSceneName(this.mCurSelectedAbItem.Table.Name));
#pragma warning restore 618
                    }

                    if (GUILayout.Button("Load Scene Async"))
                    {
                        this.StartCoroutine(this.LoadSceneAsync(this.mCurSelectedAbItem.Table.Name));
                    }
                }
                else
                {
                    if (GUILayout.Button("Load"))
                    {
                        AssetBundleManager.Load(this.mCurSelectedAbItem.Table.Name,typeof(UnityEngine.Object));
                    }

                    if (GUILayout.Button("LoadAndInit"))
                    {
                        AssetBundleManager.LoadAndInit(this.mCurSelectedAbItem.Table.Name, obj =>
                        {
                            s_mLogger.Value.Info($"Type : {obj.GetType().Name}");
                        });
                    }
                }

                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.Label($"Refer count : {this.mCurSelectedAbItem.UseCount}");

                this.mAbScrollPos1 = GUILayout.BeginScrollView(this.mAbScrollPos1);

                GUILayout.Label("Dependencies : ");

                if (this.mCurSelectedAbItem.Dependencies != null)
                    this.mCurSelectedAbItem.Dependencies.ForeachCall(x =>
                    {
                        GUILayout.Label($"{x.Table.Name}");
                    });
                GUILayout.Space(4);

                GUILayout.EndScrollView();
            }

            GUILayout.EndVertical();
        }

        string GetSceneName(string scenePath)
        {
            int idx = scenePath.LastIndexOf("/", StringComparison.Ordinal);

            if (idx != -1)
            {
                return scenePath.Substring(idx+1);
            }
            return scenePath;
        }
        IEnumerator LoadSceneAsync(string scenePath)
        {
            var op = AssetBundleManager.LoadSceneAsync(GetSceneName(scenePath));

            while (op != null && !op.isDone)
            {
                yield return null;
            }

            s_mLogger.Value.Info($"Load scene async that path is \"{scenePath}\" completed .");
        }

        private void DrawRequestInfo()
        {
            this.mAbRequestScrollPos = GUILayout.BeginScrollView(this.mAbRequestScrollPos, GUILayout.Width(Screen.width * 0.2f));

            this.mAbConfigHandle?.Cache.ForeachCall(v =>
            {
                var request = v.Value;
                if (request.IsDone)
                {
                    if (GUILayout.Button("Done" + v.Value.Path))
                        this.mCurSelectedAbRequest = v.Value;
                }
                else
                {
                    if (GUILayout.Button(v.Key))
                        this.mCurSelectedAbRequest = v.Value;

                }

            });

            GUILayout.EndScrollView();

            GUILayout.Space(10);

            GUILayout.BeginVertical(GUILayout.Width(Screen.width * 0.2f));

            if (this.mCurSelectedAbRequest == null)
                GUILayout.Label("Pleause select a request item");
            else
            {
                this.mAbRequestScrollPos1 = GUILayout.BeginScrollView(this.mAbRequestScrollPos1);

                GUILayout.Label($"Path : {this.mCurSelectedAbRequest.Path}");
                GUILayout.Label($"UserCount : {this.mCurSelectedAbRequest.UseCount}");
                GUILayout.Label($"IsDone : {this.mCurSelectedAbRequest.IsDone}");
                GUILayout.Label($"IsScene : {this.mCurSelectedAbRequest.IsScene}");


                GUILayout.EndScrollView();
            }

            GUILayout.EndVertical();
        }

        void OnGUI()
        {
            if (!this.mInitialize)
                return;
            this.DrawGUI();
        }

        void GrabResourceSystemInfo()
        {
            this.mBaseInfoSb.Clear();

            //Base Info
            if (AssetsFileSystem.ConfigInfoClient != null)
                this.mBaseInfoSb.AppendLine($"App Version : {AssetsFileSystem.ConfigInfoClient.Ver}");
#if UNITY_EDITOR
            this.mBaseInfoSb.AppendLine($"Resource Mode : {(AssetBundleManager.SimulateAssetBundleInEditor ? "Editor simulation" : "Unity resource")} ");
#else
            this.mBaseInfoSb.AppendLine($"Resource Mode : Unity resource ");
#endif


            if (this.mAbConfigHandle != null)
            {
                this.mBaseInfoSb.AppendLine($"Total ab count : {this.mAbConfigHandle.ABItemList.Count}");
                this.mBaseInfoSb.AppendLine($"Total ab request count : {this.mAbConfigHandle.Cache.Count}");
                this.mBaseInfoSb.AppendLine($"Current loading ab count : {this.mAbConfigHandle.updateList.Count}");
                int result = this.mAbConfigHandle.Cache.FoldL(
                    (a, b) =>
                    {
                        if (b.Value.IsDone) return a + 1;
                        return a;
                    }, 0);

                this.mBaseInfoSb.AppendLine($"Current loaded request count : {result}");
            }
            
        }

    }
}


