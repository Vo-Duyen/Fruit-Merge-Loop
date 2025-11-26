using System;
using System.Globalization;
using DesignPattern.Observer;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using LongNC.UI.Data;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace LongNC.UI.Panel
{
    public class GiaoDienTroChoi : BangGiaoDienCoBan
    {
        [Title("Info Display")] [OdinSerialize]
        private TextMeshProUGUI _levelText;

        [OdinSerialize] private Slider _timeSlider;

        [Title("Buttons")] [OdinSerialize] private Button _restartButton;
        [OdinSerialize] private Button _helpButton;
        [OdinSerialize] private Button _settingButton;

        [Title("Text")] [OdinSerialize] private TextMeshProUGUI _tipText;
        [OdinSerialize] private TextMeshProUGUI _scoreText;

        [Title("Tutorial")] [OdinSerialize] private Transform _hand;
        [OdinSerialize] private Transform _posHandStart;
        [OdinSerialize] private Transform _posHandEnd;


        private Tween _tween;
        private Tween _timerTween;
        private Tween _scoreTween;
        private Tween _tipTween;
        private Tween _handTween;

        private void Awake()
        {
            SetupButtons();
        }

        public void OnRestartTimeSlider(object param)
        {
            if (param is float time)
            {
                _timeSlider.value = _timeSlider.maxValue;
                _tween?.Kill();
                _tween = DOVirtual.Float(0, time, time, t => { _timeSlider.value = (time - t) / time; })
                    .SetEase(Ease.Linear).OnComplete(() => { Observer.PhatSuKien(SuKienTrongGiaoDien.OnLoseGame); });
            }
        }

        private void SetupButtons()
        {
            _restartButton?.onClick.AddListener(OnRestartClicked);
            _helpButton?.onClick.AddListener(OnHelpClicked);
            _settingButton?.onClick.AddListener(OnSettingClicked);
        }

        #region Button Handlers

        private void OnRestartClicked()
        {
            Observer.PhatSuKien(SuKienTrongGiaoDien.OnRestartClicked, _restartButton.transform);
        }

        private void OnHelpClicked()
        {
            Observer.PhatSuKien(SuKienTrongGiaoDien.OnHelpClicked, _helpButton.transform);
        }

        private void OnSettingClicked()
        {
            Observer.PhatSuKien(SuKienTrongGiaoDien.OnSettingClicked, _settingButton.transform);
        }

        #endregion

        public void UpdateLevel(int level)
        {
            _levelText.text = $"Level {level}";
            _timerTween?.Kill();
            _timerTween = _levelText.transform.DOPunchScale(Vector3.one * 0.5f, 0.5f);
        }

        public void UpdateScore(int score)
        {
            _scoreText.text = $"Target\n{score}";
            _scoreTween?.Kill();
            _scoreTween = _scoreText.transform.DOPunchScale(Vector3.one * 0.5f, 0.5f);
        }

        public void ShowTip(bool isShow, string tip)
        {
            _handTween?.Kill();
            _tipTween?.Kill();
            _hand?.gameObject.SetActive(isShow);
            _tipText.text = tip;
            _tipText.gameObject.SetActive(isShow);
            if (isShow)
            {
                _tipTween = _tipText.transform.DOScale(Vector3.one * 0.9f, 0.5f).SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Yoyo);
                _hand!.position = _posHandStart.position;
                _handTween = _hand.DOMove(_posHandEnd.position, 2f)
                    .SetEase(Ease.Linear)
                    .OnStepComplete(() => { _hand.position = _posHandStart.position; })
                    .SetLoops(-1, LoopType.Restart);
            }
        }
    }
}