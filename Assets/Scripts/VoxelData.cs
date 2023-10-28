using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{
    public static readonly int chunkWidth = 16;
    public static readonly int chunkHeight = 128;
    public static readonly int WorldSizeInChunks = 100;

    // Light data
    public static float minLightLevel = 0.15f;
    public static float maxLightLevel = 0.8f;

    public static float unitOfLight
    {
        get { return 1f / 16f; }
    }

    public static int seed;

    public static int WorldCenter
    {
        get { return (WorldSizeInChunks * chunkWidth) / 2; }
    }

    public static int WorldSizeInVoxels
    {
        get { return (WorldSizeInChunks * chunkWidth); }
    }

    public static readonly int TextureAtlasSizeInBlocks = 16;
    public static float NormalizedBlockTextureSize
    {
        get { return 1.0f / (float)TextureAtlasSizeInBlocks; }
    }

    // lookup list of all vertexes in a voxel cube
    // each Vector3 is a vertex 
    public static readonly Vector3[] voxelVerts = new Vector3[8]
    {
        new(0.0f,0.0f,0.0f),
        new(1.0f,0.0f,0.0f),
        new(1.0f,1.0f,0.0f),
        new(0.0f,1.0f,0.0f),
        new(0.0f,0.0f,1.0f),
        new(1.0f,0.0f,1.0f),
        new(1.0f,1.0f,1.0f),
        new(0.0f,1.0f,1.0f),
    };

    public static readonly Vector3Int[] faceChecks = new Vector3Int[6]
    {
        new( 0, 0,-1),   // check back face
        new( 0, 0, 1),    // check front face
        new( 0, 1, 0),    // check top face
        new( 0,-1, 0),   // check bottom face
        new(-1, 0, 0),   // check left face
        new( 1, 0, 0)     // check right face
    };

    public static readonly int[] revFaceCheckIndex = new int[6] { 1, 0, 3, 2, 5, 4 };

    // lookup list of all faces in a voxel cube
    // each sublist contains the vertexes from voxelVerts that make up a face
    public static readonly int[,] voxelTris = new int[6, 4]
    {
        {0, 3, 1, 2}, // back face
        {5, 6, 4, 7}, // front face
        {3, 7, 2, 6}, // top face
        {1, 5, 0, 4}, // bottom face
        {4, 7, 0, 3}, // left face
        {1, 2, 5, 6}  // right face
    };

    public static readonly Vector2[] voxelUvs = new Vector2[4]
    {
        new(0.0f, 0.0f),
        new(0.0f, 1.0f),
        new(1.0f, 0.0f),
        new(1.0f, 1.0f)
    };

}
