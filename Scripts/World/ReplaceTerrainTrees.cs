using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Postcarbon {
  public class ReplaceTerrainTrees: MonoBehaviour {
    public Terrain[] world;
    public GameObject[] node;

    void Start() {
      GameObject parent = GameObject.Find("World/Vegetation");
      for (int i = 0; i < world.Length; i++) {
        foreach (TreeInstance tree in world[i].terrainData.treeInstances) {
          Vector3 worldTreePos = Vector3.Scale(tree.position, world[i].terrainData.size) + world[i].transform.position;
          GameObject newTree = Instantiate(node[tree.prototypeIndex], worldTreePos, Quaternion.identity,parent.transform); 
          newTree.transform.localScale = new Vector3(tree.widthScale,tree.widthScale,tree.widthScale);
          newTree.GetComponent<Collider>().enabled = true;
        }
        List<TreeInstance> newTrees = new List<TreeInstance>(0);
        world[i].terrainData.treeInstances = newTrees.ToArray();
      }
    }
  }
}
