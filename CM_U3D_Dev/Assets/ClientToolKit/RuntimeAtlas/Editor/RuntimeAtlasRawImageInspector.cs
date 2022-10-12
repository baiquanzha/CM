using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using MTool.RuntimeAtlas.Runtime;

namespace MTool.RuntimeAtlas.Editor
{
    [CustomEditor(typeof(RuntimeAtlasRawImage))]
    public class RuntimeAtlasRawImageInspector : RawImageEditor
    {
        private Texture lastTexture = null;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            RuntimeAtlasRawImage script = target as RuntimeAtlasRawImage;
            GUILayout.Space(5);
            EditorGUILayout.LabelField("--------------------------------------------------------------------------------------------------------------------");
            GUILayout.Space(5);
            script.AtlasGroup = (RuntimeAtlasGroup)EditorGUILayout.EnumPopup("Group", script.AtlasGroup);
            EditorGUILayout.LabelField("Texture Path", script.Path);
            GUILayout.Space(5);

            if (EditorApplication.isPlaying)
            {
                if (GUILayout.Button("Show RuntimeAtlas"))
                {
                    RuntimeAtlasWindow.ShowWindow(script.AtlasGroup);
                }
            }
            else
            {
                if ((lastTexture != script.texture) || (script.texture != null && string.IsNullOrEmpty(script.Path)))
                {
                    lastTexture = script.texture;
                    script.Path = AssetDatabase.GetAssetPath(script.texture);
                }

                if (script.texture == null)
                {
                    lastTexture = null;
                    script.Path = string.Empty;
                }
            }
            EditorGUILayout.LabelField("--------------------------------------------------------------------------------------------------------------------");
            EditorGUILayout.LabelField("--------------------------------------------------------------------------------------------------------------------");
        }
    }
}