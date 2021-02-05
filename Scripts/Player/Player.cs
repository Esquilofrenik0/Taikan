using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkedVar;
using MLAPI.NetworkedVar.Collections;

namespace Postcarbon {
  [System.Serializable]
  public class Player: NetworkedBehaviour {
    [Header("Components")]
    public Hero hero;
    public InputHandler input;
    public BuildSystem buildSystem;
    [HideInInspector] public Camera cam;
    [HideInInspector] public Camera map;

    public override void NetworkStart() {
      base.NetworkStart();
      hero.initRagdoll();
      // if(hero.recipeAvatar != null){hero.LoadAvatar(hero.recipeAvatar);}
    }

    void Start() {
      if (!IsLocalPlayer) { return; }
      hero.spawnPoint = GameObject.Find("PlayerSpawner").GetComponent<PlayerSpawner>().spawnPoints[0].position;
      cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
      cam.transform.SetParent(hero.camTarget.transform);
      cam.transform.localPosition = new Vector3(0.2f, 0f, -3f);
      map = GameObject.Find("MapCamera").GetComponent<Camera>();
      map.gameObject.SetActive(false);
      // hero.LoadAvatar(hero.GetAvatar());
      hero.hud.initHUD();
      hero.Respawn();
    }

    void FixedUpdate() {
      if (!IsLocalPlayer) { return; }
      hero.hud.SetBars();
      if (hero.state == (int)pS.Dead) { return; }
      hero.IsGrounded();
      hero.Move(input.movement.x, input.movement.y);
      hero.Regenerate();
    }

    void Update() {
      if (!IsLocalPlayer) { return; }
      if (hero.state == (int)pS.Dead) { return; }
      if (!hero.inventoryOpen) {
        hero.Crouch(input.crouch);
        hero.Sprint(input.sprint);
        hero.Block(input.block);
        if (input.attack) { hero.Attack(); }
      }
      if (buildSystem.building) { buildSystem.DoBuildRay(); }
    }

    void LateUpdate() {
      if (!IsLocalPlayer) { return; }
      if (hero.state == (int)pS.Dead) { return; }
      hero.Look(input.camvect.x, input.camvect.y);
    }

    #region Controls
    void OnJump() {
      if (!IsLocalPlayer || hero.state == (int)pS.Dead) { return; }
      hero.Jump();
    }

    void OnDodge() {
      if (!IsLocalPlayer || hero.state == (int)pS.Dead || hero.inventoryOpen) { return; }
      hero.Dodge(input.movement.x, input.movement.y);
    }

    void OnEquip() {
      if (!IsLocalPlayer || hero.state == (int)pS.Dead || hero.inventoryOpen) { return; }
      if (buildSystem.building) { buildSystem.preview.transform.Rotate(0, 90f, 0); }
      else { hero.Equip(); }
    }

    void OnInteract() {
      if (!IsLocalPlayer || hero.state == (int)pS.Dead) { return; }
      if (buildSystem.building) { buildSystem.BuildObject(); }
      else { hero.Interact(); }
    }

    void OnMenu() {
      if (!IsLocalPlayer) { return; }
      if (buildSystem.building) { buildSystem.CancelBuild(); }
      else {
        if (input.menu) {
          input.menu = false;
          map.gameObject.SetActive(false);
          cam.gameObject.SetActive(true);
        }
        else {
          input.menu = true;
          cam.gameObject.SetActive(false);
          map.gameObject.SetActive(true);
        }
      }
    }

    void OnInventory() {
      if (!IsLocalPlayer) { return; }
      hero.ToggleInventory();
    }

    void OnChangeView() {
      if (!IsLocalPlayer) { return; }
      if (input.firstPerson) {
        input.firstPerson = false;
        cam.transform.localPosition = new Vector3(0.2f, 0, -3f);
      }
      else {
        input.firstPerson = true;
        cam.transform.localPosition = Vector3.zero;
      }
    }
    #endregion
  }
}