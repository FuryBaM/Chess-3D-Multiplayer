using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameStarter : MonoBehaviour
{
    private void Start()
    {
        NetworkManager.singleton.StartHost();
    }
}
