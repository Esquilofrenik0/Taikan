using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UMA;
using UMA.CharacterSystem;
using System.IO;

namespace SRPG {
  public class AvatarCreator: MonoBehaviour {
    public InputHandler input;
    public InputField iLoadName;
    public InputField iSaveName;
    public Transform cam;
    public DynamicCharacterAvatar avatar;
    public Dictionary<string, DnaSetter> dna;
    public Slider heightSlider;
    public Slider bellySlider;
    private Coroutine getDna;
    public List<string> hairModels = new List<string>();
    private int currentHair;
    [HideInInspector] public string characterName = "";
    [HideInInspector] public string loadName = "";
    [HideInInspector] public string myRecipe;

    void OnEnable() {
      avatar.CharacterUpdated.AddListener(Updated);
      heightSlider.onValueChanged.AddListener(HeightChange);
      bellySlider.onValueChanged.AddListener(BellyChange);
    }

    void OnDisable() {
      avatar.CharacterUpdated.RemoveListener(Updated);
      heightSlider.onValueChanged.RemoveListener(HeightChange);
      bellySlider.onValueChanged.RemoveListener(BellyChange);
    }

    void Start() {
      ChangeName("Avatar");
      Timer.Delay(this, SwitchFemale, 0f);
      Timer.Delay(this, SwitchMale, 0.1f);
      Timer.Delay(this, SwitchFemale, 0.2f);
      Timer.Delay(this, SwitchMale, 0.3f);
    }

    float mouseX = 0;

    void Update() {
      if (input.attack) {
        mouseX -= input.camvect.x * 5;
        transform.rotation = Quaternion.Euler(0, 180 + mouseX, 0);

      }
      if (input.firstPerson) {
        cam.position = new Vector3(0, 1.5f, -1f);
      }
      else {
        cam.position = new Vector3(0, 1f, -2f);
      }

    }

    void getDNA() {
      dna = avatar.GetDNA();
    }

    void Updated(UMAData data) {
      dna = avatar.GetDNA();
      heightSlider.value = dna["height"].Get();
      bellySlider.value = dna["belly"].Get();
      avatar.BuildCharacter();
    }

    public void SwitchFemale() {
      SwitchGender(false);
    }

    public void SwitchMale() {
      SwitchGender(true);
    }

    public void SwitchGender(bool male) {
      if (male && avatar.activeRace.name != "HumanMaleDCS") {
        avatar.ChangeRace("HumanMaleDCS");
        avatar.SetSlot("Underwear", "MaleUnderwear");
        avatar.GetDNA()["height"].Set(0.4f);
        avatar.BuildCharacter();
      }
      if (!male && avatar.activeRace.name != "HumanFemaleDCS") {
        avatar.ChangeRace("HumanFemaleDCS");
        avatar.SetSlot("Underwear", "FemaleUndies2");
        avatar.GetDNA()["height"].Set(0.6f);
        avatar.BuildCharacter();
      }
    }

    public void HeightChange(float val) {
      dna["height"].Set(val);
      avatar.BuildCharacter();
    }

    public void BellyChange(float val) {
      dna["belly"].Set(val);
      avatar.BuildCharacter();
    }

    public void ChangeSkinColor(Color col) {
      avatar.SetColor("Skin", col);
      avatar.UpdateColors(true);
    }

    public void ChangeHairColor(Color col) {
      avatar.SetColor("Hair", col);
      avatar.UpdateColors(true);
    }

    public void ChangeEyeColor(Color col) {
      avatar.SetColor("Eyes", col);
      avatar.UpdateColors(true);
    }

    public void ChangeHair(bool plus) {
      if (plus) { currentHair++; }
      else { currentHair--; }
      currentHair = Mathf.Clamp(currentHair, 0, hairModels.Count - 1);
      if (hairModels[currentHair] == "None") { avatar.ClearSlot("Hair"); }
      else { avatar.SetSlot("Hair", hairModels[currentHair]); }
      avatar.BuildCharacter();
    }

    public void SetLoadName(string loadThis) {
      loadName = loadThis;
    }

    public void ChangeName(string newName) {
      characterName = newName;
      avatar.name = characterName;
      iSaveName.text = characterName;
    }

    public void SaveRecipe() {
      myRecipe = avatar.GetCurrentRecipe();
      File.WriteAllText(Application.persistentDataPath + "/" + characterName + ".txt", myRecipe);
    }

    public void LoadRecipe() {
      if (System.IO.File.Exists(Application.persistentDataPath + "/" + loadName + ".txt")) {
        myRecipe = File.ReadAllText(Application.persistentDataPath + "/" + loadName + ".txt");
        avatar.ClearSlots();
        avatar.LoadFromRecipeString(myRecipe);
      }
    }

    public void SaveAvatar() {
      myRecipe = avatar.GetCurrentRecipe();
      File.WriteAllText(Application.persistentDataPath + "/" + "Avatar.txt", myRecipe);
    }

    public void ExitToLobby() {
      ChangeName(characterName);
      File.WriteAllText(Application.persistentDataPath + "/" + "AvatarName.txt", characterName);
      SaveAvatar();
      UnityEngine.SceneManagement.SceneManager.LoadScene("GameLobby");
    }
  }
}
