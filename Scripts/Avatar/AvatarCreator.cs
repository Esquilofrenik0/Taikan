using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UMA;
using UMA.CharacterSystem;
using System.IO;

namespace SRPG {
  public class AvatarCreator : MonoBehaviour {
    public InputHandler input;
    public InputField iLoadName;
    public InputField iSaveName;
    public Transform cam;
    public DynamicCharacterAvatar avatar;
    public Dictionary<string, DnaSetter> dna;
    public Slider heightSlider;
    public Slider bellySlider;
    private Coroutine getDna;
    public List<string> maleHairModels = new List<string>();
    private int maleCurrentHair;
    public List<string> femaleHairModels = new List<string>();
    private int femaleCurrentHair;
    public string characterName = "";
    public string loadName = "";
    public string myRecipe;

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
      Timer.Delay(this, SwitchMale, 0.2f);
      Timer.Delay(this, SwitchFemale, 0.4f);
      Timer.Delay(this, SwitchMale, 0.6f);
    }

    float mouseX = 0;

    void Update() {
      if (input.attack) {
        mouseX -= input.camvect.x * 5;
        transform.rotation = Quaternion.Euler(0, 180 + mouseX, 0);

      }
      if (input.firstPerson) {
        cam.position = new Vector3(0, 1.5f, -1f);
      } else {
        cam.position = new Vector3(0, 1f, -2f);
      }

    }

    void getDNA() {
      dna = avatar.GetDNA();
    }

    void Updated(UMAData data) {
      dna = avatar.GetDNA();
      if (avatar.activeRace.name == "HumanMaleDCS") {
        dna["height"].Set(0.5f);
      }
      if (avatar.activeRace.name == "HumanFemaleDCS") {
        dna["height"].Set(0.6f);
      }
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
        avatar.BuildCharacter();
      }
      if (!male && avatar.activeRace.name != "HumanFemaleDCS") {
        avatar.ChangeRace("HumanFemaleDCS");
        avatar.SetSlot("Underwear", "FemaleUndies2");
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

    public void ChangeHair(bool plus) {
      if (avatar.activeRace.name == "HumanMaleDCS") {
        if (plus) { maleCurrentHair++; } else { maleCurrentHair--; }
        maleCurrentHair = Mathf.Clamp(maleCurrentHair, 0, maleHairModels.Count - 1);
        if (maleHairModels[maleCurrentHair] == "None") { avatar.ClearSlot("Hair"); } else { avatar.SetSlot("Hair", maleHairModels[maleCurrentHair]); }
      } else if (avatar.activeRace.name == "HumanFemaleDCS") {
        if (plus) { femaleCurrentHair++; } else { femaleCurrentHair--; }
        femaleCurrentHair = Mathf.Clamp(femaleCurrentHair, 0, femaleHairModels.Count - 1);
        if (femaleHairModels[femaleCurrentHair] == "None") { avatar.ClearSlot("Hair"); } else { avatar.SetSlot("Hair", femaleHairModels[femaleCurrentHair]); }
      }
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
