using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class NetworkManagerRemover : MonoBehaviour
{
    private void Start()
    {
        if (NetworkManager.singleton != null)
            Destroy(NetworkManager.singleton.gameObject);
    }
    private void OnEnable()
    {
        if (NetworkManager.singleton != null)
            Destroy(NetworkManager.singleton.gameObject);
    }
}
