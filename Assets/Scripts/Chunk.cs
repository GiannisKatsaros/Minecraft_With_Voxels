using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;

    private void Start()
    {
        int vertexIndex = 0;
        // all vertices in the chunk
        List<Vector3> vertices = new();

        // all triangles in the chunk
        List<int> triangles = new();

        // all uvs in the chunk
        List<Vector2> uvs = new();

        // loop through all faces in the voxel cube
        for (int p = 0; p < 6; p++)
        {
            // loop through all vertices in the face
            for (int i = 0; i < 6; i++)
            {
                // get the index of the vertex in voxelVerts
                int triangleIndex = VoxelData.voxelTris[p, i];

                // add the vertex to the list of vertices
                vertices.Add(VoxelData.voxelVerts[triangleIndex]);

                // add the index of the vertex to the list of triangles to keep track of the order
                triangles.Add(vertexIndex);

                // add the uv to the list of uvs
                uvs.Add(VoxelData.voxelUvs[i]);

                vertexIndex++;
            }
        }

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
