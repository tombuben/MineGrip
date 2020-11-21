using System;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public ComputeShader shader = null;
    public bool generateUsingComputeShaders = true;

    private int _kernelHandle;
    private uint _kernelThreadsX, _kernelThreadsY, _kernelThreadsZ;

    private void Awake()
    {
        // Some platforms don't support compute shaders (mainly WebGL)
        if (!SystemInfo.supportsComputeShaders || shader == null)
        {
            generateUsingComputeShaders = false;
            return;
        }

        if (generateUsingComputeShaders)
        {
            _kernelHandle = shader.FindKernel("CSMain");
            shader.GetKernelThreadGroupSizes(_kernelHandle, out _kernelThreadsX, out _kernelThreadsY,
                out _kernelThreadsZ);
        }
    }
    
    /// <summary>
    /// Generates a given chunk, either on the CPU or on a GPU
    /// </summary>
    /// <param name="worldPosition">Chunk position</param>
    /// <param name="chunkData"></param>
    public void GenerateChunk(Vector3Int worldPosition, ref sbyte[,,] chunkData)
    {
        if (generateUsingComputeShaders)
        {
            GenerateChunkGPU(worldPosition, ref chunkData);
        }
        else
        {
            Debug.LogError("CPU generation not implemented yet!"); 
            GenerateChunkCPU(worldPosition, ref chunkData);
        }
    }

    private void GenerateChunkGPU(Vector3Int worldPosition, ref sbyte[,,] chunkData)
    {
        var buffer = new ComputeBuffer(chunkData.Length, sizeof(int));
        shader.SetBuffer(_kernelHandle, "voxel_buffer", buffer);
        shader.SetInts("buffer_size", chunkData.GetLength(0), chunkData.GetLength(1), chunkData.GetLength(2));
        shader.SetInts("world_position", worldPosition.x, worldPosition.y, worldPosition.z);

        var groupCountX = (int) (chunkData.GetLength(0) / _kernelThreadsX);
        var groupCountZ = (int) (chunkData.GetLength(2) / _kernelThreadsZ);
        shader.Dispatch(_kernelHandle, groupCountX, 1, groupCountZ);

        var shaderData = new int[chunkData.GetLength(0), chunkData.GetLength(1), chunkData.GetLength(2)];
        buffer.GetData(shaderData);
        buffer.Dispose();

        for (var x = 0; x < shaderData.GetLength(0); x++)
            for (var y = 0; y < shaderData.GetLength(1); y++)
                for (var z = 0; z < shaderData.GetLength(2); z++)
                    chunkData[x, y, z] = (sbyte) shaderData[x, y, z];
    }

    private void GenerateChunkCPU(Vector3Int worldPosition, ref sbyte[,,] chunkData)
    {
        throw new NotImplementedException();
        for (var x = 0; x < chunkData.GetLength(0); x++)
        {
            for (var z = 0; z < chunkData.GetLength(2); z++)
            {
                // Here should be a reimplementation of the compute shader
            }
        }
        
    }
}
