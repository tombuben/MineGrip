using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class VoxelChunk : MonoBehaviour
{
    public VoxelTypes voxelTypes;
    
    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;
    
    private const uint ChunkSize = 16;
    
    private sbyte[,,] _voxels = new sbyte[ChunkSize, ChunkSize, ChunkSize];

    private void Awake()
    {
        if (!SystemInfo.supportsComputeShaders)
        {
            Debug.LogError("No support for compute shaders found, constructing geometry on the CPU!");
            Destroy(this);
            return;
        }
        
        //Init array
        Array.Clear(_voxels, 0, _voxels.Length);
        
        
        for (var x = 0; x < ChunkSize; x++)
            for (var y = 0; y < ChunkSize; y++)
                for (var z = 0; z < ChunkSize; z++)
                    if (x + y + z <= ChunkSize)
                        _voxels[x, y, z] = 2;
        
        
        _meshFilter = GetComponent<MeshFilter>();
        _meshFilter.mesh = CreateMesh();
    }

    
    /// <summary>
    /// Create a single mesh from the _voxels data structure.
    /// </summary>
    /// <returns>Single mesh of the entire chunk</returns>
    private Mesh CreateMesh()
    {
        var vertices = new List<Vector3>();
        var uvs = new List<Vector2>();

        for (var x = 0; x < ChunkSize; x++)
            for (var y = 0; y < ChunkSize; y++)
                for (var z = 0; z < ChunkSize; z++)
                    if (_voxels[x,y,z] != 0)
                        AddCubeToMesh(new Vector3(x, y, z), _voxels[x,y,z], ref vertices, ref uvs);
        
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
                _voxels[
                    (int)(pos.x + CubeFaces.WallDirection[face].x),
                    (int)(pos.y + CubeFaces.WallDirection[face].y),
                    (int)(pos.z + CubeFaces.WallDirection[face].z)] == 0)
            {
                for (var i = 0; i < 6; i++)
                {
                    var index = CubeFaces.FaceIndices[face, i];
                    vertices.Add(CubeFaces.Vertices[index] + pos);
                    uvs.Add(voxelTypes.AtlasCubeFaceUvs[voxelTypes.GetAtlasPosition(type, face), i]);
                    //uvs.Add(CubeFaces.UVs[i]);
                }      
            }
        }
        
    }
    
}
