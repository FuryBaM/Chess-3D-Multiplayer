using System;
using System.Net;
using kcp2k;
using Mirror;
using Mirror.Discovery;
using TMPro;
using UnityEngine;

public class ServerConnectElement : MonoBehaviour
{
    private ServerResponse _info;
    [SerializeField] private TextMeshProUGUI _nameText;

    public void SetServerData(ServerResponse info)
    {
        _info = info;
        _nameText.text = $"{_info.uri.Host}:{_info.uri.Port}";
    }
    public void JoinServer()
    {
        NetworkManager.singleton.StartClient(_info.uri);
    }
}
