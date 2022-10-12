using UnityEditor;
using XNodeEditor;

[NodeEditor.CustomNodeEditorAttribute(typeof(NamespaceTypesNodeDisplayNode))]
class NamespaceTypesNodeDisplayNodeEditor : NodeEditor
{

    public override void OnBodyGUI()
    {
        base.OnBodyGUI();

        var node = target as NamespaceTypesNodeDisplayNode;

        var fullNames = node.GetValue();
        if (fullNames != null && fullNames.Length > 0)
        {

            for (int i = 0; i < fullNames.Length; i++)
            {
                var fullName = fullNames[i];
                var typeName = fullName.Substring(fullName.LastIndexOf('.') + 1);


                EditorGUILayout.LabelField(typeName);
            }

        }
    }
}
