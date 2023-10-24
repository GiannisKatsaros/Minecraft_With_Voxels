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
    public static float lightFalloff = 0.08f;
    public static int WorldSizeInVoxels
    {
        get { return (WorldSizeInChunks * chunkWidth); }
    }

    public static readonly int ViewDistanceInChunks = 5;

    public static readonly int TextureAtlasSizeInBlocks = 16;
    public static float NormalizedBlockTextureSize
    {
        get { return 1.0f / (float)TextureAtlasSizeInBlocks; }
    }

    // lookup list of all vertexes in a voxel cube
    // each Vector3 is a vertex 
    public static readonly Vector3[] voxelVerts = new Vector3[8]
    {
        new Vector3(0.0f,0.0f,0.0f),
        new Vector3(1.0f,0.0f,0.0f),
        new Vector3(1.0f,1.0f,0.0f),
        new Vector3(0.0f,1.0f,0.0f),
        new Vector3(0.0f,0.0f,1.0f),
        new Vector3(1.0f,0.0f,1.0f),
        new Vector3(1.0f,1.0f,1.0f),
        new Vector3(0.0f,1.0f,1.0f),
    };

    public static readonly Vector3[] faceChecks = new Vector3[6]
    {
        new Vector3( 0.0f, 0.0f,-1.0f),   // check back face
        new Vector3( 0.0f, 0.0f, 1.0f),    // check front face
        new Vector3( 0.0f, 1.0f, 0.0f),    // check top face
        new Vector3( 0.0f,-1.0f, 0.0f),   // check bottom face
        new Vector3(-1.0f, 0.0f, 0.0f),   // check left face
        new Vector3( 1.0f, 0.0f, 0.0f)     // check right face
    };

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
        new Vector2(0.0f, 0.0f),
        new Vector2(0.0f, 1.0f),
        new Vector2(1.0f, 0.0f),
        new Vector2(1.0f, 1.0f)
    };

}
