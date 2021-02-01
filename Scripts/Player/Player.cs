using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using MLAPI;
using MLAPI.NetworkedVar;
using MLAPI.NetworkedVar.Collections;

namespace Postcarbon {
  [System.Serializable]
  public class Player: NetworkedBehaviour {
    [Header("Components")]
    public Hero hero;
    public InputHandler input;
    [HideInInspector] public Camera cam;
    [HideInInspector] public CinemachineVirtualCamera heroCam;
    [HideInInspector] public CinemachineVirtualCamera worldCam;
    [HideInInspector] public Cinemachine3rdPersonFollow view;

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
      heroCam = GameObject.Find("HeroCam").GetComponent<CinemachineVirtualCamera>();
      heroCam.transform.SetParent(hero.camTarget.transform);
      heroCam.Follow = hero.camTarget.transform;
      view = heroCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
      worldCam = GameObject.Find("WorldCam").GetComponent<CinemachineVirtualCamera>();
      worldCam.Priority = 8;
      gameObject.layer = 9;
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
      hero.Equip();
    }

    void OnInteract() {
      if (!IsLocalPlayer || hero.state == (int)pS.Dead) { return; }
      hero.Interact();
    }

    void OnMenu() {
      if (!IsLocalPlayer) { return; }
      if (input.menu) {
        input.menu = false;
        worldCam.Priority = 8;
        HideLayer(31);
      }
      else {
        input.menu = true;
        worldCam.Priority = 10;
        ShowLayer(31);
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
        heroCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>().ShoulderOffset.x = 0.2f;
        heroCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraDistance = 3;
      }
      else {
        input.firstPerson = true;
        heroCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>().ShoulderOffset.x = 0;
        heroCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraDistance = 0;
      }
    }
    #endregion

    #region Camera
    private void ShowLayer(int layer) {
      cam.cullingMask |= 1 << layer;
    }

    private void HideLayer(int layer) {
      cam.cullingMask &= ~(1 << layer);
    }
    #endregion
  }
}