using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class OutlineNormalsCalculator : MonoBehaviour {

    [SerializeField] private int storeInTexcoordChannel = 1;
    [SerializeField] private float cospatialVertexDistance = 0.01f;

    // This class holds the accumulated normal for merged, or cospatial, vertices
    private class CospatialVertex {
        public Vector3 position;
        public Vector3 accumulatedNormal;
    }

    private void Start() {
        Mesh mesh = GetComponent<MeshFilter>().mesh;


        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector3[] outlineNormals = new Vector3[vertices.Length];

        List<CospatialVertex> cospatialVerticesData = new List<CospatialVertex>();
        int[] cospacialVertexIndices = new int[vertices.Length];
        FindCospatialVertices(vertices, cospacialVertexIndices, cospatialVerticesData);
        
        int numTriangles = triangles.Length / 3;
        // For each triangle
        for(int t = 0; t < numTriangles; t++) {
            
            int vertexStart = t * 3;
            int v1Index = triangles[vertexStart];
            int v2Index = triangles[vertexStart + 1];
            int v3Index = triangles[vertexStart + 2];
            // Get this triangle's normal vector and the weight for each vertex
            ComputeNormalAndWeights(vertices[v1Index], vertices[v2Index], vertices[v3Index], out Vector3 normal, out Vector3 weights);
            // Add the weighted normal to each cospatial vertex data instance
            AddWeightedNormal(normal * weights.x, v1Index, cospacialVertexIndices, cospatialVerticesData);
            AddWeightedNormal(normal * weights.y, v2Index, cospacialVertexIndices, cospatialVerticesData);
            AddWeightedNormal(normal * weights.z, v3Index, cospacialVertexIndices, cospatialVerticesData);
        }

        // For each vertex
        for(int v = 0; v < outlineNormals.Length; v++) {
            // Find the cospacial registry index for this vertex
            int cvIndex = cospacialVertexIndices[v];
            // Get the cospatial data object
            var cospatial = cospatialVerticesData[cvIndex];
            // Normalize the accumulated normal
            // This averages it
            outlineNormals[v] = cospatial.accumulatedNormal.normalized;
        }

        // Store the outline normals in the mesh's UV channel
        mesh.SetUVs(storeInTexcoordChannel, outlineNormals);
    }

    private void FindCospatialVertices(Vector3[] vertices, int[] indices, List<CospatialVertex> registry) {
        // For each vertex
        for(int v = 0; v < vertices.Length; v++) {
            if(SearchForPreviouslyRegisteredCV(vertices[v], registry, out int index)) {
                // If this vertex is cospatial with another, then register the data index
                indices[v] = index;
            } else {
                // If this vertex is unique, create a new cospacial vertex data object
                var cospatialEntry = new CospatialVertex() {
                    position = vertices[v],
                    accumulatedNormal = Vector3.zero,
                };
                // Set the cospatial index to this new object's index in the list
                indices[v] = registry.Count;
                registry.Add(cospatialEntry);
            }
        }
    }

    private bool SearchForPreviouslyRegisteredCV(Vector3 position, List<CospatialVertex> registry, out int index) {
        // For each registry entry
        for(int i = 0; i < registry.Count; i++) {
            // If the vertex is close enough, consider it cospatial
            if(Vector3.Distance(registry[i].position, position) <= cospatialVertexDistance) {
                index = i;
                return true;
            }
        }
        index = -1;
        return false;
    }

    private void ComputeNormalAndWeights(Vector3 a, Vector3 b, Vector3 c, out Vector3 normal, out Vector3 weights) {
        normal = Vector3.Cross(b - a, c - a).normalized;
        weights = new Vector3(Vector3.Angle(b - a, c - a), Vector3.Angle(c - b, a - b), Vector3.Angle(a - c, b - c));
    }

    private void AddWeightedNormal(Vector3 weightedNormal, int vertexIndex, int[] cvIndices, List<CospatialVertex> cvRegistry) {
        int cvIndex = cvIndices[vertexIndex];
        cvRegistry[cvIndex].accumulatedNormal += weightedNormal;
    }
}