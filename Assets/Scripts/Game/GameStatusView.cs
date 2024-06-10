using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class GameStatusView : NetworkBehaviour
{
    [HideInInspector] public static GameStatusView singleton = null;
    [SerializeField] private Board _board;
    [SerializeField] private MoveUIElement _moveElementPrefab;
    [SerializeField] private RectTransform _moveContent;
    [Header("Piece Image Prefabs")]
    [SerializeField] private Image _whiteKingSprite;
    [SerializeField] private Image _whitePawnSprite;
    [SerializeField] private Image _whiteKnightSprite;
    [SerializeField] private Image _whiteBishopSprite;
    [SerializeField] private Image _whiteRookSprite;
    [SerializeField] private Image _whiteQueenSprite;
    [SerializeField] private Image _blackKingSprite;
    [SerializeField] private Image _blackPawnSprite;
    [SerializeField] private Image _blackKnightSprite;
    [SerializeField] private Image _blackBishopSprite;
    [SerializeField] private Image _blackRookSprite;
    [SerializeField] private Image _blackQueenSprite;
    private List<MoveUIElement> _currentMoveElements = new List<MoveUIElement>();
    private List<Image> _whiteCapturedPieces = new List<Image>();
    private List<Image> _blackCapturedPieces = new List<Image>();

    private int _whiteScore = 0;
    private int _blackScore = 0;

    private void Start () 
    {
        if (singleton == null) 
        {
            singleton = this;
        } 
        else if(singleton == this)
        {
            Destroy(gameObject);
        }
        UpdateCaptures();
    }

    private void OnEnable()
    {
        _board.OnCapture.AddListener(OnCapture);
        _board.OnCheck.AddListener(OnCheck);
        _board.OnMakeMove.AddListener(OnMove);
        _board.OnMate.AddListener(OnMate);
        _board.OnCastle.AddListener(OnCastle);
        _board.OnStalemate.AddListener(OnMate);
        _board.OnPromotion.AddListener(OnMove);
    }

    private void OnDisable()
    {
        _board.OnCapture.RemoveListener(OnCapture);
        _board.OnCheck.RemoveListener(OnCheck);
        _board.OnMakeMove.RemoveListener(OnMove);
        _board.OnMate.RemoveListener(OnMate);
        _board.OnCastle.RemoveListener(OnCastle);
        _board.OnStalemate.RemoveListener(OnMate);
        _board.OnPromotion.RemoveListener(OnMove);
    }
    [Server]
    private MoveUIElement AddMove()
    {
        MoveUIElement currentMoveElement = Instantiate(_moveElementPrefab);
        _currentMoveElements.Add(currentMoveElement);
        NetworkServer.Spawn(currentMoveElement.gameObject);
        return currentMoveElement;
    }
    private void OnMove()
    {
        MoveUIElement currentMoveElement;
        if (_board.Player == 1)
        {
            currentMoveElement = AddMove();
            currentMoveElement.SetMoveNumeration(_board.CurrentMove / 2);
            Move move = _board.GetLastMove();
            Piece piece = move.MovedPiece;
            currentMoveElement.SetWhiteMove(MoveConverter.ConvertMoveToString(move, piece));
            ClientUI.singleton.SetMoveOwner(Side.black);
        }
        else
        {
            currentMoveElement = _currentMoveElements.Last();
            Move move = _board.GetLastMove();
            Piece piece = move.MovedPiece;
            currentMoveElement.SetBlackMove(MoveConverter.ConvertMoveToString(move, piece));
            ClientUI.singleton.SetMoveOwner(Side.white);
        }
    }
    
    private void OnCapture(uint capturedPieceId)
    {
        Piece piece = NetworkClient.spawned[capturedPieceId].GetComponent<Piece>();
        // Увеличиваем счет за захваты
        int captureValue = GetPieceValue(piece);
        if (piece.Side == Side.white)
        {
            _blackScore += captureValue;
        }
        else
        {
            _whiteScore += captureValue;
        }

        // Обновляем отображение счета за захваты
        UpdateCaptures();
    }
    
    private int GetPieceValue(Piece piece)
    {
        if (piece.GetType() == typeof(Pawn))
        {
            return 1;
        }
        else if (piece.GetType() == typeof(Knight))
        {
            return 3;
        }
        else if (piece.GetType() == typeof(Bishop))
        {
            return 3;
        }
        else if (piece.GetType() == typeof(Rook))
        {
            return 5;
        }
        else if (piece.GetType() == typeof(Queen))
        {
            return 9;
        }
        else if (piece.GetType() == typeof(King))
        {
            // Обычно король не захватывается, но вы можете добавить соответствующую логику,
            // если хотите учитывать это
            return 0;
        }
        else
        {
            return 0; // Для случая, если тип фигуры не совпадает с ожидаемыми
        }
    }
    
    private void OnCheck()
    {
        // Дополнительные действия при шахе
    }
    
    private void OnMate()
    {
        // Дополнительные действия при мате
    }
    
    private void OnCastle()
    {
        OnMove();
    }
    
    private void UpdateCaptures()
    {
        UpdateCapturedPieces(Side.white, _whiteCapturedPieces, _board.CapturedPieces[Side.white]);
        UpdateCapturedPieces(Side.black, _blackCapturedPieces, _board.CapturedPieces[Side.black]);
    }
    
    private void UpdateCapturedPieces(Side side, List<Image> capturedPieces, List<Piece> pieces)
    {
        // Удаляем все текущие отображаемые фигуры
        foreach (var capturedPiece in capturedPieces)
        {
            NetworkServer.Destroy(capturedPiece.gameObject);
        }
        capturedPieces.Clear();

        // Создаем новые отображаемые фигуры на основе списка pieces
        int maxSprites = 6; // Максимальное количество спрайтов для каждого цвета
        foreach (var piece in pieces)
        {
            // Получаем спрайт в зависимости от типа фигуры и ее цвета
            Image sprite = GetPieceSprite(piece);
            if (sprite != null && capturedPieces.Count < maxSprites)
            {
                // Создаем новую отображаемую фигуру с соответствующим спрайтом
                Image capturedPieceImage = Instantiate(sprite);
                capturedPieceImage.rectTransform.sizeDelta = new Vector2(50, 50);
                NetworkServer.Spawn(capturedPieceImage.gameObject);
                capturedPieces.Add(capturedPieceImage);
            }
        }
    }
    
    private Image GetPieceSprite(Piece piece)
    {
        if (piece.GetType() == typeof(Pawn))
        {
            return piece.Side == Side.white ? _whitePawnSprite : _blackPawnSprite;
        }
        else if (piece.GetType() == typeof(Knight))
        {
            return piece.Side == Side.white ? _whiteKnightSprite : _blackKnightSprite;
        }
        else if (piece.GetType() == typeof(Bishop))
        {
            return piece.Side == Side.white ? _whiteBishopSprite : _blackBishopSprite;
        }
        else if (piece.GetType() == typeof(Rook))
        {
            return piece.Side == Side.white ? _whiteRookSprite : _blackRookSprite;
        }
        else if (piece.GetType() == typeof(Queen))
        {
            return piece.Side == Side.white ? _whiteQueenSprite : _blackQueenSprite;
        }
        else if (piece.GetType() == typeof(King))
        {
            return piece.Side == Side.white ? _whiteKingSprite : _blackKingSprite;
        }
        else
        {
            return null; // Если тип фигуры не совпадает с ожидаемыми
        }
    }
}
