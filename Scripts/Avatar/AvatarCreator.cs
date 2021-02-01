using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UMA;
using UMA.CharacterSystem;
using System.IO;

namespace Postcarbon {
  public class AvatarCreator: MonoBehaviour {
    public InputHandler input;
    public InputField iLoadName;
    public InputField iSaveName;
    public Transform cam;
    public DynamicCharacterAvatar avatar;
    public Dictionary<string, DnaSetter> dna;
    public Slider earSlider;
    public Slider chinSlider;
    public Slider jawSlider;
    public Slider ageSlider;
    public Slider eyeRotationSlider;
    public List<string> hairModels = new List<string>();
    private int currentHair;
    private float mouseX = 0;
    private Coroutine getDna;
    [HideInInspector] public string characterName = "";
    [HideInInspector] public string loadName = "";
    [HideInInspector] public string myRecipe;

    void OnEnable() {
      avatar.CharacterUpdated.AddListener(Updated);
      earSlider.onValueChanged.AddListener(EarChange);
      chinSlider.onValueChanged.AddListener(ChinChange);
      jawSlider.onValueChanged.AddListener(JawChange);
      ageSlider.onValueChanged.AddListener(AgeChanged);
      eyeRotationSlider.onValueChanged.AddListener(EyeRotationChange);
    }

    void OnDisable() {
      avatar.CharacterUpdated.RemoveListener(Updated);
      earSlider.onValueChanged.RemoveListener(EarChange);
      chinSlider.onValueChanged.RemoveListener(ChinChange);
      jawSlider.onValueChanged.RemoveListener(JawChange);
      ageSlider.onValueChanged.RemoveListener(AgeChanged);
      eyeRotationSlider.onValueChanged.RemoveListener(EyeRotationChange);
    }

    void OnChangeView() {
      if (input.firstPerson) {
        input.firstPerson = false;
        cam.position = new Vector3(0, 1f, -2f);
      }
      else {
        input.firstPerson = true;
        cam.position = new Vector3(0, 1.5f, -1f);
      }
    }

    void Start() {
      ChangeName("Avatar");
      Timer.Delay(this, SwitchFemale, 0f);
      Timer.Delay(this, SwitchMale, 0.1f);
      Timer.Delay(this, SwitchFemale, 0.2f);
      Timer.Delay(this, SwitchMale, 0.3f);
    }

    void FixedUpdate() {
      if (input.attack) {
        mouseX -= input.camvect.x * 5;
        transform.rotation = Quaternion.Euler(0, 180 + mouseX, 0);
      }
    }

    void getDNA() {
      dna = avatar.GetDNA();
    }

    void Updated(UMAData data) {
      dna = avatar.GetDNA();
      earSlider.value = dna["earsSize"].Get();
      chinSlider.value = dna["chinPronounced"].Get();
      jawSlider.value = dna["jawsSize"].Get();
      ageSlider.value = dna["ageBase"].Get();
      eyeRotationSlider.value = dna["eyeRotation"].Get();
      avatar.BuildCharacter();
    }

    public void SwitchFemale() {
      SwitchGender(false);
    }

    public void SwitchMale() {
      SwitchGender(true);
    }

    public void SwitchGender(bool male) {
      if (male) {avatar.ChangeRace("o3n Stunner John");}
      else {avatar.ChangeRace("o3n Stunner Jane");}
      avatar.BuildCharacter();
    }

    public void EarChange(float val) {
      dna["earsSize"].Set(val);
      avatar.BuildCharacter();
    }

    public void ChinChange(float val) {
      dna["chinPronounced"].Set(val);
      avatar.BuildCharacter();
    }

    public void JawChange(float val) {
      dna["jawsSize"].Set(val);
      avatar.BuildCharacter();
    }

    public void AgeChanged(float val) {
      dna["ageBase"].Set(val);
      avatar.BuildCharacter();
    }

    public void EyeRotationChange(float val) {
      dna["eyeRotation"].Set(val);
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
