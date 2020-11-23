using System;
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
    private SerializedProperty _typeDurability;

    private bool _texturesFoldout = false;
    private VoxelTypes _voxelTypes;
    
    void OnEnable()
    {
        _textureAtlas = serializedObject.FindProperty("atlas");
        _material = serializedObject.FindProperty("material");
        _texturePerSide = serializedObject.FindProperty("texturePerSide");
        _typeTextures = serializedObject.FindProperty("typeTextures");
        _typeDurability = serializedObject.FindProperty("typeDurability");
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
            subrect.width = rect.width - subrect.x;
            style.alignment = TextAnchor.MiddleLeft;
            GUI.Label(subrect, "Durability", style);
            subrect.x += previewSize + padding;
            
            for (var typeNum = 1; typeNum < _typeTextures.arraySize / 6; typeNum++)
            {
                VoxelTextureSelector(typeNum, previewSize, padding);
            }

            if (GUILayout.Button("+"))
            {
                _typeTextures.arraySize += 6;
                _typeDurability.arraySize += 1;
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

        
        var durability = _typeDurability.GetArrayElementAtIndex(typeNum);
        var durRect = new Rect(subrect);
        durRect.height = previewSize / 2.0f;
        durRect.width = previewSize / 2.0f;
        durRect.y += previewSize / 4.0f;
        durRect.x += previewSize / 4.0f;
        EditorGUI.PropertyField(durRect, durability, new GUIContent());
        
        subrect.x += previewSize + padding * 2;
        if (GUI.Button(subrect, "-"))
        {
            for (var face = 0; face < 6; face++)
            {
                _typeTextures.DeleteArrayElementAtIndex(typeNum*6);
            }
            _typeDurability.DeleteArrayElementAtIndex(typeNum);
        }

        GUILayout.Space(padding * 2);
    }
}
