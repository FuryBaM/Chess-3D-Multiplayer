using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStarter : MonoBehaviour
{
    private void Start()
    {
        if (Utils.IsHeadless())
        {
            GetComponent<MenuNavigation>().LoadLobby();
        }
    }
}
