using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(WorldGenerator))]
public class WorldManager : MonoBehaviour
{
    private static readonly Vector3Int CenterChunk = new Vector3Int(0, 0, 0);
    public int viewDistance = 5;
    public int deleteDistance = 8;
    public int chunkSize = 32;

    public VoxelTypes voxelTypes;
    private WorldGenerator _generator;

    private class Vector3IntComparer : IEqualityComparer<Vector3Int>{
        public bool Equals(Vector3Int vec1, Vector3Int vec2) {
            return vec1.x == vec2.x && vec1.y == vec2.y && vec1.z == vec2.z;
        }
     
        public int GetHashCode (Vector3Int vec){
            return vec.x ^ vec.y << 2 ^ vec.z >> 2;
        }
    }
    private Dictionary<Vector3Int, VoxelChunk> _chunks = new Dictionary<Vector3Int, VoxelChunk>(new Vector3IntComparer());

    
    private void Awake()
    {
        _generator = GetComponent<WorldGenerator>();
        for (var x = CenterChunk.x - viewDistance; x < CenterChunk.x + viewDistance; ++x)
            for (var z = CenterChunk.z - viewDistance; z < CenterChunk.z + viewDistance; ++z)
                AddChunk(new Vector3Int(x, 0, z));
    }


    /// <summary>
    /// Add chunks around a given chunk, using coroutines
    /// </summary>
    /// <param name="playerChunk">Chunk around which to generate chunks</param>
    public void AddChunksAround(Vector3Int playerChunk)
    {
        StartCoroutine(AddChunksAroundCoroutine(playerChunk, viewDistance));
    }
    
    /// <summary>
    /// Coroutine for generating the chunks around a given chunk
    /// </summary>
    /// <param name="playerChunk">Chunk around which to generate chunks</param>
    /// <param name="generateDistance">Distance in which chunks should be generated</param>
    /// <returns>Coroutine</returns>
    private IEnumerator AddChunksAroundCoroutine(Vector3Int playerChunk, int generateDistance)
    {
        var queue = new List<Vector3Int>();
        for (var x = playerChunk.x - generateDistance; x < playerChunk.x + generateDistance; ++x)
        {
            for (var z = playerChunk.z - generateDistance; z < playerChunk.z + generateDistance; ++z)
            {
                var chunkPosition = new Vector3Int(x, 0, z);
                if (!_chunks.ContainsKey(chunkPosition)) queue.Add(chunkPosition);
            }
        }

        
        for (var x = playerChunk.x - 1; x <= playerChunk.x + 1; ++x)
        {
            for (var z = playerChunk.z - 1; z <= playerChunk.z + 1; ++z)
            {
                foreach (var aboveOrBelow in new []{-1, 1})
                {
                    var chunkPosition = new Vector3Int(x, playerChunk.y + aboveOrBelow, z);
                    if (!_chunks.ContainsKey(chunkPosition)) queue.Add(chunkPosition);
                }
            }
        }
        
        foreach (var chunkPosition in queue)
        {
            AddChunk(chunkPosition);
            yield return null;
        }
    }

    /// <summary>
    /// Remove chunks outside delete distance, using coroutines
    /// </summary>
    /// <param name="playerChunk">Chunk around which to start deleting chunks</param>
    public void RemoveChunksAround(Vector3Int playerChunk)
    {
        StartCoroutine(RemoveChunksAroundCoroutine(playerChunk, deleteDistance));
    }

    /// <summary>
    /// Coroutine for deleting the chunks around a given chunk
    /// </summary>
    /// <param name="playerChunk">Chunk around which to center the deletion chunks</param>
    /// <param name="generateDistance">Distance outside which chunks should be deleted</param>
    /// <returns>Coroutine</returns>
    private IEnumerator RemoveChunksAroundCoroutine(Vector3Int playerChunk, int distance)
    {
        var keysToDelete = _chunks.Keys.ToArray();
        foreach (var key in keysToDelete)
        {
            if (Vector3Int.Distance(playerChunk, key) > deleteDistance)
            {
                var chunk = _chunks[key];
                _chunks.Remove(key);
                Destroy(chunk.gameObject);
                yield return null;
            }            
        }
    }
    
    
    /// <summary>
    /// Add a chunk to the world
    /// </summary>
    /// <param name="worldPosition">Chunk space coordinates</param>
    private void AddChunk(Vector3Int worldPosition)
    {
        if (_chunks.ContainsKey(worldPosition)) return;
        
        var chunkObject = new GameObject(String.Format("Chunk ({0},{1},{2})", worldPosition.x, worldPosition.y, worldPosition.z));
        chunkObject.transform.parent = transform;
        chunkObject.transform.localPosition = worldPosition * chunkSize;
        var chunk = chunkObject.AddComponent<VoxelChunk>();
        chunk.worldPosition = worldPosition;
        chunk.voxelTypes = voxelTypes;
        chunk.generator = _generator;

        _chunks.Add(worldPosition, chunk);
    }

