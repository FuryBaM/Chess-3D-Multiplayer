using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using kcp2k;
using Mirror;
using Mirror.Discovery;
using UnityEngine;
using UnityEngine.UI;

public class ServerListViewer : MonoBehaviour
{
    private Dictionary<long, ServerResponse> _foundServers = new Dictionary<long, ServerResponse>();
    [SerializeField] private NetworkDiscovery _networkDiscovery;
    [SerializeField] private ServerConnectElement _serverPrefab;
    [SerializeField] private Transform _serverContent;
    [SerializeField] private Button _hostButton;
    [SerializeField] private Button _refreshListButton;
    private List<ServerConnectElement> _serversOnScene = new List<ServerConnectElement>();
    private void Start()
    {
        _networkDiscovery = NetworkManager.singleton.GetComponent<NetworkDiscovery>();
        _networkDiscovery.StartDiscovery();
        print($"{_networkDiscovery.secretHandshake}");
    }

    private void OnEnable() 
    {
        _hostButton.onClick.AddListener(StartGame);
        _refreshListButton.onClick.AddListener(UpdateVisuals);    
    }

    public void OnServerFound(ServerResponse info)
    {
        Debug.Log($"server found {info.serverId} {info.EndPoint.Address} {info.EndPoint.ToString()}");
        _foundServers[info.serverId] = info;
    }

    public void UpdateVisuals()
    {
        _networkDiscovery.StartDiscovery();
        foreach(var serverObj in _serversOnScene)
        {
            Destroy(serverObj.gameObject);
        }
        _serversOnScene.Clear();
        foreach (var server in _foundServers)
        {
            AddServer(server.Value);
        }
        _foundServers.Clear();
    }

    private void AddServer(ServerResponse info)
    {
        ServerConnectElement serverElement = Instantiate(_serverPrefab, _serverContent);
        _serversOnScene.Add(serverElement);
        serverElement.SetServerData(info);
    }
    public void StartGame()
    {
        _foundServers.Clear();
        NetworkManager.singleton.StartHost();
        _networkDiscovery.AdvertiseServer();
    }
}
