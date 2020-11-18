using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class VoxelChunk : MonoBehaviour
{
    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;
    
    private const uint ChunkSize = 16;
    private sbyte[] _voxels = new sbyte[ChunkSize * ChunkSize * ChunkSize];

    private void Awake()
    {
        if (!SystemInfo.supportsComputeShaders)
        {
            Debug.LogError("No support for compute shaders found!");
            Destroy(this);
            return;
        }
        
        //Init array
        Array.Clear(_voxels, 0, _voxels.Length);
        _voxels[0] = 1;
        
        _meshFilter = GetComponent<MeshFilter>();
        _meshFilter.mesh = CreateMesh();
    }

    private Mesh CreateMesh()
    {
        var vertices = new List<Vector3>();
        var indices = new List<int>();
        var uvs = new List<Vector2>();
        var maxIndex = 0;
        
        
        for (int face = 0; face < 6; face++)
            for (int i = 0; i < 6; i++)
            {
                int index = CubeFaces.FaceIndices[face, i];
                vertices.Add(CubeFaces.Vertices[index]);
                uvs.Add(CubeFaces.UVs[i]);
                indices.Add(maxIndex++);
            }
        
        var mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.uv = uvs.ToArray();
        
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
