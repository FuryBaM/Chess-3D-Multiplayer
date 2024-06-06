using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoveUIElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _numeration;
    [SerializeField] private TextMeshProUGUI _whiteMove;
    [SerializeField] private TextMeshProUGUI _blackMove;
    private void Start()
    {
        transform.SetParent(GameObject.FindGameObjectWithTag("MoveContent").transform);
    }
    public void SetMoveNumeration(int numeration)
    {
        _numeration.text = numeration.ToString() + ".";
    }
    public void SetWhiteMove(string whiteMove)
    {
        _whiteMove.text = whiteMove;
    }
    public void SetBlackMove(string blackMove)
    {
        _blackMove.text = blackMove;
    }
}
