using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WorldGenerator))]
public class WorldManager : MonoBehaviour
{
    public Vector3Int CenterChunk = new Vector3Int(0, 0, 0);
    public int viewDistance = 5;
    public int chunkSize = 32;

    public VoxelTypes voxelTypes;
    private WorldGenerator _generator;
    
    public class Vector3IntComparer : IEqualityComparer<Vector3Int>{
     
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


    public void AddChunksAround(Vector3Int playerChunk, int generateDistance)
    {
        
        StartCoroutine(AddChunksAroundCoroutine(playerChunk, generateDistance));
    }
    
    public IEnumerator AddChunksAroundCoroutine(Vector3Int playerChunk, int generateDistance)
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

        foreach (var chunkPosition in queue)
        {
            AddChunk(chunkPosition);
            yield return null;
        }
    }
    
    
    
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

    public Vector3 GetVoxelPoint(Vector3 worldPoint)
    {
        return transform.InverseTransformPoint(worldPoint);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="voxelPoint"></param>
    /// <returns></returns>
    public Vector3 GetWorldPoint(Vector3 voxelPoint)
    {
        return transform.TransformPoint(voxelPoint);
    }
    
    /// <summary>
    /// Check if a voxel point is solid
    /// </summary>
    /// <param name="voxelPoint">Point to check</param>
    /// <returns>Is the point solid</returns>
    public bool IsSolidPoint(Vector3Int voxelPoint)
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

        var isSolid = _chunks[chunkToCheck].Voxels[coordsInsideChunk.x, coordsInsideChunk.y, coordsInsideChunk.z] > 0;
        return isSolid;
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


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
