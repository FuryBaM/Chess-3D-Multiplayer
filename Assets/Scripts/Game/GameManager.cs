using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private Board _board;

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
        _board.OnCheck.AddListener(OnCheck);
        _board.OnMate.AddListener(OnMate);
        _board.OnStalemate.AddListener(OnStalemate);
        _board.OnCapture.AddListener(OnCapture);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            _board.LogBoardState();
            Debug.Log(_board.GetPieceAtPosition(new Vector2Int(0,0)).GetComponent<NetworkIdentity>().netId);
            Debug.Log(_board.GetPieceAtPosition(new Vector2Int(1,0)).GetComponent<NetworkIdentity>().netId);
        }
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        print("Client Connected");
        if (isServer)
        {
            _board.ImportFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");
        }
        if (NetworkServer.connections.Count >= 2)
        {
            print("Enough Players");
            CmdGiveColor();
        }
    }
    [Command]
    private void CmdGiveColor()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        players[0].SetPlayerSide(Side.white);
        players[1].SetPlayerSide(Side.black);
    }

    private void OnDisable()
    {
        _board.OnMate.RemoveListener(OnMate);
        _board.OnCheck.RemoveListener(OnCheck);
        _board.OnStalemate.RemoveListener(OnStalemate);
        _board.OnCapture.RemoveListener(OnCapture);
    }
}
