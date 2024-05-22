using Mirror;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private Board _board;
    private bool _playersReady = false;

    private void OnCheck()
    {
        print($"Check! Player {_board.Player} has to move.");
    }
    private void OnMate()
    {
        print($"Mate! Player {1 - _board.Player} has won.");
    }
    private void OnStalemate()
    {
        print($"Draw!");
    }
    private void OnCapture(Piece capturedPiece)
    {
        print($"{capturedPiece} is captured.");
    }
    private void OnEnable() 
    {
        NetworkServer.OnConnectedEvent += OnConnected;
        _board.OnCheck.AddListener(OnCheck);
        _board.OnMate.AddListener(OnMate);
        _board.OnStalemate.AddListener(OnStalemate);
        _board.OnCapture.AddListener(OnCapture);
    }
    private void Update()
    {
        if (NetworkServer.connections.Count >= 2 && _playersReady == false)
        {
            foreach(var conn in NetworkServer.connections)
            {
                if (!conn.Value.isReady)
                {
                    return;
                }
            }
            GiveColor();
        }
    }

    private void OnConnected(NetworkConnectionToClient client)
    {
        print("Client Connected");
        print($"{client.address} {client.connectionId} {client.isReady}");
        if (NetworkServer.connections.Count >= 2)
        {
            print("Enough Players");
            _board.ImportFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");
        }
    }
    public void GiveColor()
    {
        if (NetworkServer.connections.Count < 2) return;
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        players[0].SetPlayerSide(Side.white);
        players[1].SetPlayerSide(Side.black);
        _playersReady = true;
    }

    private void OnDisable()
    {
        NetworkServer.OnConnectedEvent-=OnConnected;
        _board.OnMate.RemoveListener(OnMate);
        _board.OnCheck.RemoveListener(OnCheck);
        _board.OnStalemate.RemoveListener(OnStalemate);
        _board.OnCapture.RemoveListener(OnCapture);
    }
}
