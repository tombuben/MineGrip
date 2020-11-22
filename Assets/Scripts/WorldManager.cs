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
    
    private Dictionary<Vector3Int, VoxelChunk> _chunks = new Dictionary<Vector3Int, VoxelChunk>();

    
    private void Awake()
    {
        _generator = GetComponent<WorldGenerator>();
        for (var x = CenterChunk.x - viewDistance; x < CenterChunk.x + viewDistance; ++x)
            for (var z = CenterChunk.z - viewDistance; z < CenterChunk.z + viewDistance; ++z)
                AddChunk(new Vector3Int(x, 0, z));
    }

    private void AddChunk(Vector3Int worldPosition)
    {
        var chunkObject = new GameObject(String.Format("Chunk ({0},{1},{2})", worldPosition.x, worldPosition.y, worldPosition.z));
        chunkObject.transform.parent = transform;
        chunkObject.transform.localPosition = worldPosition * chunkSize;
        var chunk = chunkObject.AddComponent<VoxelChunk>();
        chunk.worldPosition = worldPosition;
        chunk.voxelTypes = voxelTypes;
        chunk.generator = _generator;
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
