using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using MLAPI;
using MLAPI.NetworkedVar;
using MLAPI.Messaging;

namespace Postcarbon {
  [System.Serializable]
  public class Hero: Human {
    [Header("Components")]
    public Player player;
    public HUD hud;
    public string recipeAvatar;

    [Header("Camera")]
    public GameObject camTarget;
    public float rotationSpeed = 1;
    [HideInInspector] public float mouseX = 0;
    [HideInInspector] public float mouseY = 0;

    [Header("Stats")]
    public float maxMana = 100;
    public float maxStamina = 100;
    public float manaRegen = 1;
    public float staminaRegen = 1;
    public float jumpCost = 0;
    public float blockCost = 5;
    public float dodgeCost = 5;
    public float attackCost = 5;
    public float sprintCost = 5;
    public float climbCost = 5;
    public float dodgeStrength = 10;
    public float jumpHeight = 4;
    public float idleSpeed = 5;
    public float crouchSpeed = 2;
    public float sprintSpeed = 8;
    [HideInInspector] public float mana = 100;
    [HideInInspector] public float stamina = 100;
    [HideInInspector] public Vector3 impact = Vector3.zero;

    [Header("Inventory")]
    public Inventory inventory;
    [HideInInspector] public Container container;
    [HideInInspector] public bool containerOpen = false;
    [HideInInspector] public bool inventoryOpen = false;
    [HideInInspector] public GameObject[] iSlot;

    #region Init
    public string GetAvatar() {
      if (File.Exists(Application.persistentDataPath + "/Avatar.txt")) {
        recipeAvatar = File.ReadAllText(Application.persistentDataPath + "/Avatar.txt");
        return recipeAvatar;
      }
      return null;
    }

    public void LoadAvatar(string recipe) {
      avatar.ClearSlots();
      avatar.LoadFromRecipeString(recipe);
      avatar.name = "Player";
      avatar.BuildCharacter();
    }


    public void Teleport(Vector3 pos) { transform.position = pos; }

    public override void Respawn() {
      Teleport(spawnPoint);
      Cursor.visible = false;
      Cursor.lockState = CursorLockMode.Locked;
      avatar.ClearSlots();
      if (avatar.activeRace.name == "HumanMaleDCS") {
        avatar.GetDNA()["height"].Set(0.4f);
        avatar.SetSlot("Underwear", "MaleUnderwear");
      }
      else if (avatar.activeRace.name == "HumanFemaleDCS") {
        avatar.GetDNA()["height"].Set(0.6f);
        avatar.SetSlot("Underwear", "FemaleUndies2");
      }
      equipment.init();
      health.Value = maxHealth / 2;
      stamina = maxStamina / 2;
      mana = maxMana / 2;
      grounded = true;
      crouching = false;
      SetState(0);
      DisableRagdoll();
    }
    #endregion

    #region Actions
    public void Jump() {
      if (state == (int)pS.Climb) { SetState(0); rb.isKinematic = false; }
      else if (grounded) {
        if (state == 0 || state == (int)pS.Block || state == (int)pS.Sprint) {
          if (!StaminaCost(jumpCost)) { return; }
          rb.velocity += jumpHeight * Vector3.up;
          anim.SetTrigger("Jump");
        }
      }
      else if (HitWall()) {
        SetState((int)pS.Climb);
        anim.SetTrigger("Climb");
        rb.isKinematic = true;
      }
    }

    public bool HitWall() {
      RaycastHit hit = new RaycastHit();
      Ray ray = new Ray(col.bounds.center, transform.forward);
      if (Physics.Raycast(ray, out hit, 0.5f)) {
        transform.rotation = Quaternion.FromToRotation(transform.forward, -hit.normal) * transform.rotation;
        return true;
      }
      else { return false; }
    }

    public void ClimbLedge() {
      RaycastHit hit = new RaycastHit();
      Ray ray = new Ray(col.bounds.center + (transform.forward), Vector3.down * 5);
      if (Physics.Raycast(ray, out hit, 0.5f)) {
        Teleport(hit.point);
      }
    }

