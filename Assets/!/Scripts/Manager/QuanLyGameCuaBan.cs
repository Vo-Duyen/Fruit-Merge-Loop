using System;
using System.Collections;
using DesignPattern;
using DesignPattern.Observer;
using DG.Tweening;
using LongNC.Cube;
using LongNC.UI;
using LongNC.UI.Data;
using LongNC.UI.Manager;
using UnityEngine;

namespace LongNC.Manager
{
    public class QuanLyGameCuaBan : DuyNhat<QuanLyGameCuaBan>
    {
        private ToiLaAi<SuKienTrongGiaoDien> Observer => ToiLaAi<SuKienTrongGiaoDien>.Instance;

        private int manHienTAi;

        private void OnEnable()
        {
            RegisterObserver();
            manHienTAi = PlayerPrefs.GetInt("CurrentLevel");
            if (manHienTAi <= 0) manHienTAi = 1;
            QuanLyGiaoDien.Instance.UpdateLevel(manHienTAi);
        }

        private void OnDisable()
        {
            UnregisterObserver();
        }
        
        private void Start()
        {
            Application.targetFrameRate = 60;
            QuanLyMan.Instance.LaySoMan();
            QuanLyMan.Instance.LayDuLieuManHienTai();
            QuanLyMan.Instance.TaoMoiVatTrongGame();
            QuanLyDauVao.Instance.SetIsCanControl();
        }

        #region Observers

        private void RegisterObserver()
        {
            Observer.DangKy(SuKienTrongGiaoDien.OnNextLevelButtonClicked, OnNextLevelButtonClicked);
            Observer.DangKy(SuKienTrongGiaoDien.OnRestartButtonClicked, AnNutChoiLai);
            Observer.DangKy(SuKienTrongGiaoDien.OnTryAgainButtonClicked, VanLaAnNutchoiali);
        }

        private void UnregisterObserver()
        {
            Observer.HuyDangKy(SuKienTrongGiaoDien.OnNextLevelButtonClicked, OnNextLevelButtonClicked);
            Observer.HuyDangKy(SuKienTrongGiaoDien.OnRestartButtonClicked, AnNutChoiLai);
            Observer.HuyDangKy(SuKienTrongGiaoDien.OnTryAgainButtonClicked, VanLaAnNutchoiali);
        }

        #endregion

        #region Button Handlers

        private void OnNextLevelButtonClicked(object param)
        {
            QuanLyMan.Instance.ClearCurrentLevel();
            QuanLyMan.Instance.LayDuLieuManTiep();
            DOVirtual.DelayedCall(0.2f, () =>
            {
                QuanLyMan.Instance.TaoMoiVatTrongGame();
                ++manHienTAi;
                QuanLyGiaoDien.Instance.UpdateLevel(manHienTAi);
                PlayerPrefs.SetInt("CurrentLevel", manHienTAi);
            });
        }
        
        private void AnNutChoiLai(object param)
        {
            QuanLyMan.Instance.ClearCurrentLevel();
            DOVirtual.DelayedCall(0.2f, () =>
            {
                QuanLyMan.Instance.TaoMoiVatTrongGame();
            });
        }

        private void VanLaAnNutchoiali(object param)
        {
            QuanLyMan.Instance.ClearCurrentLevel();
            QuanLyMan.Instance.TaoMoiVatTrongGame();
        }
        
        #endregion

        public void TestNextLevel()
        {
            OnNextLevelButtonClicked(null);
        }

        public void ResetLevel()
        {
            PlayerPrefs.SetInt("CurrentLevel", 0);
            AnNutChoiLai(null);
        }
    }
}