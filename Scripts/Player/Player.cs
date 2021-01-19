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
      hero.transform.Find("MinimapIcon").gameObject.SetActive(true);
      GameObject.Find("MinimapCamera").transform.SetParent(hero.transform);
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
      if (hero.equipment.item[0] != 0) { if (GetNetworkedObject(hero.equipment.item[0]).GetComponent<Weapon>().fx != null) { GetNetworkedObject(hero.equipment.item[0]).GetComponent<Weapon>().fx.SetActive(false); } }
    }

    void Update() {
      if (!IsLocalPlayer) { return; }
      if (input.inventory) { hero.ToggleInventory(); input.inventory = false; }
      if (hero.state == (int)pS.Dead) { return; }
      if (input.interact) { hero.Interact(); input.interact = false; }
      if (!hero.inventoryOpen) {
        if (input.equip) { hero.Equip(); input.equip = false; }
        hero.Sprint(input.sprint);
        hero.Crouch(input.crouch);
        hero.Block(input.block);
        if (input.attack) { hero.Attack(); }
        if (input.dodge) { hero.Dodge(input.movement.x, input.movement.y); input.dodge = false; }
      }
      if (input.jump) { hero.Jump(); input.jump = false; }
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
  }
}