using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapturedPieceImage : MonoBehaviour
{
    [SerializeField] private Side side;
    private void Start()
    {
        if (side == Side.white)
        {
            transform.SetParent(GameObject.FindGameObjectWithTag("WhitePieceContent").transform);
        }
        else
        {
            transform.SetParent(GameObject.FindGameObjectWithTag("BlackPieceContent").transform);
        }
    }
}
