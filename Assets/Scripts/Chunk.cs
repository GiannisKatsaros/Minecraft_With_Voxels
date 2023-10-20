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
    private List<int> transparentTriangles = new();
    private Material[] materials = new Material[2];

    // all uvs in the chunk
    private List<Vector2> uvs = new();


    // a 3d array of booleans that represents the voxel map (solid/transparent)
    public  byte[,,] voxelMap = new byte[VoxelData.chunkWidth, VoxelData.chunkHeight, VoxelData.chunkWidth];

    private World world;

    private bool _isActive;

    public bool isVoxelMapPopulated = false;

    public Chunk (ChunkCoord _coord, World _world, bool generateOnLoad)
    {
        this.coord = _coord;
        this.world = _world;
        this._isActive = true;

        if (generateOnLoad)
            Init();
    }

    public void Init()
    {
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        materials[0] = world.material;
        materials[1] = world.transparentMaterial;
        meshRenderer.materials = materials;

        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.chunkWidth, 0.0f, coord.z * VoxelData.chunkWidth);
        chunkObject.name = "Chunk " + coord.x + ", " + coord.z;
        
        PopulateVoxelMap();
        UpdateChunk();
    }


    // create the mesh data for the chunk
    private void UpdateChunk()
    {
        ClearMeshData();
        // loop through all voxels in the chunk
        for (int y = 0; y < VoxelData.chunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.chunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.chunkWidth; z++)
                {
                    if (world.blockTypes[voxelMap[x, y, z]].isSolid)
                        UpdateMeshData(new Vector3(x, y, z));
                }
            }
        }

        CreateMesh();
    }

    private void ClearMeshData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
    }

    public bool IsActive
    {
        get {  return _isActive;}
        set
        { 
            _isActive = value;
            if(chunkObject != null)
                chunkObject.SetActive(value);
        }
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
        isVoxelMapPopulated = true;
    }

    private bool IsVoxelInChunk(int x, int y, int z)
    {
        return (!(x < 0 || x > VoxelData.chunkWidth - 1 || y < 0 || y > VoxelData.chunkHeight - 1 || z < 0 || z > VoxelData.chunkWidth - 1));
    }

    public void EditVoxel(Vector3 pos, byte newId)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        voxelMap[xCheck, yCheck, zCheck] = newId;

        UpdateSurroundingVoxels(xCheck, yCheck, zCheck);
        UpdateChunk();
    }

    private void UpdateSurroundingVoxels(int x, int y, int z)
    {
        Vector3 thisVoxel = new Vector3(x, y, z);

        for (int p = 0; p < 6; p++)
        {
            Vector3 currentVoxel = thisVoxel + VoxelData.faceChecks[p];

            if (!IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
                world.GetChunkFromVector3(thisVoxel + Position).UpdateChunk();
        }
    }

    private bool CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        // if the voxel is not in the chunk, check the world for the voxel
        if (!IsVoxelInChunk(x,y,z))
            return world.CheckIfVoxelTransparent(pos + Position);

        // if the voxel is in the chunk, check the voxel map for the voxel
        return world.blockTypes[voxelMap[x, y, z]].isTransparent;
    }

    public byte GetVoxelFromGlobalVector3(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        return voxelMap[xCheck, yCheck, zCheck];
    }

    // add vertexes , triangles, and uvs to the chunk
    private void UpdateMeshData(Vector3 pos)
    {
        byte blockId = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];
        bool isTransparent = world.blockTypes[blockId].isTransparent;

        // loop through all faces in the voxel cube
        for (int p = 0; p < 6; p++)
        {
            // check if parent is transparent
            if (CheckVoxel(VoxelData.faceChecks[p] + pos))
            {
                vertices.Add(VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]] + pos);
                vertices.Add(VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]] + pos);
                vertices.Add(VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]] + pos);
                vertices.Add(VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]] + pos);

                AddTexture(world.blockTypes[blockId].GetTextureId(p));

                if (!isTransparent)
                {
                    triangles.Add(vertexIndex);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 3);
                }
                else
                {
                    transparentTriangles.Add(vertexIndex);
                    transparentTriangles.Add(vertexIndex + 1);
                    transparentTriangles.Add(vertexIndex + 2);
                    transparentTriangles.Add(vertexIndex + 2);
                    transparentTriangles.Add(vertexIndex + 1);
                    transparentTriangles.Add(vertexIndex + 3);
                }


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
            subMeshCount = 2,
            uv = uvs.ToArray()
        };
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.SetTriangles(transparentTriangles.ToArray(), 1);

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

    public ChunkCoord()
    {
        this.x = 0;
        this.z = 0;
    }

    public ChunkCoord(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x) / VoxelData.chunkWidth;
        int z = Mathf.FloorToInt(pos.z) / VoxelData.chunkWidth;
        this.x = x;
        this.z = z;
    }

    public bool Equals(ChunkCoord other)
    {
        if (other == null)
            return false;
        return (other.x == x && other.z == z);
            
    }
}