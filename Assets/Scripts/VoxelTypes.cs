﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

[CreateAssetMenu(fileName = "voxelTypes", menuName = "VoxelTypesAtlas")]
public class VoxelTypes : ScriptableObject
{
    public Texture2D atlas;
    public Material material;
    
    public int texturePerSide;
    
    /// <summary>
    /// An x by 6 list where x is the number of known voxel block types containing
    /// the atlas position of each of the sides of the voxel cube textures
    /// </summary>
    public List<int> typeTextures = new List<int>(){0,0,0,0,0,0};

    private void OnEnable()
    {
        var textureCount = texturePerSide * texturePerSide;
        AtlasCubeFaceUvs = new Vector2[textureCount, 6];
        for (var i = 0; i < textureCount; i++)
        {
            var faceUVs = CubeFaces.getUVsOfRect(GetAtlasPositionUvs(i));
            for (var face = 0; face < 6; face++)
            {
                AtlasCubeFaceUvs[i, face] = faceUVs[face];
            }
        }
    }
    
    private void OnValidate()
    {
        if (texturePerSide <= 0) texturePerSide = 1;
    }

    public Rect GetAtlasFaceUvs(int type, int face)
    {
        return GetAtlasPositionUvs(typeTextures[type * 6 + face]);
    }

    public Vector2[,] AtlasCubeFaceUvs { get; private set; }

    public int GetAtlasPosition(int type, int face)
    {
        return typeTextures[type * 6 + face];
    }
    
    /// <summary>
    /// Get a rect representing the uv's of a given atlas position
    /// </summary>
    /// <param name="atlasPosition">Position in the atlas (starts in bottom left)</param>
    /// <returns></returns>
    public Rect GetAtlasPositionUvs(int atlasPosition)
    {
        if (texturePerSide <= 0) texturePerSide = 1;
        var lenght = 1.0f / texturePerSide;
        var xAtlas = atlasPosition % texturePerSide;
        var yAtlas = atlasPosition / texturePerSide;
        
        var minPos = new Vector2(xAtlas * lenght, yAtlas * lenght);
        
        return new Rect(minPos, new Vector2(lenght, lenght));
    }
}
