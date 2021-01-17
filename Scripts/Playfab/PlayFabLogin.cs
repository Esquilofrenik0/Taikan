// using PlayFab;
// using PlayFab.ClientModels;
// using UnityEngine;
// using UnityEngine.UI;

// public class PlayFabLogin: MonoBehaviour {
//   public Text regEmail;
//   public Text regUsername;
//   public Text regPassword;
//   public Text loginUsername;
//   public Text loginPassword;

//   public void Start() {
//     if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId)) {
//       PlayFabSettings.staticSettings.TitleId = "90533";
//     }
//     // var request = new LoginWithCustomIDRequest { CustomId = "GettingStartedGuide", CreateAccount = true };
//     // PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
//   }

//   public void TryToLogIn() {
//     var request = new LoginWithPlayFabRequest { Username = loginUsername.text, Password = loginPassword.text };
//     PlayFabClientAPI.LoginWithPlayFab(request, OnLoginSuccess, OnLoginFailure);
//   }

//   private void OnLoginSuccess(LoginResult result) {
//     Debug.Log("Congratulations, you made your first successful API call!");
//   }

//   private void OnLoginFailure(PlayFabError error) {
//     Debug.LogWarning("Something went wrong with your first API call.  :(");
//     Debug.LogError("Here's some debug information:");
//     Debug.LogError(error.GenerateErrorReport());
//   }


//   public void TryToRegister() {
//     string email = regEmail.text;
//     string username = regUsername.text;
//     string password = regPassword.text;
//     var register = new RegisterPlayFabUserRequest { Email = regEmail.text, Username = regUsername.text, Password = regPassword.text };
//     PlayFabClientAPI.RegisterPlayFabUser(register, OnRegisterSuccess, OnLoginFailure);
//   }

//   private void OnRegisterSuccess(RegisterPlayFabUserResult result) {
//     var request = new LoginWithPlayFabRequest { Username = regUsername.text, Password = regPassword.text };
//     PlayFabClientAPI.LoginWithPlayFab(request, OnLoginSuccess, OnLoginFailure);
//   }
// }