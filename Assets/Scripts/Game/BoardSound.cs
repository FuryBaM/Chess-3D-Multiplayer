﻿using Mirror;
using UnityEngine;

public class BoardSound : NetworkBehaviour
{
    [SerializeField] private Board _board;
    [SerializeField] private AudioClip _moveSound;
    [SerializeField] private AudioClip _captureSound;
    [SerializeField] private AudioClip _checkSound;
    [SerializeField] private AudioClip _mateSound;
    [SerializeField] private AudioClip _castleSound;
    [SerializeField] private AudioClip _promoteSound;
    private AudioSource _audioSource;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }
    private void OnEnable()
    {
        _board.OnCapture.AddListener(OnCapture);
        _board.OnCheck.AddListener(OnCheck);
        _board.OnMakeMove.AddListener(OnMove);
        _board.OnMate.AddListener(OnMate);
        _board.OnCastle.AddListener(OnCastle);
        _board.OnStalemate.AddListener(OnMate);
        _board.OnPromotion.AddListener(OnPromotion);
    }
    private void OnDisable()
    {
        _board.OnCapture.RemoveListener(OnCapture);
        _board.OnCheck.RemoveListener(OnCheck);
        _board.OnMakeMove.RemoveListener(OnMove);
        _board.OnMate.RemoveListener(OnMate);
        _board.OnCastle.RemoveListener(OnCastle);
        _board.OnStalemate.RemoveListener(OnMate);
    }
    [ClientCallback]
    private void OnMove()
    {
        _audioSource.PlayOneShot(_moveSound);
    }
    [ClientCallback]
    private void OnCapture(uint pieceId)
    {
        _audioSource.PlayOneShot(_captureSound);
    }
    [ClientCallback]
    private void OnCheck()
    {
        _audioSource.PlayOneShot(_checkSound);
    }
    [ClientCallback]
    private void OnMate()
    {
        _audioSource.PlayOneShot(_mateSound);
    }
    [ClientCallback]
    private void OnCastle()
    {
        _audioSource.PlayOneShot(_castleSound);
    }
    [ClientCallback]
    private void OnPromotion()
    {
        _audioSource.PlayOneShot(_promoteSound);
    }
}
