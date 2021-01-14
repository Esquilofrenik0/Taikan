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
    [HideInInspector] public CinemachineVirtualCamera vcam;
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
      hero.transform.Find("MinimapIcon").gameObject.SetActive(true);
      GameObject.Find("MinimapCamera").transform.SetParent(hero.transform);
      GameObject.FindWithTag("MainCamera").transform.SetParent(hero.camTarget.transform);
      vcam = GameObject.Find("HeroCam").GetComponent<CinemachineVirtualCamera>();
      vcam.Follow = hero.camTarget.transform;
      view = vcam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
      gameObject.layer = 9;
      hero.hud.initHUD();
      hero.Respawn();
    }

    void FixedUpdate() {
      if (!IsLocalPlayer) { return; }
      hero.hud.SetBars();
      if (hero.state == pS.Dead) { return; }
      hero.IsGrounded();
      hero.RefreshState();
      if (hero.aiming) { vcam.m_Lens.FieldOfView = 30; } else { vcam.m_Lens.FieldOfView = 45; }
      if (hero.equipment.item[0] != 0) { if (GetNetworkedObject(hero.equipment.item[0]).GetComponent<Weapon>().fx != null) { GetNetworkedObject(hero.equipment.item[0]).GetComponent<Weapon>().fx.SetActive(false); } }
    }

    void Update() {
      if (!IsLocalPlayer) { return; }
      if (input.inventory) { hero.ToggleInventory(); input.inventory = false; }
      if (hero.state == pS.Dead) { return; }
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
      if (hero.state == pS.Dead) { return; }
      hero.Look(input.camvect.x, input.camvect.y);
    }
    #endregion
  }
}