using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using kcp2k;
using Mirror;
using Mirror.Discovery;
using UnityEngine;

public class ServerListViewer : MonoBehaviour
{
    private Dictionary<long, ServerResponse> foundServers = new Dictionary<long, ServerResponse>();
    [SerializeField] private NetworkDiscovery _networkDiscovery;
    [SerializeField] private ServerConnectElement _serverPrefab;
    [SerializeField] private Transform _serverContent;
    private List<ServerConnectElement> _serversOnScene = new List<ServerConnectElement>();
    private void Start()
    {
        if (_networkDiscovery == null)
        {
            _networkDiscovery = GetComponent<NetworkDiscovery>();
        }
        _networkDiscovery.StartDiscovery();
        print($"{_networkDiscovery.secretHandshake}");
    }

    public void OnServerFound(ServerResponse info)
    {
        Debug.Log($"server found {info.serverId} {info.EndPoint.Address} {info.EndPoint.ToString()}");
        foundServers[info.serverId] = info;
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        foreach(var serverObj in _serversOnScene)
        {
            Destroy(serverObj.gameObject);
        }
        _serversOnScene.Clear();
        foreach (var server in foundServers)
        {
            AddServer(server.Value);
        }
    }

    private void AddServer(ServerResponse info)
    {
        ServerConnectElement serverElement = Instantiate(_serverPrefab, _serverContent);
        _serversOnScene.Add(serverElement);
        serverElement.SetServerData(info);
    }
    public void StartGame()
    {
        Debug.LogError($"{NetworkManager.singleton.networkAddress} {NetworkManager.singleton.GetComponent<KcpTransport>().port}");
        NetworkManager.singleton.networkAddress = "localhost";
        NetworkManager.singleton.StartHost();
    }
}
