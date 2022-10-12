using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UI;
using MTool.RuntimeAtlas.Runtime;

namespace MTool.RuntimeAtlas.Editor
{
    public class RuntimeAtlasWindow : EditorWindow
    {
        private static RuntimeAtlasWindow Window;
        private RuntimeAtlasGroup Group;
        private float scale = 0.2f;
        private List<Texture2D> Texture2Ds = new List<Texture2D>();
        private Color32[] FillColor;
        private bool isShowFreeAreas = false;
        private bool isRefreshFreeAreas = true;
        private readonly float formPosY = 62;

        public static void ShowWindow(RuntimeAtlasGroup group)
        {
            if (Window == null)
                Window = GetWindow<RuntimeAtlasWindow>();
            Window.Show();
            Window.Init(group);
            Window.titleContent.text = "RuntimeAtlas";
        }

        public void Init(RuntimeAtlasGroup group)
        {
            Group = group;
        }

        public void OnGUI()
        {
            if (EditorApplication.isPlaying == false)
            {
                Window.Close();
                return;
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("--------------------------------------------------------------------------------");
            Runtime.RuntimeAtlas runtimeAtlas = RuntimeAtlasManager.Instance.GetRuntimeAtlas(Group, true);
            EditorGUILayout.LabelField($"图集尺寸：{runtimeAtlas.AtlasWidth} x {runtimeAtlas.AtlasHeight}");
            EditorGUILayout.LabelField("--------------------------------------------------------------------------------");
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
            EditorGUILayout.BeginVertical();
            isShowFreeAreas = GUILayout.Toggle(isShowFreeAreas, "Show Free Areas", GUILayout.Width(200), GUILayout.Height(20));
            EditorGUILayout.BeginHorizontal();
            if (isShowFreeAreas)
            {
                if (GUILayout.Button("Refresh and Clear Free Area", GUILayout.Width(200), GUILayout.Height(20)))
                {
                    isRefreshFreeAreas = true;
                    ClearFreeAreas();
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            scale = EditorGUILayout.Slider(scale, 0.2f, 1);
            EditorGUILayout.EndHorizontal();

            if (runtimeAtlas.UsingCopyTexture)
            {
                List<Texture2D> texture2Ds = runtimeAtlas.Texture2Ds;
                int count = texture2Ds.Count;
                for (int i = 0; i < count; i++)
                {
                    Texture2D tex2D = texture2Ds[i];
                    float posX = (i + 1) * 10 + i * runtimeAtlas.AtlasWidth * scale;
                    if (isShowFreeAreas)
                        DrawFreeArea(i, runtimeAtlas);
                    GUI.DrawTexture(new Rect(posX, formPosY, runtimeAtlas.AtlasWidth * scale, runtimeAtlas.AtlasHeight * scale), tex2D);
                }
            }
            else
            {
                List<RenderTexture> renderTexList = runtimeAtlas.RenderTextures;
                int count = renderTexList.Count;
                for (int i = 0; i < count; i++)
                {
                    float posX = (i + 1) * 10 + i * runtimeAtlas.AtlasWidth * scale;
                    if (isShowFreeAreas)
                        DrawFreeArea(i, runtimeAtlas);
                    GUI.DrawTexture(new Rect(posX, formPosY, runtimeAtlas.AtlasWidth * scale, runtimeAtlas.AtlasHeight * scale), renderTexList[i]);
                }
            }

            if (isShowFreeAreas)
                isRefreshFreeAreas = false;
        }

        private void DrawFreeArea(int index, Runtime.RuntimeAtlas runtimeAtlas)
        {
            Texture2D tex2D = null;
            if (Texture2Ds.Count < index + 1)
            {
                int size = (int)Group;
                tex2D = new Texture2D(size, size, TextureFormat.ARGB32, false, true);
                Texture2Ds.Add(tex2D);
                if (FillColor == null)
                {
                    FillColor = tex2D.GetPixels32();
                    for (int i = 0; i < FillColor.Length; i++)
                        FillColor[i] = Color.clear;
                }
            }
            else
                tex2D = Texture2Ds[index];
            tex2D.SetPixels32(FillColor);
            if (isRefreshFreeAreas)
            {
                Color32[] tmpColor;
                List<IntegerRectangle> freeList = runtimeAtlas.FreeAreas[index];
                foreach (IntegerRectangle rect in freeList)
                {
                    int size = rect.Width * rect.Height;
                    tmpColor = new Color32[size];
                    for (int i = 0; i < size; i++)
                        tmpColor[i] = Color.green;
                    tex2D.SetPixels32(rect.X, rect.Y, rect.Width, rect.Height, tmpColor);
                    int outLineSize = 2;
                    if (rect.Width < outLineSize * 2 || rect.Height < outLineSize * 2)
                        outLineSize = 0;
                    size -= outLineSize * 4;
                    tmpColor = new Color32[size];
                    for(int i = 0; i < size; i++)
                        tmpColor[i] = Color.yellow;
                    tex2D.SetPixels32(rect.X + outLineSize, rect.Y + outLineSize, rect.Width - outLineSize * 2, rect.Height - outLineSize * 2, tmpColor);
                    tex2D.Apply();
                }
            }

            float posX = (index + 1) * 10 + index * runtimeAtlas.AtlasWidth * scale;
            GUI.DrawTexture(new Rect(posX, formPosY, runtimeAtlas.AtlasWidth * scale, runtimeAtlas.AtlasHeight * scale), tex2D);
        }

        private void ClearFreeAreas()
        {
            Runtime.RuntimeAtlas runtimeAtlas = RuntimeAtlasManager.Instance.GetRuntimeAtlas(Group, true);
            if (runtimeAtlas.UsingCopyTexture)
            {
                List<List<IntegerRectangle>> freeAreas = runtimeAtlas.FreeAreas;
                int freeAreaCount = freeAreas.Count;
                List<Texture2D> texture2Ds = runtimeAtlas.Texture2Ds;
                for (int i = 0; i < freeAreaCount; i++)
                {
                    var list = freeAreas[i];
                    int listCount = list.Count;
                    Texture2D dstTex = texture2Ds[i];
                    for (int j = 0; j < listCount; j++)
                    {
                        IntegerRectangle rect = list[j];
                        Color32[] colors = new Color32[rect.Width * rect.Height];
                        for (int k = 0; k < colors.Length; k++)
                            colors[k] = Color.clear;
                        dstTex.SetPixels32(rect.X, rect.Y, rect.Width, rect.Height, colors);
                        dstTex.Apply();
                    }
                }
            }
            else
                runtimeAtlas.ClearTextureWithBlit();
        }
    }
}