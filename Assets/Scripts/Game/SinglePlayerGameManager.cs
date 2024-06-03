using System;
using Mirror;
using Mirror.Discovery;
using UnityEngine;

public class SinglePlayerGameManager : NetworkBehaviour
{
    [SerializeField] private Board _board;
    private bool _playerReady = false;
    [SerializeField] private AIController aIController;

    private void Start()
    {
        print("Waiting");
    }
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
    private void OnCapture(uint capturedPieceId)
    {
        print($"{NetworkServer.spawned[capturedPieceId]} is captured.");
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
        if (_playerReady == false)
        {
            foreach(var conn in NetworkServer.connections)
            {
                if (!conn.Value.isReady)
                {
                    return;
                }
            }
            GiveColor();
            _board.SyncBoard();
        }
    }

    private void OnConnected(NetworkConnectionToClient client)
    {
        print("Client Connected");
        print($"{client.address} {client.connectionId} {client.isReady}");
        if (NetworkServer.connections.Count >= 1)
        {
            print("Enough Players");
            _board.ImportFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");
        }
    }
    public void GiveColor()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        players[0].SetPlayerSide(Side.white);
        var ai = Instantiate(aIController);
        NetworkServer.Spawn(ai.gameObject);
        _playerReady = true;
    }

    private void OnDisable()
    {
        NetworkServer.OnConnectedEvent -= OnConnected;
        _board.OnMate.RemoveListener(OnMate);
        _board.OnCheck.RemoveListener(OnCheck);
        _board.OnStalemate.RemoveListener(OnStalemate);
        _board.OnCapture.RemoveListener(OnCapture);
    }
}
