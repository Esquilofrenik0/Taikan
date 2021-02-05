using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.SceneManagement;

namespace Postcarbon {
  public class LobbyMenu: NetworkedBehaviour {
    public GameObject HUD;
    public GameObject menuPanel;

    void Start() {
      HUD.SetActive(false);
      menuPanel.SetActive(true);
#if UNITY_SERVER
      Server();
      return;
#endif
      // Host();
    }

    public void Host() {
      menuPanel.SetActive(false);
      HUD.SetActive(true);
      NetworkingManager.Singleton.StartHost();
    }

    public void Server() {
      menuPanel.SetActive(false);
      HUD.SetActive(false);
      NetworkingManager.Singleton.StartServer();
    }

    public void Client() {
      menuPanel.SetActive(false);
      HUD.SetActive(true);
      NetworkingManager.Singleton.StartClient();
    }
  }
}