    /// <summary>
    /// Get voxel space coordinates of a world space point
    /// </summary>
    /// <param name="worldPoint">World space coordinates</param>
    /// <returns>Voxel space coordinates</returns>
    public Vector3 GetVoxelPoint(Vector3 worldPoint)
    {
        return transform.InverseTransformPoint(worldPoint);
    }

    /// <summary>
    /// get world space coordinates of a voxel point
    /// </summary>
    /// <param name="voxelPoint">Voxel space coordinates</param>
    /// <returns>World space coordinates</returns>
    public Vector3 GetWorldPoint(Vector3 voxelPoint)
    {
        return transform.TransformPoint(voxelPoint);
    }
    
    /// <summary>
    /// Returns a type of voxel at given point
    /// </summary>
    /// <param name="voxelPoint">voxel world space coordinates of the point</param>
    /// <returns>Type of voxel at given point</returns>
    public sbyte GetVoxelType(Vector3Int voxelPoint)
    {
        var chunkToCheck = voxelPoint/ chunkSize;
        var coordsInsideChunk = new Vector3Int(voxelPoint.x % chunkSize, voxelPoint.y % chunkSize, voxelPoint.z % chunkSize);
        for (var i = 0; i < 3; i++)
        {
            if (coordsInsideChunk[i] < 0)
            {
                chunkToCheck[i] -= 1;
                coordsInsideChunk[i] += chunkSize;
            }
        }

        if (!_chunks.ContainsKey(chunkToCheck)) return 0;
        
        var chunk = _chunks[chunkToCheck];
        return chunk.Voxels[coordsInsideChunk.x, coordsInsideChunk.y, coordsInsideChunk.z];
    }
    
    /// <summary>
    /// Check if a voxel point is solid
    /// </summary>
    /// <param name="voxelPoint">Point to check</param>
    /// <returns>Is the point solid</returns>
    public bool IsSolidPoint(Vector3Int voxelPoint)
    {
        return GetVoxelType(voxelPoint) > 0;
    }

    /// <summary>
    /// Checks if a voxel point is solid
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns>Is the point solid</returns>
    public bool IsSolidPoint(int x, int y, int z)
    {
        return IsSolidPoint(new Vector3Int(x, y, z));
    }
    
    /// <summary>
    /// In which chunk a given voxel point is located
    /// </summary>
    /// <param name="voxelPoint">Voxel point to locate</param>
    /// <returns>Chunk in which the voxel point lies</returns>
    public Vector3Int GetVoxelPointChunk(Vector3Int voxelPoint)
    {
        var chunkToCheck = voxelPoint/ chunkSize;
        var coordsInsideChunk = new Vector3Int(voxelPoint.x % chunkSize, voxelPoint.y % chunkSize, voxelPoint.z % chunkSize);
        for (var i = 0; i < 3; i++)
        {
            if (coordsInsideChunk[i] < 0)
            {
                chunkToCheck[i] -= 1;
                coordsInsideChunk[i] += chunkSize;
            }
        }
        return chunkToCheck;
    }
    
    /// <summary>
    /// Sets a given voxel to a given type and regenerates the chunk mesh.
    /// </summary>
    /// <param name="voxelPoint"></param>
    /// <param name="type"></param>
    public void SetVoxel(Vector3Int voxelPoint, sbyte type)
    {
        var chunkToCheck = voxelPoint/ chunkSize;
        var coordsInsideChunk = new Vector3Int(voxelPoint.x % chunkSize, voxelPoint.y % chunkSize, voxelPoint.z % chunkSize);
        for (var i = 0; i < 3; i++)
        {
            if (coordsInsideChunk[i] < 0)
            {
                chunkToCheck[i] -= 1;
                coordsInsideChunk[i] += chunkSize;
            }
        }
        
        var chunk = _chunks[chunkToCheck];
        chunk.modified = true;
        chunk.Voxels[coordsInsideChunk.x, coordsInsideChunk.y, coordsInsideChunk.z] = type;
        chunk.RegenerateMesh();
    }
}
