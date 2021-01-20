using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace SRPG {
  [System.Serializable]
  public class Hero: Human {
    [Header("Components")]
    public Player player;
    public HUD hud;
    public CharacterController cc;
    [HideInInspector] public string recipeAvatar;

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
    public float dodgeStrength = 20;
    public float idleSpeed = 5;
    public float crouchSpeed = 2;
    public float sprintSpeed = 8;
    public float jumpHeight = 2;
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
    public void LoadAvatar() {
      if (File.Exists(Application.persistentDataPath + "/Avatar.txt")) {
        recipeAvatar = File.ReadAllText(Application.persistentDataPath + "/Avatar.txt");
        avatar.ClearSlots();
        avatar.LoadFromRecipeString(recipeAvatar);
        avatar.name = "Player";
        avatar.BuildCharacter();
      }
    }

    public void Teleport(Vector3 pos) {
      cc.enabled = false;
      transform.position = pos;
      cc.enabled = true;
    }

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
      equipment.Dress();
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
    public void AddImpact(Vector3 dir, float force) {
      dir.Normalize();
      if (dir.y < 0) dir.y = -dir.y;
      impact += dir.normalized * force;
    }

    public void UpdateImpact() {
      if (impact.magnitude > 1) { cc.Move(impact * Time.deltaTime); }
      impact = Vector3.Lerp(impact, Vector3.zero, 5 * Time.deltaTime);
    }

    public void ApplyGravity() {
      if (grounded && velocity.y < 0 || state == (int)pS.Climb) { velocity.y = 0f; }
      else { velocity.y += gravity * Time.deltaTime; }
      cc.Move(velocity * Time.deltaTime);
      if (containerOpen) {
        if (Vector3.Distance(transform.position, container.transform.position) > 5) {
          CloseInventory();
        }
      }
    }

    public void Jump() {
      if (state == (int)pS.Climb) { SetState(0); }
      else if (grounded) {
        if (state == 0 || state == (int)pS.Block || state == (int)pS.Sprint) {
          if (!StaminaCost(jumpCost)) { return; }
          velocity.y += Mathf.Sqrt(jumpHeight * -gravity);
          anim.SetTrigger("Jump");
        }
      }
      else if (StaminaCost(climbCost * Time.deltaTime)) {
        if (HitWall()) {
          SetState((int)pS.Climb);
          anim.SetTrigger("Climb");
        }
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
      if (xIn != 0 || yIn != 0) {
        if (state == (int)pS.Climb) {
          if (!HitWall() || !StaminaCost(climbCost * Time.deltaTime)) {
            ClimbLedge();
            SetState(0);
          }
          else { direction = new Vector3(xIn, yIn, 0f); }
        }
        else { direction = new Vector3(xIn, 0f, yIn); }
        direction = transform.rotation * direction * speed;
        cc.Move(direction * Time.deltaTime);
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
        if (state == 0 || state == (int)pS.Block || state == (int)pS.Sprint) {
          if (GetComponent<Hero>()) {
            Hero hero = GetComponent<Hero>();
            if (!hero.StaminaCost(hero.dodgeCost)) { return; }
          }
          SetState((int)pS.Dodge);
          if (yIn < 0) { anim.SetInteger("DodgeDirection", 0); anim.SetTrigger("Dodge"); AddImpact(-transform.forward, dodgeStrength); }
          else if (yIn > 0) { anim.SetInteger("DodgeDirection", 1); anim.SetTrigger("Dodge"); AddImpact(transform.forward, dodgeStrength); }
          else if (xIn < 0) { anim.SetInteger("DodgeDirection", 2); anim.SetTrigger("Dodge"); AddImpact(-transform.right, dodgeStrength); }
          else if (xIn > 0) { anim.SetInteger("DodgeDirection", 3); anim.SetTrigger("Dodge"); AddImpact(transform.right, dodgeStrength); }
          else if (yIn == 0) { anim.SetInteger("DodgeDirection", 0); anim.SetTrigger("Dodge"); AddImpact(-transform.forward, dodgeStrength); }
        }
      }
    }

    public void Interact() {
      if (containerOpen) {
        CloseInventory();
      }
      else {
        Ray ray = new Ray(camTarget.transform.position, camTarget.transform.rotation * Vector3.forward);
        RaycastHit hit = new RaycastHit();
        Physics.SphereCast(ray, 1, out hit, 5);
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
        }
      }
    }
    #endregion

    #region Stats
    // public void DropInventory(Container bag) {
    //   for (int i = 0; i < 7; i++) {
    //     if (equipment.equipSlot[i].GetComponentInChildren<Item>()) {
    //       Item item = equipment.equipSlot[i].GetComponentInChildren<Item>();
    //       bag.inventory.Store(item.dItem, 1);
    //       GameObject.Destroy(item.gameObject);
    //     }
    //   }
    //   for (int i = 0; i < inventory.nSlots; i++) {
    //     if (inventory.slot[i].amount > 0) {
    //       bag.inventory.StoreStack(inventory.slot[i]);
    //       inventory.RemoveStack(i);
    //     }
    //   }
    // }

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

    public void RefreshState() {
      anim.SetBool("Crouching", crouching);
      SetSpeed();
    }

    public void SetSpeed() {
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
      if (inventoryOpen) { CloseInventory(); } else if (!inventoryOpen) { OpenInventory(); }
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
        hud.cSlot[i].slot = container.inventory.slot[i];
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

    public void ClickItem(dItem dItem, int number) {
      if (containerOpen) {
        int freeSlot = container.inventory.FreeSlot();
        if (freeSlot >= 0) { container.Store(number); }
      }
      else {
        if (dItem.type == iT.Armor || dItem.type == iT.Weapon) {
          equipment.EquipItem(dItem);
          inventory.RemoveStack(number);
        }
      }
      hud.Refresh();
      hud.DisplayStats();
    }
    #endregion
  }
}
