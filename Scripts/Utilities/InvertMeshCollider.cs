using UnityEngine;
using System.Linq;
using System.Collections;

public class InvertMeshCollider: MonoBehaviour {
  public bool removeExistingColliders = true;

  public void Awake() {
    if (removeExistingColliders) { RemoveExistingColliders(); }
    InvertMesh();
    gameObject.AddComponent<MeshCollider>();
  }

  private void RemoveExistingColliders() {
    Collider[] colliders = GetComponents<Collider>();
    for (int i = 0; i < colliders.Length; i++){
      DestroyImmediate(colliders[i]);
    }
  }

  private void InvertMesh() {
    Mesh mesh = GetComponent<MeshFilter>().mesh;
    mesh.triangles = mesh.triangles.Reverse().ToArray();
  }
}

