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
}
