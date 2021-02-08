using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MLAPI;

namespace Postcarbon {
  public class WSlot: NetworkedBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    public Transform slotIcon;
    [HideInInspector] public dWeapon dWeapon;
    [HideInInspector] public bool empty = true;
    [HideInInspector] public Sprite background;
    [HideInInspector] public Image bgImage;
    [HideInInspector] public Hero hero;
    [HideInInspector] public int number;

    void Start() {
      bgImage = GetComponent<Image>();
      background = slotIcon.GetComponent<Image>().sprite;
    }

    public void UpdateSlot() {
      if (hero.equipment.weapon[number]) {
        dWeapon = hero.equipment.weapon[number];
        slotIcon.GetComponent<Image>().sprite = dWeapon.icon;
        empty = false;
      }
      else {
        dWeapon = null;
        slotIcon.GetComponent<Image>().sprite = background;
        empty = true;
      }
    }

    public void OnPointerClick(PointerEventData pointerEventData) {
      if (!hero) { return; }
      if (hero.equipment.weapon[number]) {
        dWeapon = hero.equipment.weapon[number];
        hero.hud.WriteWorldInfo("Unequipped " + dWeapon.name);
        hero.equipment.UnequipWeapon(number);
      }
      UpdateSlot();
    }

    public void OnPointerEnter(PointerEventData pointerEventData) {
      if (!hero) { return; }
      bgImage.color = new Vector4(255, 255, 0, 200);
      if (hero.equipment.weapon[number]) {
        dWeapon = hero.equipment.weapon[number];
        hero.hud.DisplayItemInfo(dWeapon);
      }
      UpdateSlot();
    }

    public void OnPointerExit(PointerEventData pointerEventData) {
      if (!hero) { return; }
      bgImage.color = new Vector4(255, 255, 255, 100);
      hero.hud.ResetInfo();
    }
  }
}