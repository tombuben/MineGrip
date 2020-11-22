using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VoxelTypes))]
public class VoxelTypesEditor : Editor
{
    private SerializedProperty _textureAtlas;
    private SerializedProperty _material;
    private SerializedProperty _texturePerSide;
    private SerializedProperty _typeTextures;

    private bool _texturesFoldout = false;
    private VoxelTypes _voxelTypes;
    
    void OnEnable()
    {
        _textureAtlas = serializedObject.FindProperty("atlas");
        _material = serializedObject.FindProperty("material");
        _texturePerSide = serializedObject.FindProperty("texturePerSide");
        _typeTextures = serializedObject.FindProperty("typeTextures");
        _voxelTypes = (VoxelTypes) target;
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(_textureAtlas);
        EditorGUILayout.PropertyField(_material);
        EditorGUILayout.PropertyField(_texturePerSide);


        _texturesFoldout = EditorGUILayout.Foldout(_texturesFoldout, "Types", true);
        if (_texturesFoldout)
        {

            const int previewSize = 50;
            const int padding = 2;
            var rect = EditorGUILayout.GetControlRect(false);
            var subrect = new Rect(rect) {width = previewSize};
            var style = new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter};
            GUI.Label(subrect, "Z+", style);
            subrect.x += previewSize + padding;
            GUI.Label(subrect, "X+", style);
            subrect.x += previewSize + padding;
            GUI.Label(subrect, "Z-", style);
            subrect.x += previewSize + padding;
            GUI.Label(subrect, "X-", style);
            subrect.x += previewSize + padding;
            GUI.Label(subrect, "Y-", style);
            subrect.x += previewSize + padding;
            GUI.Label(subrect, "Y+", style);
            subrect.x += previewSize + padding;
            
            for (var typeNum = 1; typeNum < _typeTextures.arraySize / 6; typeNum++)
            {
                VoxelTextureSelector(typeNum, previewSize, padding);
            }

            if (GUILayout.Button("+"))
            {
                _typeTextures.arraySize += 6;
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void VoxelTextureSelector(int typeNum, int previewSize, int padding)
    {
        var rect = EditorGUILayout.GetControlRect(false, previewSize);
        var subrect = new Rect(rect) {width = previewSize};

        for (var face = 0; face < 6; face++)
        {
            if (GUI.Button(subrect, ""))
            {
                var atlasPicker = ScriptableObject.CreateInstance<TextureAtlasPickerEditor>();
                atlasPicker.voxeltypes = _voxelTypes;
                atlasPicker.targetFacePosition = typeNum * 6 + face;
                var width = previewSize * _texturePerSide.intValue + 50;
                var height = previewSize * _texturePerSide.intValue + 100;

                var windowPos = EditorGUIUtility.GUIToScreenRect(subrect); 
                atlasPicker.ShowAsDropDown(windowPos, new Vector2(width, height));

            }
            GUI.DrawTextureWithTexCoords(subrect, _voxelTypes.atlas, _voxelTypes.GetAtlasFaceUvs(typeNum, face) );

            subrect.x += previewSize + padding;
        }

        subrect.x += previewSize / 2;
        if (GUI.Button(subrect, "-"))
        {
            for (var face = 0; face < 6; face++)
            {
                _typeTextures.DeleteArrayElementAtIndex(typeNum*6);
            }
        }

        GUILayout.Space(padding * 2);
    }
}
