using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public ChunkCoord coord;

    private GameObject chunkObject;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;

    private int vertexIndex = 0;
    // all vertices in the chunk
    private List<Vector3> vertices = new();

    // all triangles in the chunk
    private List<int> triangles = new();

    // all uvs in the chunk
    private List<Vector2> uvs = new();


    // a 3d array of booleans that represents the voxel map (solid/transparent)
    private byte[,,] voxelMap = new byte[VoxelData.chunkWidth, VoxelData.chunkHeight, VoxelData.chunkWidth];

    private World world;

    public Chunk (ChunkCoord _coord, World _world)
    {
        this.coord = _coord;
        this.world = _world;

        this.chunkObject = new GameObject();
        this.chunkObject.transform.SetParent(world.transform);
        this.chunkObject.transform.position = new Vector3(coord.x * VoxelData.chunkWidth, 0.0f, coord.z * VoxelData.chunkWidth);
        this.chunkObject.name = "Chunk " + coord.x + ", " + coord.z;

        this.meshFilter = chunkObject.AddComponent<MeshFilter>();

        this.meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        this.meshRenderer.material = world.material;


        PopulateVoxelMap();
        CreateMeshData();
        CreateMesh();
    }


    // create the mesh data for the chunk
    private void CreateMeshData()
    {
        // loop through all voxels in the chunk
        for (int y = 0; y < VoxelData.chunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.chunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.chunkWidth; z++)
                {
                    AddVoxelDataToChunk(new Vector3(x, y, z));
                }
            }
        }
    }

    public bool IsActive
    {
        get {  return chunkObject.activeSelf;}
        set { chunkObject.SetActive(value); }
    }

    public Vector3 Position
    {
        get { return chunkObject.transform.position; }
    }

    // populate the voxel map with all solid voxels
    private void PopulateVoxelMap()
    {
        // loop through all voxels in the chunk
        for (int y = 0; y < VoxelData.chunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.chunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.chunkWidth; z++)
                {
                    voxelMap[x, y, z] = world.GetVoxel(new Vector3(x, y, z) + Position);
                }
            }
        }
    }

    private bool IsVoxelInChunk(int x, int y, int z)
    {
        return (!(x < 0 || x > VoxelData.chunkWidth - 1 || y < 0 || y > VoxelData.chunkHeight - 1 || z < 0 || z > VoxelData.chunkWidth - 1));
    }

    private bool CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        // if the voxel is not in the chunk, check the world for the voxel
        if (!IsVoxelInChunk(x,y,z))
            return world.blockTypes[world.GetVoxel(pos + Position)].isSolid;

        // if the voxel is in the chunk, check the voxel map for the voxel
        return world.blockTypes[voxelMap[x, y, z]].isSolid;
    }

    // add vertexes , triangles, and uvs to the chunk
    private void AddVoxelDataToChunk(Vector3 pos)
    {
        // loop through all faces in the voxel cube
        for (int p = 0; p < 6; p++)
        {
            // check if parent is transparent
            if (!CheckVoxel(VoxelData.faceChecks[p] + pos))
            {
                byte blockId = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

                vertices.Add(VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]] + pos);
                vertices.Add(VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]] + pos);
                vertices.Add(VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]] + pos);
                vertices.Add(VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]] + pos);

                AddTexture(world.blockTypes[blockId].GetTextureId(p));

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);


                vertexIndex += 4;
            }
        }
    }
    
    // create a mesh from the vertices, triangles, and uvs
    private void CreateMesh()
    {
        // create a new mesh
        Mesh mesh = new()
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            uv = uvs.ToArray()
        };

        // recalculate normals
        mesh.RecalculateNormals();

        // assign the mesh to the mesh filter
        meshFilter.mesh = mesh;
    }

    // add a texture to the chunk
    private void AddTexture(int textureId)
    {
        // get the x and y coordinates of the texture
        float y = textureId / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureId - ( y * VoxelData.TextureAtlasSizeInBlocks);

        // normalize the texture
        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1.0f - y - VoxelData.NormalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
    }
}

public class ChunkCoord
{
    public int x;
    public int z;

    public ChunkCoord(int _x, int _z)
    {
        this.x = _x;
        this.z = _z;
    }

    public bool Equals(ChunkCoord other)
    {
        if (other == null)
            return false;
        return (other.x == x && other.z == z);
            
    }
}