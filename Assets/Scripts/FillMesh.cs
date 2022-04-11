using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FillMesh : MonoBehaviour
{
    Mesh mesh;

    [SerializeField] Vector3[] verts;
    [SerializeField] int[] tris;
    [SerializeField] Vector2[] uvs;
    Color[] colors;

    public void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        mesh = GetComponent<MeshFilter>().mesh;

        verts = new Vector3[4];
        tris = new int[6];
        uvs = new Vector2[4];

        verts[0] = v1;
        verts[1] = v2;
        verts[2] = v3;
        verts[3] = v4;

        tris[0] = 0;
        tris[1] = 1;
        tris[2] = 2;

        tris[3] = 0;
        tris[4] = 2;
        tris[5] = 3;

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
    }

    public void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
    {
        colors = new Color[4];

        colors[0] = c1;
        colors[1] = c2;
        colors[2] = c3;
        colors[3] = c4;

        mesh.colors = colors;
    }

    public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        mesh = GetComponent<MeshFilter>().mesh;

        verts = new Vector3[3];
        tris = new int[3];
        uvs = new Vector2[3];

        verts[0] = v1;
        verts[1] = v2;
        verts[2] = v3;

        tris[0] = 0;
        tris[1] = 1;
        tris[2] = 2;

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        //mesh.uv = uvs;
    }

    public void AddTriangleColor(Color c1, Color c2, Color c3)
    {
        colors = new Color[3];

        colors[0] = c1;
        colors[1] = c2;
        colors[2] = c3;

        mesh.colors = colors;
    }

}
