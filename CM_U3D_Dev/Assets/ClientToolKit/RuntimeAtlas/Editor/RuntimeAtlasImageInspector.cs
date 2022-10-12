using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using MTool.RuntimeAtlas.Runtime;

namespace MTool.RuntimeAtlas.Editor
{
    [CustomEditor(typeof(RuntimeAtlasImage))]
    public class RuntimeAtlasImageInspector : ImageEditor
    {
        private Sprite lastSprite = null;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            RuntimeAtlasImage script = target as RuntimeAtlasImage;
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
                if ((lastSprite != script.sprite) || (script.sprite != null && string.IsNullOrEmpty(script.Path)))
                {
                    lastSprite = script.sprite;
                    script.Path = AssetDatabase.GetAssetPath(script.sprite);
                }

                if (script.sprite == null)
                {
                    lastSprite = null;
                    script.Path = string.Empty;
                }
            }

            EditorGUILayout.LabelField("--------------------------------------------------------------------------------------------------------------------");
            EditorGUILayout.LabelField("--------------------------------------------------------------------------------------------------------------------");
        }
    }
}