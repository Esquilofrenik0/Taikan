using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Postcarbon {
  public class BuildPreview: MonoBehaviour {
    public dBuildable buildable;
    private MeshRenderer rend;
    private BuildSystem buildSystem;
    [HideInInspector] public bool isSnapped = false;

    void Start() {
      buildSystem = GameObject.FindObjectOfType<BuildSystem>();
      rend = GetComponent<MeshRenderer>();
      ChangeColor();
    }

    public void Place() {
      Instantiate(buildable.prefab, transform.position, transform.rotation);
      Destroy(gameObject);
    }

    public void ChangeColor() {
      if (buildable.isFoundation) {
        rend.material = buildable.canBuild;
        isSnapped = true;
      }
      else {
        if (isSnapped) { rend.material = buildable.canBuild; }
        else { rend.material = buildable.cantBuild; }
      }
    }

    private void OnTriggerEnter(Collider other) {
      if(buildSystem.pauseBuilding){return;}
      for (int i = 0; i < buildable.tags.Count; i++) {
        if (other.tag == buildable.tags[i]) {
          buildSystem.PauseBuild(true);
          transform.position = other.transform.position;
          isSnapped = true;
          ChangeColor();
        }
      }
    }

    private void OnTriggerExit(Collider other) {
      for (int i = 0; i < buildable.tags.Count; i++) {
        if (other.tag == buildable.tags[i]) {
          isSnapped = false;
          ChangeColor();
        }
      }
    }
  }
}