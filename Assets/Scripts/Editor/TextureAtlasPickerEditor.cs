using System;
using UnityEditor;
using UnityEngine;

public class TextureAtlasPickerEditor : EditorWindow
{
    public VoxelTypes voxeltypes;
    public int targetFacePosition;
    
    private void OnGUI()
    {
        GUILayout.Label("Pick texture for face", EditorStyles.boldLabel);
        const int previewSize = 50;
        const int padding = 2;
        


        for (var y = voxeltypes.texturePerSide -1; y >= 0; y--)
        {
            var rect = EditorGUILayout.GetControlRect(false, previewSize);
            var subrect = new Rect(rect) {width = previewSize};
            for (var x = 0; x < voxeltypes.texturePerSide; x++)
            {
                var atlasPosition = y * voxeltypes.texturePerSide + x;
                
                if (GUI.Button(subrect, ""))
                {
                    voxeltypes.typeTextures[targetFacePosition] = atlasPosition;
                    this.Close();
                }

                GUI.DrawTextureWithTexCoords(subrect, voxeltypes.atlas, voxeltypes.GetAtlasPositionUvs(atlasPosition) );
                subrect.x += previewSize + padding;
            }
        }
    }
}