    public void Move(float xIn, float yIn) {
      if (inventoryOpen) { xIn = 0f; yIn = 0f; }
      anim.SetFloat("Horizontal", xIn * speed);
      anim.SetFloat("Vertical", yIn * speed);
      if (state == (int)pS.Climb) {
        if (xIn != 0 || yIn != 0) {
          if (!HitWall() || !StaminaCost(climbCost * Time.deltaTime)) {
            ClimbLedge();
            SetState(0);
            rb.isKinematic = false;
          }
          else {
            direction = new Vector3(xIn, yIn, 0f);
            direction = transform.TransformDirection(direction) * speed;
            transform.position = transform.position + (direction * Time.deltaTime);
          }
        }
      }
      else {
        direction = new Vector3(xIn, 0f, yIn);
        direction = transform.TransformDirection(direction) * speed;
        Vector3 velocityChange = (direction - rb.velocity);
        velocityChange.x = Mathf.Clamp(velocityChange.x, -10, 10);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -10, 10);
        velocityChange.y = 0;
        if (!grounded || state == (int)pS.Dodge) { rb.AddForce(velocityChange * 3, ForceMode.Impulse); }
        else { rb.AddForce(velocityChange, ForceMode.VelocityChange); }
      }
    }

    public void Look(float xIn, float yIn) {
      if (!inventoryOpen) {
        float rot;
        if (!aiming) { rot = rotationSpeed; }
        else { rot = rotationSpeed * 0.2f; }
        mouseX += xIn * rot;
        mouseY -= yIn * rot;
        mouseY = Mathf.Clamp(mouseY, -60, 60);
      }
      camTarget.transform.rotation = Quaternion.Euler(mouseY, mouseX, 0);
      if (equipment.holstered.Value || direction.sqrMagnitude > 0) {
        if (state != (int)pS.Climb) {
          transform.rotation = Quaternion.Euler(0, mouseX, 0);
          if (state != (int)pS.Dodge) {
            spine.transform.localRotation = Quaternion.Euler(spine.transform.localEulerAngles.x, mouseY, spine.transform.localEulerAngles.z);
          }
        }
      }
      if (state != (int)pS.Dodge) {
        head.transform.localRotation = Quaternion.Euler(0, 0, 0);
        camTarget.transform.position = new Vector3(head.transform.position.x, head.transform.position.y + 0.05f, head.transform.position.z);
      }
    }

    public void Dodge(float xIn, float yIn) {
      if (grounded) {
        if (state == 0 || state == (int)pS.Sprint) {
          if (GetComponent<Hero>()) {
            Hero hero = GetComponent<Hero>();
            if (!hero.StaminaCost(hero.dodgeCost)) { return; }
          }
          SetState((int)pS.Dodge);
          if (yIn < 0) { anim.SetTrigger("Dodge"); rb.velocity += -transform.forward * dodgeStrength; }
          else if (yIn > 0) { anim.SetTrigger("Dodge"); rb.velocity += transform.forward * dodgeStrength; }
          else if (xIn < 0) { anim.SetTrigger("Dodge"); rb.velocity += -transform.right * dodgeStrength; }
          else if (xIn > 0) { anim.SetTrigger("Dodge"); rb.velocity += transform.right * dodgeStrength; }
          else if (yIn == 0) { anim.SetTrigger("Dodge"); rb.velocity += -transform.forward * dodgeStrength; }
        }
      }
    }

    public void Interact() {
      if (containerOpen) {
        CloseInventory();
      }
      else {
        int prevLayer = gameObject.layer;
        gameObject.layer = 2;
        RaycastHit hit = new RaycastHit();
        Ray ray = new Ray(player.cam.transform.position, player.cam.transform.forward);
        Physics.Raycast(ray, out hit, 10);
        gameObject.layer = prevLayer;
        if (hit.collider != null) {
          if (hit.collider.GetComponent<Item>()) {
            if (hit.collider.isTrigger == false) {
              PickupItem(hit.collider.GetComponent<Item>());
            }
          }
          else if (hit.collider.GetComponent<Container>()) {
            if (!hit.collider.GetComponent<Container>().open)
              OpenContainer(hit.collider.GetComponent<Container>());
          }
          else if (hit.collider.GetComponent<Stone>()) {
            hit.collider.GetComponent<Stone>().Pickup(this);
          }
        }
      }
    }
    #endregion

