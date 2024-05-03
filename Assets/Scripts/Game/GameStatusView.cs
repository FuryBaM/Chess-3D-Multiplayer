using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameStatusView : MonoBehaviour
{
    [SerializeField] private Board _board;
    [SerializeField] private MoveUIElement _moveElementPrefab;
    [SerializeField] private TextMeshProUGUI _whiteCaptures;
    [SerializeField] private TextMeshProUGUI _blackCaptures;
    [SerializeField] private Transform _moveContent;
    private List<MoveUIElement> _currentMoveElements = new List<MoveUIElement>();

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
            _currentMoveElement = Instantiate(_moveElementPrefab, Vector3.zero, Quaternion.identity, _moveContent);
            _currentMoveElements.Add(_currentMoveElement);
        }
        else
        {
            _currentMoveElement = _currentMoveElements.Last();
        }
        
        _currentMoveElement.SetMoveNumeration(_board.CurrentMove/2);
        if (1 - _board.Player == 0)
        {
            _currentMoveElement.SetWhiteMove(MoveConverter.ConvertMoveToString(_board.GetLastMove()));
        }
        else{
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
        // Обновляем отображение захватов
        _whiteCaptures.text = $"White Captures: {Mathf.Max(_whiteScore - _blackScore, 0)}";
        _blackCaptures.text = $"Black Captures: {Mathf.Max(_blackScore - _whiteScore, 0)}";
    }
}
