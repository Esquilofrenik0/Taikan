using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MLAPI;

namespace Postcarbon {
  public class RSlot: NetworkedBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    [HideInInspector] public Hero hero;
    [HideInInspector] public int number;
    [HideInInspector] public dRecipe recipe;
    [HideInInspector] public Image bgImage;
    [HideInInspector] public Sprite background;
    [HideInInspector] public Text textAmount;
    [HideInInspector] public GameObject slotIcon;
    [HideInInspector] public GameObject thisSlot;


    void Awake() {
      bgImage = GetComponent<Image>();
      slotIcon = transform.GetChild(0).gameObject;
      background = slotIcon.GetComponent<Image>().sprite;
      textAmount = slotIcon.GetComponentInChildren<Text>(true);
    }

    public void UpdateSlot() {
      recipe = hero.hud.data.recipes[number];
      if (recipe) { slotIcon.GetComponent<Image>().sprite = recipe.result.dItem.icon; }
      else { slotIcon.GetComponent<Image>().sprite = background; }
    }

    public void OnPointerClick(PointerEventData pointerEventData) {
      List<int> invSlot = new List<int>();
      bool canCraft = false;
      for (int i = 0; i < recipe.cost.Count; i++) {
        invSlot.Add(hero.inventory.SearchItem(recipe.cost[i].dItem));
        if (invSlot[i] >= 0 && hero.inventory.amount[invSlot[i]] >= recipe.cost[i].amount) {
          canCraft = true;
        }
        else {
          canCraft = false;
          return;
        }
      }
      if (canCraft) {
        for (int i = 0; i < recipe.cost.Count; i++) {
          hero.inventory.Remove(invSlot[i], recipe.cost[i].amount);
        }
        hero.inventory.Store(recipe.result.dItem, recipe.result.amount);
      }
    }

    public void OnPointerEnter(PointerEventData pointerEventData) {
      bgImage.color = new Vector4(255, 255, 0, 200);
      hero.hud.DisplayRecipeInfo(recipe);
    }

    public void OnPointerExit(PointerEventData pointerEventData) {
      bgImage.color = new Vector4(255, 255, 255, 100);
      hero.hud.ResetRecipeInfo();
    }
  }
}