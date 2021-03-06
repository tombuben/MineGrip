﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using UnityEngine;

/// <summary>
/// Object representing a single chunk of voxels.
/// Stores a 3d array of voxels and is able to generate a mesh from it  
/// </summary>
[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class VoxelChunk : MonoBehaviour
{
    public VoxelTypes voxelTypes;
    public Vector3Int worldPosition;
    public WorldGenerator generator;

    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;

    private const uint ChunkSize = 32;

    public sbyte[,,] Voxels;
    public bool modified = false;

    private string savePath =>
        Application.persistentDataPath + "/" + worldPosition.x + "." + worldPosition.y + "." +
        worldPosition.z + ".chunk";

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        _meshRenderer.material = voxelTypes.material;
        
        if (!LoadChunk())
            GenerateVoxels();
        
        _meshFilter.mesh = CreateMesh();
    }

    private void OnDestroy()
    {
        if (modified)
            SaveChunk();
    }

    /// <summary>
    /// Dumps the chunk on the disk
    /// </summary>
    private void SaveChunk()
    {
        var bf = new BinaryFormatter();
        using (var ms = new MemoryStream())
        {
            bf.Serialize(ms, Voxels);
            var byteArr = ms.ToArray();
            File.WriteAllBytes(savePath, byteArr);
        }
    }

    /// <summary>
    /// Tries to load the chunk from the disk
    /// </summary>
    /// <returns>Was the chunk loaded from the disk</returns>
    private bool LoadChunk()
    {
        if (!File.Exists(savePath)) return false;
        
        var byteArr = File.ReadAllBytes(savePath);
        using (var memStream = new MemoryStream())
        {
            var binForm = new BinaryFormatter();
            memStream.Write(byteArr, 0, byteArr.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            var obj = binForm.Deserialize(memStream);
            Voxels = obj as sbyte[,,];
            return true;
        }
    }

    /// <summary>
    /// Generates the voxel data
    /// </summary>
    public void GenerateVoxels()
    {
        Voxels = new sbyte[ChunkSize, ChunkSize, ChunkSize];

        if (generator != null)
            generator.GenerateChunk(worldPosition, ref Voxels);
        else
        {
            //Init array
            Array.Clear(Voxels, 0, Voxels.Length);
            for (var x = 0; x < ChunkSize; x++)
            {
                for (var y = 0; y < ChunkSize; y++)
                {
                    Voxels[x, y, 0] = 1;
                }
            }
        }
    }

    /// <summary>
    /// Regenerates the current mesh based on the voxel data
    /// </summary>
    public void RegenerateMesh()
    {
        Destroy(_meshFilter.mesh);
        _meshFilter.mesh = CreateMesh();
    }
    
    /// <summary>
    /// Create a single mesh from the _voxels data structure.
    /// </summary>
    /// <returns>Single mesh of the entire chunk</returns>
    private Mesh CreateMesh()
    {
        var vertices = new List<Vector3>();
        vertices.Capacity = 30000;
        var uvs = new List<Vector2>();
        uvs.Capacity = 30000;

        UnityEngine.Profiling.Profiler.BeginSample("CreateMesh.AddingCubes");
        for (var x = 0; x < ChunkSize; x++)
        for (var y = 0; y < ChunkSize; y++)
        for (var z = 0; z < ChunkSize; z++)
            if (Voxels[x, y, z] != 0)
                AddCubeToMesh(new Vector3(x, y, z), Voxels[x, y, z], ref vertices, ref uvs);
        UnityEngine.Profiling.Profiler.EndSample();

        var mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = Enumerable.Range(0, vertices.Count).ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        return mesh;
    }

    /// <summary>
    /// Adds a single cube to the mesh, ignoring the occluded internal walls.
    ///
    /// Neighbor check done on the CPU, could be possibly optimized by using a
    /// compute shader and creating the mesh by using a lookup table (ala marching cube)
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="vertices"></param>
    /// <param name="uvs"></param>
    private void AddCubeToMesh(Vector3 pos, int type, ref List<Vector3> vertices, ref List<Vector2> uvs)
    {
        for (var face = 0; face < 6; face++)
        {
            var facepos = pos + CubeFaces.WallDirection[face];
            if (pos.x + CubeFaces.WallDirection[face].x < 0 ||
                pos.x + CubeFaces.WallDirection[face].x >= ChunkSize ||
                pos.y + CubeFaces.WallDirection[face].y < 0 ||
                pos.y + CubeFaces.WallDirection[face].y >= ChunkSize ||
                pos.z + CubeFaces.WallDirection[face].z < 0 ||
                pos.z + CubeFaces.WallDirection[face].z >= ChunkSize ||
                Voxels[
                    (int) (pos.x + CubeFaces.WallDirection[face].x),
                    (int) (pos.y + CubeFaces.WallDirection[face].y),
                    (int) (pos.z + CubeFaces.WallDirection[face].z)] == 0)
            {
                for (var i = 0; i < 6; i++)
                {
                    var index = CubeFaces.FaceIndices[face, i];
                    vertices.Add(CubeFaces.Vertices[index] + pos);
                    uvs.Add(voxelTypes.AtlasCubeFaceUvs[voxelTypes.GetAtlasPosition(type, face), i]);
                }
            }
        }
    }
}