    #region Stats
    public void DropInventory(Container bag) {
      for (int i = 0; i < inventory.nSlots; i++) {
        if (inventory.amount[i] > 0) {
          Slot slot = new Slot();
          slot.amount = inventory.amount[i];
          slot.dItem = inventory.data.GetItem(inventory.item[i]);
          bag.inventory.StoreStack(slot);
          inventory.RemoveStack(i);
        }
      }
    }

    public void UpdateStamina(float amount) {
      stamina += amount;
      if (stamina < 0) { stamina = 0; } else if (stamina > maxStamina) { stamina = maxStamina; }
    }

    public bool StaminaCost(float cost) {
      if (stamina - cost >= 0) {
        stamina -= cost;
        return true;
      }
      else { return false; }
    }

    public void UpdateMana(float amount) {
      mana += amount;
      if (mana < 0) { mana = 0; } else if (mana > maxMana) { mana = maxMana; }
    }

    public bool ManaCost(float cost) {
      if (mana - cost >= 0) {
        mana -= cost;
        return true;
      }
      else { return false; }
    }

    public void Regenerate() {
      UpdateHealth(healthRegen * Time.deltaTime);
      if (grounded && state != (int)pS.Block && state != (int)pS.Climb && state != (int)pS.Sprint) {
        UpdateStamina(staminaRegen * Time.deltaTime);
      }
      UpdateMana(manaRegen * Time.deltaTime);
    }

    public override void SetSpeed() {
      if (aiming) { speed = crouchSpeed; }
      else if (crouching) { speed = crouchSpeed; }
      else if (state == (int)pS.Idle) { speed = idleSpeed; }
      else if (state == (int)pS.Attack) { speed = idleSpeed; }
      else if (state == (int)pS.Block) { speed = crouchSpeed; }
      else if (state == (int)pS.Sprint) { speed = sprintSpeed; }
      else if (state == (int)pS.Dodge) { speed = idleSpeed; }
      else if (state == (int)pS.Climb) { speed = crouchSpeed; }
      else if (state == (int)pS.Swim) { speed = crouchSpeed; }
    }
    #endregion

    #region Inventory
    public void ToggleInventory() {
      if (inventoryOpen) { CloseInventory(); }
      else if (!inventoryOpen) { OpenInventory(); }
    }

    public void OpenInventory() {
      Cursor.visible = true;
      Cursor.lockState = CursorLockMode.None;
      inventoryOpen = true;
      hud.inventoryUI.gameObject.SetActive(true);
      hud.Refresh();
      hud.DisplayStats();
    }

    public void CloseInventory() {
      Cursor.visible = false;
      Cursor.lockState = CursorLockMode.Locked;
      if (containerOpen) {
        container.Close();
        container = null;
        containerOpen = false;
        hud.containerUI.SetActive(false);
      }
      inventoryOpen = false;
      hud.inventoryUI.gameObject.SetActive(false);
    }

    public void OpenContainer(Container hitContainer) {
      OpenInventory();
      container = hitContainer;
      container.interactingHero = this;
      container.Open(this);
      hud.containerUI.SetActive(true);
      containerOpen = true;
      for (int i = 0; i < container.inventory.nSlots; i++) {
        hud.cSlot[i].number = i;
        hud.cSlot[i].hero = this;
        hud.cSlot[i].slot.amount = container.inventory.amount[i];
        hud.cSlot[i].slot.dItem = inventory.data.GetItem(container.inventory.item[i]);
        hud.cSlot[i].UpdateSlot();
      }
      hud.cSlots(container.inventory.nSlots);
    }

    public void PickupItem(Item item) {
      int freeSlot = inventory.FreeSlot();
      if (freeSlot >= 0) {
        inventory.Store(item.dItem, 1);
        item.Despawn();
        hud.Refresh();
      }
    }
    #endregion
  }
}
