using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WorldGenerator))]
public class WorldGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}

//[CreateAssetMenu(fileName = "voxelTypes", menuName = "VoxelTypesAtlas")]
public class WorldGenerator : MonoBehaviour
{
    public int seed = 0;
    
    public RenderTexture noise;
    public ComputeShader shader = null;

    private int _kernelHandle;

    const int texWidth = 256;
    const int texHeight = 256;
    uint groupWidth, groupHeight, groupDepth;
    
    private void Start()
    {
        _kernelHandle = shader.FindKernel("CSMain");
        shader.GetKernelThreadGroupSizes(_kernelHandle, out groupWidth, out groupHeight, out groupDepth);
        
        noise = new RenderTexture(texWidth,texHeight,24);
        noise.enableRandomWrite = true;
        noise.Create();
        
        shader.SetTexture(_kernelHandle, "result", noise);
    }

    private void Update()
    {
        shader.Dispatch(_kernelHandle, (int) (texWidth/groupWidth), (int) (texHeight/groupHeight), 1);
    }

    public void GenerateChunk(int x, int y, int z, ref sbyte[,,] chunkData)
    {
        var buffer = new ComputeBuffer(chunkData.Length, sizeof(sbyte));
        shader.SetBuffer(_kernelHandle, "chunkData", buffer);
        shader.SetInts("buffer_size", chunkData.GetLength(0), chunkData.GetLength(1), chunkData.GetLength(2));
        shader.Dispatch(_kernelHandle, (int) (texWidth/groupWidth), (int) (texHeight/groupHeight), 1);
        buffer.GetData(chunkData);
    }
}
