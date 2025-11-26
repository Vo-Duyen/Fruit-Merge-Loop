using System;
using DesignPattern;
using DesignPattern.Observer;
using DG.Tweening;
using LongNC.Cube;
using LongNC.Manager;
using LongNC.UI.Data;
using LongNC.UI.Panel;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace LongNC.UI.Manager
{
    public class QuanLyGiaoDien : DuyNhat<QuanLyGiaoDien>
    {
        [FormerlySerializedAs("_gameplayUI")]
        [Title("UI Panels")]
        [SerializeField] private GiaoDienTroChoi giaoDienTroChoi;
        
        [FormerlySerializedAs("_restartUI")] [SerializeField] private GiaoDienChoiLai giaoDienChoiLai;
        [FormerlySerializedAs("_helpUI")] [SerializeField] private GiaoDienTroGiup giaoDienTroGiup;
        [FormerlySerializedAs("_settingUI")] [SerializeField] private GiaoDienCaiDat giaoDienCaiDat;

        [FormerlySerializedAs("_winUI")] [SerializeField] private GiaoDienChienThang giaoDienChienThang;
        [FormerlySerializedAs("_loseUI")] [SerializeField] private GiaoDienThua giaoDienThua;
        
        private float _currentTimeScale;
        
        private ToiLaAi<SuKienTrongGiaoDien> Observer => ToiLaAi<SuKienTrongGiaoDien>.Instance;
        
        private void OnEnable()
        {
            RegisterEvents();
            // ShowStartScreen();
        }

        private void OnDisable()
        {
            UnregisterEvents();
        }
        
        private void RegisterEvents()
        {
            Observer.DangKy(SuKienTrongGiaoDien.OnRestartClicked, OnRestartClicked);
            Observer.DangKy(SuKienTrongGiaoDien.OnCloseRestartClicked, OnCloseRestartClicked);
            Observer.DangKy(SuKienTrongGiaoDien.OnRestartButtonClicked, OnRestartButtonClicked);
            
            Observer.DangKy(SuKienTrongGiaoDien.OnHelpClicked, OnHelpClicked);
            Observer.DangKy(SuKienTrongGiaoDien.OnCloseHelpClicked, OnCloseHelpClicked);
            
            Observer.DangKy(SuKienTrongGiaoDien.OnSettingClicked, OnSettingClicked);
            Observer.DangKy(SuKienTrongGiaoDien.OnCloseSettingClicked, OnCloseSettingClicked);
            
            Observer.DangKy(SuKienTrongGiaoDien.OnStartGame, OnStartGame);
            Observer.DangKy(SuKienTrongGiaoDien.OnWinGame, OnWinGame);
            Observer.DangKy(SuKienTrongGiaoDien.OnLoseGame, OnLoseGame);
            
            Observer.DangKy(SuKienTrongGiaoDien.OnNextLevelButtonClicked, OnNextLevelButtonClicked);
            Observer.DangKy(SuKienTrongGiaoDien.OnTryAgainButtonClicked, OnTryAgainButtonClicked);
        }
        
        private void UnregisterEvents()
        {
            Observer.HuyDangKy(SuKienTrongGiaoDien.OnRestartClicked, OnRestartClicked);
            Observer.HuyDangKy(SuKienTrongGiaoDien.OnCloseRestartClicked, OnCloseRestartClicked);
            Observer.HuyDangKy(SuKienTrongGiaoDien.OnRestartButtonClicked, OnRestartButtonClicked);
            
            Observer.HuyDangKy(SuKienTrongGiaoDien.OnHelpClicked, OnHelpClicked);
            Observer.HuyDangKy(SuKienTrongGiaoDien.OnCloseHelpClicked, OnCloseHelpClicked);
            
            Observer.HuyDangKy(SuKienTrongGiaoDien.OnSettingClicked, OnSettingClicked);
            Observer.HuyDangKy(SuKienTrongGiaoDien.OnCloseSettingClicked, OnCloseSettingClicked);
            
            Observer.HuyDangKy(SuKienTrongGiaoDien.OnStartGame, OnStartGame);
            Observer.HuyDangKy(SuKienTrongGiaoDien.OnWinGame, OnWinGame);
            Observer.HuyDangKy(SuKienTrongGiaoDien.OnLoseGame, OnLoseGame);
            
            Observer.HuyDangKy(SuKienTrongGiaoDien.OnNextLevelButtonClicked, OnNextLevelButtonClicked);
            Observer.HuyDangKy(SuKienTrongGiaoDien.OnTryAgainButtonClicked, OnTryAgainButtonClicked);
        }
        
        #region Show Screens
        
        private void ShowStartScreen()
        {
            HideAllPanels();
        }
        
        private void ShowGameplayScreen()
        {
            HideAllPanels();
            giaoDienTroChoi?.Show();
        }
        
        private void HideAllPanels()
        {
            giaoDienTroChoi?.Hide(true);
            giaoDienChoiLai?.Hide(true);
            giaoDienChienThang?.Hide(true);
            giaoDienThua?.Hide(true);
        }
        
        #endregion

        public void UpdateLevel(int level)
        {
            giaoDienTroChoi?.UpdateLevel(level);
        }

        public void UpdateScore(int score)
        {
            giaoDienTroChoi?.UpdateScore(score);
        }

        public void OnRestartTimer(object param)
        {
            giaoDienTroChoi.OnRestartTimeSlider(param);
        }
        
        #region Event Handlers

        private void OnRestartClicked(object param)
        {
            ButtonScaleAnim(param);
            SetContinueGame(false);
            giaoDienTroChoi?.SetControl(false);
            giaoDienChoiLai?.Show();
        }

        private void OnCloseRestartClicked(object param)
        {
            ButtonScaleAnim(param);
            SetContinueGame(true, 0.3f);
            giaoDienTroChoi?.SetControl();
            giaoDienChoiLai?.Hide();
        }

        private void OnRestartButtonClicked(object param)
        {
            ButtonScaleAnim(param);
            SetContinueGame(true, 0.3f);
            giaoDienTroChoi?.SetControl();
            giaoDienChoiLai?.Hide();
        }
        
        private void OnHelpClicked(object param)
        {
            ButtonScaleAnim(param);
            SetContinueGame(false);
            giaoDienTroChoi?.SetControl(false);
            giaoDienTroGiup?.Show();
        }

        private void OnCloseHelpClicked(object param)
        {
            ButtonScaleAnim(param);
            SetContinueGame(true, 0.3f);
            giaoDienTroChoi?.SetControl();
            giaoDienTroGiup?.Hide();
        }
        
        private void OnSettingClicked(object param)
        {
            ButtonScaleAnim(param);
            SetContinueGame(false);
            giaoDienTroChoi?.SetControl(false);
            giaoDienCaiDat?.Show();
        }

        private void OnCloseSettingClicked(object param)
        {
            ButtonScaleAnim(param);
            SetContinueGame(true, 0.3f);
            giaoDienTroChoi?.SetControl();
            giaoDienCaiDat?.Hide();
        }
        
        private void OnStartGame(object param)
        {
            ShowGameplayScreen();
        }
        
        private void OnWinGame(object param)
        {
            giaoDienTroChoi?.SetControl(false);
            if (param is float timeDelay)
            {
                DOVirtual.DelayedCall(timeDelay, () =>
                {
                    SetContinueGame(false);
                    giaoDienChienThang.Show();
                    QuanLyAmThanhTroChoi.Instance.PlayFX(SoundId.Win);
                }).SetUpdate(true);
            }
            else
            {
                SetContinueGame(false);
                giaoDienChienThang?.Show();
                QuanLyAmThanhTroChoi.Instance.PlayFX(SoundId.Win);
            }
        }
        
        private void OnLoseGame(object param)
        {
            giaoDienTroChoi?.SetControl(false);
            if (param is float timeDelay)
            {
                DOVirtual.DelayedCall(timeDelay, () =>
                {
                    SetContinueGame(false);
                    giaoDienThua?.Show();
                    QuanLyAmThanhTroChoi.Instance.PlayFX(SoundId.Lose);
                }).SetUpdate(true);
            }
            else
            {
                SetContinueGame(false);
                giaoDienThua?.Show();
                QuanLyAmThanhTroChoi.Instance.PlayFX(SoundId.Lose);
            }
        }

        private void OnNextLevelButtonClicked(object param)
        {
            ButtonScaleAnim(param);
            SetContinueGame(true, 0.3f);
            giaoDienTroChoi?.SetControl();
            giaoDienChienThang?.Hide();
        }

        private void OnTryAgainButtonClicked(object param)
        {
            ButtonScaleAnim(param);
            SetContinueGame(true, 0.3f);
            giaoDienTroChoi?.SetControl();
            giaoDienThua?.Hide();
        }
        
        #endregion

        private void SetContinueGame(bool value = true, float timeDelay = 0f)
        {
            if (value)
            {
                QuanLyDauVao.Instance.SetIsCanControl(true, timeDelay);
                Time.timeScale = _currentTimeScale;
            }
            else
            {
                QuanLyDauVao.Instance.SetIsCanControl(false, timeDelay);
                _currentTimeScale = Time.timeScale;
                Time.timeScale = 0;
            }
        }

        private void ButtonScaleAnim(object param)
        {
            if (param is Transform target)
            {
                var scale = target.localScale;
                target.DOScale(scale * 1.2f, 0.1f).SetEase(Ease.Linear).SetLoops(2, LoopType.Yoyo).SetUpdate(true);
            }
        }

        public void ShowTip(bool isShow, string tip)
        {
            giaoDienTroChoi?.ShowTip(isShow, tip);
        }
    }
}