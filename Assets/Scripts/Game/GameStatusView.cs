using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameStatusView : MonoBehaviour
{
    [SerializeField] private Board _board;
    [SerializeField] private MoveUIElement _moveElementPrefab;
    [SerializeField] private RectTransform _moveContent;
    [SerializeField] private Sprite _whiteKingSprite;
    [SerializeField] private Sprite _whitePawnSprite;
    [SerializeField] private Sprite _whiteKnightSprite;
    [SerializeField] private Sprite _whiteBishopSprite;
    [SerializeField] private Sprite _whiteRookSprite;
    [SerializeField] private Sprite _whiteQueenSprite;
    [SerializeField] private Sprite _blackKingSprite;
    [SerializeField] private Sprite _blackPawnSprite;
    [SerializeField] private Sprite _blackKnightSprite;
    [SerializeField] private Sprite _blackBishopSprite;
    [SerializeField] private Sprite _blackRookSprite;
    [SerializeField] private Sprite _blackQueenSprite;
    [SerializeField] private Image _whiteCapturesImage;
    [SerializeField] private Image _blackCapturesImage;
    private List<MoveUIElement> _currentMoveElements = new List<MoveUIElement>();
    private List<Image> _whiteCapturedPieces = new List<Image>();
    private List<Image> _blackCapturedPieces = new List<Image>();

    private int _whiteScore = 0;
    private int _blackScore = 0;

    private void Start()
    {
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

    private void OnMove()
    {
        MoveUIElement _currentMoveElement;
        if (_board.Player == 1)
        {
            _currentMoveElement = Instantiate(_moveElementPrefab, _moveContent);
            _currentMoveElements.Add(_currentMoveElement);
        }
        else
        {
            _currentMoveElement = _currentMoveElements.Last();
        }

        _currentMoveElement.SetMoveNumeration(_board.CurrentMove / 2);
        if (1 - _board.Player == 0)
        {
            _currentMoveElement.SetWhiteMove(MoveConverter.ConvertMoveToString(_board.GetLastMove()));
        }
        else
        {
            _currentMoveElement.SetBlackMove(MoveConverter.ConvertMoveToString(_board.GetLastMove()));
        }
    }

    private void OnCapture(Piece piece)
    {
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
        UpdateCapturedPieces(_whiteCapturesImage, _whiteCapturedPieces, _board.CapturedPieces[Side.white]);
        UpdateCapturedPieces(_blackCapturesImage, _blackCapturedPieces, _board.CapturedPieces[Side.black]);
    }

    private void UpdateCapturedPieces(Image capturesImage, List<Image> capturedPieces, List<Piece> pieces)
    {
        // Удаляем все текущие отображаемые фигуры
        foreach (var capturedPiece in capturedPieces)
        {
            Destroy(capturedPiece.gameObject);
        }
        capturedPieces.Clear();

        // Создаем новые отображаемые фигуры на основе списка pieces
        int maxSprites = 6; // Максимальное количество спрайтов для каждого цвета
        foreach (var piece in pieces)
        {
            // Получаем спрайт в зависимости от типа фигуры и ее цвета
            Sprite sprite = GetPieceSprite(piece);
            if (sprite != null && capturedPieces.Count < maxSprites)
            {
                // Создаем новую отображаемую фигуру с соответствующим спрайтом
                GameObject capturedPieceObject = new GameObject("CapturedPiece");
                capturedPieceObject.transform.SetParent(capturesImage.transform, false);
                Image capturedPieceImage = capturedPieceObject.AddComponent<Image>();
                capturedPieceImage.sprite = sprite;
                capturedPieceImage.rectTransform.localScale = Vector3.one;
                capturedPieces.Add(capturedPieceImage);
            }
        }
    }

    private Sprite GetPieceSprite(Piece piece)
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
