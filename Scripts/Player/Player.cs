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
    [Header("Inputs")]
    [HideInInspector] public InputMaster controls;
    [HideInInspector] public Vector2 movement = Vector2.zero;
    [HideInInspector] public Vector2 camvect = Vector2.zero;
    [HideInInspector] public bool jump = false;
    [HideInInspector] public bool dodge = false;
    [HideInInspector] public bool attack = false;
    [HideInInspector] public bool block = false;
    [HideInInspector] public bool menu = false;
    [HideInInspector] public bool sprint = false;
    [HideInInspector] public bool equip = false;
    [HideInInspector] public bool crouch = false;
    [HideInInspector] public bool interact = false;
    [HideInInspector] public bool inventory = false;
    [HideInInspector] public bool firstPerson = false;

    [Header("Components")]
    public Hero hero;
    public BuildSystem buildSystem;
    [HideInInspector] public Camera cam;
    [HideInInspector] public Camera map;

    public override void NetworkStart() {
      base.NetworkStart();
      hero.initRagdoll();
    }

    void Start() {
      if (!IsLocalPlayer) { return; }
      hero.spawnPoint = GameObject.Find("PlayerSpawner").GetComponent<PlayerSpawner>().spawnPoints[0].position;
      cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
      cam.transform.SetParent(hero.camTarget.transform);
      cam.transform.localPosition = new Vector3(0.2f, 0f, -3f);
      map = GameObject.Find("MapCamera").GetComponent<Camera>();
      map.gameObject.SetActive(false);
      hero.GetAvatar();
      hero.hud.initHUD();
      hero.Respawn();
    }

    void FixedUpdate() {
      if (!IsLocalPlayer) { return; }
      hero.hud.SetBars();
      if (hero.state.Value == (int)pS.Dead) { return; }
      hero.IsGrounded();
      hero.Move(movement.x, movement.y);
      hero.Regenerate();
    }

    void Update() {
      if (!IsLocalPlayer) { return; }
      if (hero.state.Value == (int)pS.Dead) { return; }
      if (!hero.inventoryOpen) {
        hero.Crouch(crouch);
        hero.Sprint(sprint);
        hero.Block(block);
        if (attack) { hero.Attack(); }
      }
      if (buildSystem.building) { buildSystem.DoBuildRay(); }
    }

    void LateUpdate() {
      if (!IsLocalPlayer) { return; }
      if (hero.state.Value == (int)pS.Dead) { return; }
      hero.Look(camvect.x, camvect.y);
    }

    #region Controls
    public void OnEnable() {
      if (controls == null) { controls = new InputMaster(); }
      controls.Player.Enable();
    }

    public void OnDisable() {
      controls.Player.Disable();
    }

    #region Vectors
    void OnMovement() {
      if (!IsLocalPlayer) { return; }
      movement = controls.Player.Movement.ReadValue<Vector2>();
    }

    void OnCamera() {
      if (!IsLocalPlayer) { return; }
      camvect = controls.Player.Camera.ReadValue<Vector2>();
    }
    #endregion

    #region Pressed
    void OnBlock() {
      if (!IsLocalPlayer) { return; }
      block = !block;
    }

    void OnAttack() {
      if (!IsLocalPlayer) { return; }
      attack = !attack;
    }

    void OnSprint() {
      if (!IsLocalPlayer) { return; }
      sprint = !sprint;
    }

    void OnCrouch() {
      if (!IsLocalPlayer) { return; }
      crouch = !crouch;
    }
    #endregion

    #region Buttons
    void OnJump() {
      if (!IsLocalPlayer || hero.state.Value == (int)pS.Dead) { return; }
      hero.Jump();
    }

    void OnDodge() {
      if (!IsLocalPlayer || hero.state.Value == (int)pS.Dead || hero.inventoryOpen) { return; }
      hero.Dodge(movement.x, movement.y);
    }

    void OnEquip() {
      if (!IsLocalPlayer || hero.state.Value == (int)pS.Dead || hero.inventoryOpen) { return; }
      if (buildSystem.building) { buildSystem.preview.transform.Rotate(0, 90f, 0); }
      else { hero.Equip(); }
    }

    void OnInteract() {
      if (!IsLocalPlayer || hero.state.Value == (int)pS.Dead) { return; }
      if (buildSystem.building) { buildSystem.BuildObject(); }
      else { hero.Interact(); }
    }

    void OnMenu() {
      if (!IsLocalPlayer) { return; }
      if (buildSystem.building) { buildSystem.CancelBuild(); }
      else {
        if (menu) {
          menu = false;
          map.gameObject.SetActive(false);
          cam.gameObject.SetActive(true);
        }
        else {
          menu = true;
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
      if (firstPerson) {
        firstPerson = false;
        cam.transform.localPosition = new Vector3(0.2f, 0, -3f);
      }
      else {
        firstPerson = true;
        cam.transform.localPosition = Vector3.zero;
      }
    }
    #endregion
    #endregion
  }
}