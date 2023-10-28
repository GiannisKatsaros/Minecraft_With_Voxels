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
    private List<Color> colors = new();
    private List<Vector3> normals = new();
    public Vector3 position;
    private bool _isActive;
    private ChunkData chunkData;

    public Chunk(ChunkCoord _coord)
    {
        this.coord = _coord;
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        materials[0] = World.Instance.material;
        materials[1] = World.Instance.transparentMaterial;
        meshRenderer.materials = materials;

        chunkObject.transform.SetParent(World.Instance.transform);
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.chunkWidth, 0.0f, coord.z * VoxelData.chunkWidth);
        chunkObject.name = "Chunk " + coord.x + ", " + coord.z;
        position = chunkObject.transform.position;
        chunkData = World.Instance.worldData.RequestChunk(new Vector2Int((int)position.x, (int)position.z), true);
        chunkData.chunk = this;
        World.Instance.AddChunkToUpdate(this);

        if (World.Instance.settings.enableAnimatedChunks)
            chunkObject.AddComponent<ChunkLoadAnimation>();
    }

    // create the mesh data for the chunk
    public void UpdateChunk()
    {
        ClearMeshData();
        // loop through all voxels in the chunk
        for (int y = 0; y < VoxelData.chunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.chunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.chunkWidth; z++)
                {
                    if (World.Instance.blockTypes[chunkData.map[x, y, z].id].isSolid)
                        UpdateMeshData(new Vector3(x, y, z));
                }
            }
        }
        World.Instance.chunksToDraw.Enqueue(this);
    }

    private void ClearMeshData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        transparentTriangles.Clear();
        uvs.Clear();
        colors.Clear();
        normals.Clear();
    }

    public bool IsActive
    {
        get { return _isActive; }
        set
        {
            _isActive = value;
            if (chunkObject != null)
                chunkObject.SetActive(value);
        }
    }

    public void EditVoxel(Vector3 pos, byte newId)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        chunkData.ModifyVoxel(new Vector3Int(xCheck, yCheck, zCheck), newId);

        UpdateSurroundingVoxels(xCheck, yCheck, zCheck);


    }

    private void UpdateSurroundingVoxels(int x, int y, int z)
    {
        Vector3 thisVoxel = new(x, y, z);

        for (int p = 0; p < 6; p++)
        {
            Vector3 currentVoxel = thisVoxel + VoxelData.faceChecks[p];

            if (!chunkData.IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
                World.Instance.AddChunkToUpdate(World.Instance.GetChunkFromVector3(currentVoxel + position), true);
        }
    }

    public VoxelState GetVoxelFromGlobalVector3(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(position.x);
        zCheck -= Mathf.FloorToInt(position.z);

        return chunkData.map[xCheck, yCheck, zCheck];
    }

    // add vertexes , triangles, and uvs to the chunk
    private void UpdateMeshData(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        VoxelState voxel = chunkData.map[x, y, z];


        // loop through all faces in the voxel cube
        for (int p = 0; p < 6; p++)
        {
            VoxelState neighbor = chunkData.map[x, y, z].neighbours[p];
            // check if parent is transparent
            if (neighbor != null && neighbor.properties.renderNeighborFaces)
            {
                vertices.Add(VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]] + pos);
                vertices.Add(VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]] + pos);
                vertices.Add(VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]] + pos);
                vertices.Add(VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]] + pos);

                for (int i = 0; i < 4; i++)
                    normals.Add(VoxelData.faceChecks[p]);

                AddTexture(voxel.properties.GetTextureId(p));

                float lightLevel = neighbor.lightAsFloat;

                colors.Add(new Color(0, 0, 0, lightLevel));
                colors.Add(new Color(0, 0, 0, lightLevel));
                colors.Add(new Color(0, 0, 0, lightLevel));
                colors.Add(new Color(0, 0, 0, lightLevel));

                if (!neighbor.properties.renderNeighborFaces)
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
    public void CreateMesh()
    {
        // create a new mesh
        Mesh mesh = new()
        {
            vertices = vertices.ToArray(),
            // triangles = triangles.ToArray(),
            subMeshCount = 2,
            uv = uvs.ToArray(),
            colors = colors.ToArray(),
            normals = normals.ToArray()
        };
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.SetTriangles(transparentTriangles.ToArray(), 1);

        // recalculate normals
        // mesh.RecalculateNormals();


        // assign the mesh to the mesh filter
        meshFilter.mesh = mesh;
    }

    // add a texture to the chunk
    private void AddTexture(int textureId)
    {
        // get the x and y coordinates of the texture
        float y = textureId / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureId - (y * VoxelData.TextureAtlasSizeInBlocks);

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
