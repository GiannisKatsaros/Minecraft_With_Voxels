using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Lighting
{
    public static void RecalculateNaturalLight(ChunkData chunkData)
    {
        for (int x = 0; x < VoxelData.chunkWidth; x++)
        {
            for (int z = 0; z < VoxelData.chunkWidth; z++)
            {
                CastNaturalLight(chunkData, x, z, VoxelData.chunkHeight - 1);
            }
        }
    }
    public static void CastNaturalLight(ChunkData chunkData, int x, int z, int startY)
    {
        if (startY > VoxelData.chunkHeight - 1)
        {
            startY = VoxelData.chunkHeight - 1;
            Debug.Log("Attempting to cast light from above chunk");
        }

        bool obstructed = false;
        for (int y = startY; y > -1; y--)
        {
            VoxelState voxel = chunkData.map[x, y, z];

            if (obstructed)
            {
                voxel.light = 0;
            }
            else if (voxel.properties.opacity > 0)
            {
                voxel.light = 0;
                obstructed = true;
            }
            else
            {
                voxel.light = 15;
            }
        }
    }
}
