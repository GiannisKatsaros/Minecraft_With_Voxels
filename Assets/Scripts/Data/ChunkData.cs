using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChunkData
{
    private int x;
    private int y;
    public Vector2Int position
    {
        get
        {
            return new Vector2Int(x, y);
        }
        set
        {
            this.x = value.x;
            this.y = value.y;
        }
    }

    public ChunkData(Vector2Int pos)
    {
        position = pos;
    }

    public ChunkData(int _x, int _y)
    {
        this.x = _x;
        this.y = _y;
    }
    [System.NonSerialized] public Chunk chunk;

    [HideInInspector]
    public VoxelState[,,] map = new VoxelState[VoxelData.chunkWidth, VoxelData.chunkHeight, VoxelData.chunkWidth];

    // populate the voxel map with all solid voxels
    public void Populate()
    {
        // loop through all voxels in the chunk
        for (int y = 0; y < VoxelData.chunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.chunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.chunkWidth; z++)
                {
                    Vector3 voxelGlobalPos = new(x + position.x, y, z + position.y);
                    map[x, y, z] = new VoxelState(World.Instance.GetVoxel(voxelGlobalPos), this, new Vector3Int(x, y, z));
                    for (int p = 0; p < 6; p++)
                    {
                        Vector3Int neighbourV3 = new Vector3Int(x, y, z) + VoxelData.faceChecks[p];
                        if (IsVoxelInChunk(neighbourV3))
                            map[x, y, z].neighbours[p] = VoxelFromV3Int(neighbourV3);
                        else
                            map[x, y, z].neighbours[p] = World.Instance.worldData.GetVoxel(voxelGlobalPos + VoxelData.faceChecks[p]);
                    }
                }
            }
        }
        Lighting.RecalculateNaturalLight(this);
        World.Instance.worldData.AddToModifiedChunkList(this);
    }

    public void ModifyVoxel(Vector3Int pos, byte _id)
    {
        if (map[pos.x, pos.y, pos.z].id == _id)
            return;

        VoxelState voxel = map[pos.x, pos.y, pos.z];
        BlockType newType = World.Instance.blockTypes[_id];

        byte oldOpacity = voxel.properties.opacity;
        voxel.id = _id;

        if (voxel.properties.opacity != oldOpacity && (pos.y == VoxelData.chunkHeight - 1 || map[pos.x, pos.y + 1, pos.z].light == 15))
        {
            Lighting.CastNaturalLight(this, pos.x, pos.z, pos.y + 1);
        }

        World.Instance.worldData.AddToModifiedChunkList(this);

        if (chunk != null)
            World.Instance.AddChunkToUpdate(chunk);
    }

    public bool IsVoxelInChunk(int x, int y, int z)
    {
        return !(x < 0 || x > VoxelData.chunkWidth - 1 || y < 0 || y > VoxelData.chunkHeight - 1 || z < 0 || z > VoxelData.chunkWidth - 1);
    }

    public bool IsVoxelInChunk(Vector3Int pos)
    {
        return IsVoxelInChunk(pos.x, pos.y, pos.z);
    }

    public VoxelState VoxelFromV3Int(Vector3Int pos)
    {
        return map[pos.x, pos.y, pos.z];
    }
}
