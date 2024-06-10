using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuNavigation : MonoBehaviour
{
    public void LoadSinglePlayerGame()
    {
        SceneManager.LoadScene("SinglePlayerScene");
    }
    public void StartGame()
    {
        NetworkManager.singleton.StartHost();
    }
    public void LoadLobby()
    {
        SceneManager.LoadScene("LobbyScene");
    }
    public void LoadMenu()
    {
        NetworkManager.singleton.dontDestroyOnLoad = false;
        NetworkManager.singleton.runInBackground = false;
        Destroy(NetworkManager.singleton.gameObject);
        SceneManager.LoadScene("MenuScene");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
