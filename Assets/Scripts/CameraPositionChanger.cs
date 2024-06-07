using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class CameraPositionChanger : MonoBehaviour
{
    [SerializeField] private Board _board;
    [SerializeField] private Transform _whiteCameraPosition;
    [SerializeField] private Transform _blackCameraPosition;
    [SerializeField] private RectTransform[] _textsToRotate;
    private Coroutine _currentCoroutine;

    private void Update()
    {
        if (_board != null)
        {
            transform.LookAt(_board.transform.position);
        }
    }

    public void SetSide(Side newSide)
    {
        float rotationValue = newSide == Side.white ? 0 : 180;
        foreach(var text in _textsToRotate)
        {
            Vector3 currentRotation = text.transform.rotation.eulerAngles;
            currentRotation.z = rotationValue;
            text.transform.rotation = Quaternion.Euler(currentRotation);
            if (newSide == Side.black)
                text.GetComponent<TextMeshProUGUI>().text = string.Concat(Enumerable.Reverse(text.GetComponent<TextMeshProUGUI>().text));
        }
        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
        }
        Vector3 targetPosition = newSide == Side.white ? _whiteCameraPosition.position : _blackCameraPosition.position;
        _currentCoroutine = StartCoroutine(SetPosition(targetPosition));
    }

    private IEnumerator SetPosition(Vector3 newPosition, float duration = 1f)
    {
        Vector3 startingPos = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startingPos, newPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        transform.position = newPosition;
    }
}
