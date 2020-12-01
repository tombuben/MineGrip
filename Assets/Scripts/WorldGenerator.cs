using System;
using UnityEngine;

/// <summary>
/// A component managing the generation of the world.
/// Generates the world using Compute Shaders on the GPU, no CPU-only generation currently available
/// </summary>
public class WorldGenerator : MonoBehaviour
{
    public ComputeShader shader = null;
    private bool generateUsingComputeShaders = true;

    private int _kernelHandle;
    private uint _kernelThreadsX, _kernelThreadsY, _kernelThreadsZ;

    private void Awake()
    {
        // Some platforms don't support compute shaders (mainly WebGL)
        if (!SystemInfo.supportsComputeShaders || shader == null)
        {       
            Debug.LogError("Compute shader not available or not set up!");
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
    /// Generates a given chunk on the GPU, throws a NotImplementedException if Compute Shaders not available or enabled
    /// </summary>
    /// <param name="worldPosition">Position of the chunk in the world</param>
    /// <param name="chunkData">Array to be populated with generated chunk</param>
    public void GenerateChunk(Vector3Int worldPosition, ref sbyte[,,] chunkData)
    {
        if (generateUsingComputeShaders)
        {
            GenerateChunkGPU(worldPosition, ref chunkData);
        }
        else
        {
            Debug.LogError("CPU generation not implemented!");
            throw new NotImplementedException("WebGL is not a target platform, so CPU generation not implemented");
        }
    }

    /// <summary>
    /// Runs the compute shader generating the world and populates given chunkData with the output
    /// </summary>
    /// <param name="worldPosition">Position of the chunk in the world</param>
    /// <param name="chunkData">Array to be populated with generated chunk</param>
    private void GenerateChunkGPU(Vector3Int worldPosition, ref sbyte[,,] chunkData)
    {
        //Set up the buffers, smallest type in HLSL is int, so we can't use sbyte
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

        // Convert the int buffer to the sbyte output
        for (var x = 0; x < shaderData.GetLength(0); x++)
            for (var y = 0; y < shaderData.GetLength(1); y++)
                for (var z = 0; z < shaderData.GetLength(2); z++)
                    chunkData[x, y, z] = (sbyte) shaderData[x, y, z];
    }

}
