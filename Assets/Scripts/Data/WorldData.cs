using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldData
{
    public string worldName = "Prototype";
    public int seed;
    [System.NonSerialized]
    public Dictionary<Vector2Int, ChunkData> chunks = new();
    [System.NonSerialized]
    public List<ChunkData> modifiedChunks = new();

    public void AddToModifiedChunkList(ChunkData chunk)
    {
        if (!modifiedChunks.Contains(chunk))
            modifiedChunks.Add(chunk);
    }

    public WorldData(string _worldName, int _seed)
    {
        worldName = _worldName;
        seed = _seed;
    }

    public WorldData(WorldData wD)
    {
        worldName = wD.worldName;
        seed = wD.seed;
    }

    public ChunkData RequestChunk(Vector2Int coord, bool create)
    {
        ChunkData c;

        lock (World.Instance.chunkListThreadLock)
        {
            if (chunks.ContainsKey(coord))
                c = chunks[coord];
            else if (!create)
                return null;
            else
            {
                LoadChunk(coord);
                c = chunks[coord];
            }
        }

        return c;
    }

    public void LoadChunk(Vector2Int coord)
    {
        if (chunks.ContainsKey(coord))
            return;

        ChunkData chunk = SaveSystem.LoadChunk(worldName, coord);

        if (chunk != null)
        {
            chunks.Add(coord, chunk);
            return;
        }

        chunks.Add(coord, new ChunkData(coord));
        chunks[coord].Populate();

    }

    private bool IsVoxelInWorld(Vector3 pos)
    {
        return (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels && pos.y >= 0 && pos.y < VoxelData.chunkHeight && pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels);
    }

    public void SetVoxel(Vector3 pos, byte value)
    {
        if (!IsVoxelInWorld(pos))
            return;

        int x = Mathf.FloorToInt(pos.x / VoxelData.chunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.chunkWidth);

        x *= VoxelData.chunkWidth;
        z *= VoxelData.chunkWidth;

        ChunkData chunk = RequestChunk(new Vector2Int(x, z), true);
        Vector3Int voxel = new((int)(pos.x - x), (int)pos.y, (int)(pos.z - z));

        chunk.ModifyVoxel(voxel, value);
    }

    public VoxelState GetVoxel(Vector3 pos)
    {
        if (!IsVoxelInWorld(pos))
            return null;

        int x = Mathf.FloorToInt(pos.x / VoxelData.chunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.chunkWidth);

        x *= VoxelData.chunkWidth;
        z *= VoxelData.chunkWidth;

        ChunkData chunk = RequestChunk(new Vector2Int(x, z), false);

        if (chunk == null)
            return null;

        Vector3Int voxel = new((int)(pos.x - x), (int)pos.y, (int)(pos.z - z));

        return chunk.map[voxel.x, voxel.y, voxel.z];
    }
}
