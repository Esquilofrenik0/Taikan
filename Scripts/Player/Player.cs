using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using MLAPI;
using MLAPI.NetworkedVar;

namespace SRPG {
  [System.Serializable]
  public class Player: NetworkedBehaviour {
    [Header("Components")]
    public Hero hero;
    public InputHandler input;
    public NetworkedVarULong netID = new NetworkedVarULong(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, 0);
    [HideInInspector] public Camera cam;
    [HideInInspector] public CinemachineVirtualCamera heroCam;
    [HideInInspector] public CinemachineVirtualCamera worldCam;
    [HideInInspector] public Cinemachine3rdPersonFollow view;

    public override void NetworkStart() {
      base.NetworkStart();
      if (IsServer) {
        NetworkedObject player = NetworkingManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject;
        netID.Value = player.NetworkId;
      }
      hero.initRagdoll();
      hero.LoadAvatar();
    }

    #region Unity
    void Start() {
      if (!IsLocalPlayer) { return; }
      hero.spawnPoint = GameObject.Find("PlayerSpawner").GetComponent<PlayerSpawner>().spawnPoints[0].position;
      cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
      cam.transform.SetParent(hero.camTarget.transform);
      heroCam = GameObject.Find("HeroCam").GetComponent<CinemachineVirtualCamera>();
      heroCam.Follow = hero.camTarget.transform;
      view = heroCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
      worldCam = GameObject.Find("WorldCam").GetComponent<CinemachineVirtualCamera>();
      worldCam.Priority = 8;
      gameObject.layer = 9;
      hero.hud.initHUD();
      hero.Respawn();
    }

    void FixedUpdate() {
      if (!IsLocalPlayer) { return; }
      hero.hud.SetBars();
      if (hero.state == (int)pS.Dead) { return; }
      hero.IsGrounded();
      hero.RefreshState();
      if (hero.aiming) { heroCam.m_Lens.FieldOfView = 30; } else { heroCam.m_Lens.FieldOfView = 45; }
      if (hero.equipment.weapon1.Value) { if (hero.equipment.weapon1.Value.GetComponent<Weapon>().fx != null) { hero.equipment.weapon1.Value.GetComponent<Weapon>().fx.SetActive(false); } }
    }

    void Update() {
      if (!IsLocalPlayer || hero.state == (int)pS.Dead) { return; }
      if (!hero.inventoryOpen) {
        hero.Sprint(input.sprint);
        hero.Crouch(input.crouch);
        hero.Block(input.block);
        if (input.attack) { hero.Attack(); }
      }
      hero.Move(input.movement.x, input.movement.y);
      hero.ApplyGravity();
      hero.UpdateImpact();
      hero.Regenerate();
    }

    void LateUpdate() {
      if (!IsLocalPlayer) { return; }
      if (hero.state == (int)pS.Dead) { return; }
      hero.Look(input.camvect.x, input.camvect.y);
    }
    #endregion

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
        heroCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>().ShoulderOffset.x = 0.25f;
        heroCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraDistance = 4;
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