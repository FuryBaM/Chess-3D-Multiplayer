using System.Collections.Generic;
using System.Net;
using kcp2k;
using Mirror;
using Mirror.Discovery;
using UnityEngine;

public class ServerListViewer : MonoBehaviour
{
    readonly Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();
    [SerializeField] private NetworkDiscovery _networkDiscovery;
    [SerializeField] private ServerConnectElement _serverPrefab;
    [SerializeField] private Transform _serverContent;
    private void Start()
    {
        if (_networkDiscovery == null)
        {
            _networkDiscovery = GetComponent<NetworkDiscovery>();
        }
        _networkDiscovery.OnServerFound.AddListener(OnServerFound);
    }

    public void OnServerFound(ServerResponse info)
    {
        Debug.LogError($"server found {info.EndPoint.Address}");
        discoveredServers[info.serverId] = info;
        AddServer(info.EndPoint.Address, info.EndPoint.Port);
    }

    private void AddServer(IPAddress address, int port)
    {
        ServerConnectElement serverElement = Instantiate(_serverPrefab);
        serverElement.SetServerData(address, (ushort)port);
    }
    public void StartGame()
    {
        Debug.LogError($"{NetworkManager.singleton.networkAddress} {NetworkManager.singleton.GetComponent<KcpTransport>().port}");
        NetworkManager.singleton.networkAddress = "localhost";
        NetworkManager.singleton.StartHost();
    }
}
