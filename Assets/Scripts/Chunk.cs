using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;

    private int vertexIndex = 0;
    // all vertices in the chunk
    private List<Vector3> vertices = new();

    // all triangles in the chunk
    private List<int> triangles = new();

    // all uvs in the chunk
    private List<Vector2> uvs = new();


    // a 3d array of booleans that represents the voxel map (solid/transparent)
    bool[,,] voxelMap = new bool[VoxelData.chunkWidth, VoxelData.chunkHeight, VoxelData.chunkWidth];

    private void Start()
    {
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
                    voxelMap[x, y, z] = true;
                }
            }
        }
    }
    private bool CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (x < 0 || x > VoxelData.chunkWidth -1 || y < 0 || y > VoxelData.chunkHeight - 1 || z < 0 || z > VoxelData.chunkWidth - 1)
        {
            return false;
        }

        return voxelMap[x, y, z];
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
                vertices.Add(VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]] + pos);
                vertices.Add(VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]] + pos);
                vertices.Add(VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]] + pos);
                vertices.Add(VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]] + pos);

                uvs.Add(VoxelData.voxelUvs[0]);
                uvs.Add(VoxelData.voxelUvs[1]);
                uvs.Add(VoxelData.voxelUvs[2]);
                uvs.Add(VoxelData.voxelUvs[3]);

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
    /*
     // loop through all vertices in the face
                for (int i = 0; i < 6; i++)
                {
                    // get the index of the vertex in voxelVerts
                    int triangleIndex = VoxelData.voxelTris[p, i];

                    // add the vertex to the list of vertices
                    vertices.Add(VoxelData.voxelVerts[triangleIndex] + pos);

                    // add the index of the vertex to the list of triangles to keep track of the order
                    triangles.Add(vertexIndex);

                    // add the uv to the list of uvs
                    uvs.Add(VoxelData.voxelUvs[i]);

                    vertexIndex++;
                }
     
     */
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
}
