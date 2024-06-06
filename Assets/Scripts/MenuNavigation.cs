using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuNavigation : MonoBehaviour
{
    public void LoadGame()
    {
        SceneManager.LoadScene("GameScene");
    }
    public void LoadMultiplayerGame()
    {
        SceneManager.LoadScene("MultiplayerGameScene");
    }
    public void LoadLobby()
    {
        SceneManager.LoadScene("LobbyScene");
    }
    public void LoadMenu()
    {
        NetworkManager.singleton.dontDestroyOnLoad = false;
        Destroy(NetworkManager.singleton.gameObject);
        SceneManager.LoadScene("MenuScene");
    }
}
