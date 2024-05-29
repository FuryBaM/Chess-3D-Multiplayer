using System.Net;
using kcp2k;
using Mirror;
using TMPro;
using UnityEngine;

public class ServerConnectElement : MonoBehaviour
{
    private IPAddress _ipAddress;
    private ushort _port;
    [SerializeField] private TextMeshProUGUI _nameText;
    public void SetServerData(IPAddress address, ushort port)
    {
        _ipAddress = address;
        _port = port;
    }
    public void JoinServer()
    {
        NetworkManager.singleton.networkAddress = _ipAddress.ToString();
        NetworkManager.singleton.GetComponent<KcpTransport>().port = _port;
        NetworkManager.singleton.StartClient();
    }
}
