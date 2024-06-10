using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ClientUI : NetworkBehaviour
{
    [SerializeField] private Board _board;
    [HideInInspector] public static ClientUI singleton = null;
    [Header("Piece Promotion Buttons")]
    [SerializeField] private Button _knightPromotionButton;
    [SerializeField] private Button _bishopPromotionButton;
    [SerializeField] private Button _rookPromotionButton;
    [SerializeField] private Button _queenPromotionButton;
    [Space(20)]
    [SerializeField] private RectTransform _pawnPromotionPanel;
    [Header("Game Status")]
    [SerializeField] private RectTransform _gameOverStatusPanel;
    [SerializeField] private TextMeshProUGUI _winnerText;
    [SerializeField] private TextMeshProUGUI _winMethodText;
    [SerializeField] private TextMeshProUGUI _moveOwnerText;


    private void Start()
    {
        if (singleton == null) 
        {
            singleton = this;
        } 
        else if(singleton == this)
        {
            Destroy(gameObject);
        }
        Initialize();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Disconnect();
        }
    }
    private void Initialize()
    {
        _knightPromotionButton.onClick.AddListener(()=>_board.RequestPawnPromotion(PieceType.knight));
        _bishopPromotionButton.onClick.AddListener(()=>_board.RequestPawnPromotion(PieceType.bishop));
        _rookPromotionButton.onClick.AddListener(()=>_board.RequestPawnPromotion(PieceType.rook));
        _queenPromotionButton.onClick.AddListener(()=>_board.RequestPawnPromotion(PieceType.queen));
    }
    private void OnDisable()
    {
        _knightPromotionButton.onClick.RemoveListener(()=>_board.RequestPawnPromotion(PieceType.knight));
        _bishopPromotionButton.onClick.RemoveListener(()=>_board.RequestPawnPromotion(PieceType.bishop));
        _rookPromotionButton.onClick.RemoveListener(()=>_board.RequestPawnPromotion(PieceType.rook));
        _queenPromotionButton.onClick.RemoveListener(()=>_board.RequestPawnPromotion(PieceType.queen));
    }
    public void ShowPromotionPanel()
    {
        _pawnPromotionPanel.gameObject.SetActive(true);
    }
    [ClientRpc]
    public void ShowGameOverPanel(string winner, string winMethod)
    {
        _gameOverStatusPanel.gameObject.SetActive(true);
        _winnerText.text = winner;
        _winMethodText.text = winMethod;
    }
    [ClientRpc]
    public void SetMoveOwner(Side side)
    {
        string player = side == Side.white ? "white" : "black";
        _moveOwnerText.text = $"Makes move: {player}";
    }
    public void Disconnect()
    {
        if (isServer)
            NetworkManager.singleton.StopHost();
        else
            NetworkManager.singleton.StopClient();
    }
}
