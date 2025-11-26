using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using LongNC.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChayGame : MonoBehaviour
{
    public Slider _slider;
    public float _timeLoading;
    public Button _play;

    private void Start()
    {
        QuanLyAmThanhTroChoi.Instance.PlayFX(SoundId.Background, true);
        var loadScreen = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
        if (loadScreen != null) loadScreen.allowSceneActivation = false;

        _slider.gameObject.SetActive(true);
        _play.gameObject.SetActive(false);
        _play.onClick.AddListener(() =>
        {
            if (loadScreen != null) loadScreen.allowSceneActivation = true;
        });
        _slider.DOValue(1f, _timeLoading).OnComplete(delegate
        {
            _slider.gameObject.SetActive(false);
            _play.gameObject.SetActive(true);
        });
    }
}