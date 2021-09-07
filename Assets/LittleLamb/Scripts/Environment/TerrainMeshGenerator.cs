using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TerrainMeshGenerator : MonoBehaviour
{

  Mesh mesh;
  Vector3[] vertices;
  int[] triangles;

  public int xSize = 20;
  public int zSize = 20;

  void Start()
  {
    mesh = new Mesh();
    GetComponent<MeshFilter>().mesh = mesh;

    CreateShape();
    UpdateMesh();
  }

  /// Generate noise map via perlin noise
    float[,] GenerateNoiseMap(int xSize, int zSize, float scale)
  {
    float[,] noiseMap = new float[xSize, zSize];
    for (int zIndex = 0; zIndex < zSize; zIndex++)
    {
      for (int xIndex = 0; xIndex < xSize; xIndex++)
      {
        // calculate sample indices based on the coordinates and the scale
        float sampleX = xIndex / scale;
        float sampleZ = zIndex / scale;
        // generate noise value using PerlinNoise
        float noise = Mathf.PerlinNoise(sampleX, sampleZ);
        noiseMap[zIndex, xIndex] = noise;
      }
    }
    return noiseMap;
  }

  void CreateShape()
  {
    vertices = new Vector3[(xSize + 1) * (zSize + 1)];

    for (int i = 0, z = 0; z <= zSize; z++)
    {
      for (int x = 0; x <= xSize; x++, i++)
      {
        vertices[i] = new Vector3(x, 0, z);
      }
    }
  }

  void UpdateMesh()
  {
    mesh.Clear();
    mesh.vertices = vertices;
    mesh.triangles = triangles;
    mesh.RecalculateNormals();
  }

  private void OnDrawGizmos()
  {
    if (vertices == null) return;
    for (int i = 0; i < vertices.Length; i++)
    {
      Gizmos.DrawSphere(vertices[i], .1f);
    }
  }

  void Update()
  {

  }
}
