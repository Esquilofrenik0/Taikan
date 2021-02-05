using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Postcarbon {
  public class BuildSystem: MonoBehaviour {
    public Player player;
    public LayerMask layer;
    public float stickTolerance = 5f;
    [HideInInspector] public BuildPreview preview;
    [HideInInspector] public bool building = false;
    [HideInInspector] public bool pauseBuilding = false;

    public void NewBuild(GameObject _go) {
      if (preview) { CancelBuild(); }
      preview = Instantiate(_go, Vector3.zero, Quaternion.identity).GetComponent<BuildPreview>();
      building = true;
    }

    public void CancelBuild() {
      Destroy(preview.gameObject);
      preview = null;
      building = false;
    }

    public void BuildObject() {
      if (preview.isSnapped) {
        preview.Place();
        preview = null;
        building = false;
      }
      else { Debug.Log("Not Snapped"); }
    }

    public void PauseBuild(bool _value) {
      pauseBuilding = _value;
    }

    public void DoBuildRay() {
      Ray ray = new Ray(player.cam.transform.position, player.cam.transform.forward);
      RaycastHit hit;
      if (Physics.Raycast(ray, out hit, 100f, layer, QueryTriggerInteraction.Collide)) {
        if (pauseBuilding) {
          if (Vector3.Distance(hit.point, preview.transform.position) > stickTolerance) {
            pauseBuilding = false;
          }
        }
        else {
          float y = hit.point.y + (preview.GetComponent<Collider>().bounds.extents.y);
          Vector3 pos = new Vector3(hit.point.x, y, hit.point.z);
          preview.transform.position = pos;
        }
      }
    }
  }
}