using UnityEngine;
using Mirror;

public class GameUIHandler : MonoBehaviour
{
    [SerializeField] private Animator _rightPanelAnimator;
    [SerializeField] private RectTransform _whiteCapturesImage;
    [SerializeField] private RectTransform _blackCapturesImage;
    [SerializeField] private RectTransform _moveContent;
    private void Start()
    {
        if (_rightPanelAnimator == null)
        {
            _rightPanelAnimator = GetComponent<Animator>();
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            HidePanel();
        }
    }
    public void HidePanel()
    {
        _rightPanelAnimator.SetBool("HidePanel", !_rightPanelAnimator.GetBool("HidePanel"));
    }
}
