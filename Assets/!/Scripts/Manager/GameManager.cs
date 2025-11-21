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
    public class GameManager : Singleton<GameManager>
    {
        private ObserverManager<UIEventID> Observer => ObserverManager<UIEventID>.Instance;

        private int _curLevel;

        private void OnEnable()
        {
            RegisterObserver();
            _curLevel = PlayerPrefs.GetInt("CurrentLevel");
            UIManager.Instance.UpdateLevel(_curLevel);
        }

        private void OnDisable()
        {
            UnregisterObserver();
        }
        
        private void Start()
        {
            Application.targetFrameRate = 60;
            LevelManager.Instance.GetLevel();
            LevelManager.Instance.LoadLevel();
            LevelManager.Instance.LoadAllObjInLevel();
            InputManager.Instance.SetIsCanControl();
        }

        #region Observers

        private void RegisterObserver()
        {
            Observer.RegisterEvent(UIEventID.OnNextLevelButtonClicked, OnNextLevelButtonClicked);
            Observer.RegisterEvent(UIEventID.OnRestartButtonClicked, OnRestartButtonClicked);
            Observer.RegisterEvent(UIEventID.OnTryAgainButtonClicked, OnTryAgainButtonClicked);
        }

        private void UnregisterObserver()
        {
            Observer.RemoveEvent(UIEventID.OnNextLevelButtonClicked, OnNextLevelButtonClicked);
            Observer.RemoveEvent(UIEventID.OnRestartButtonClicked, OnRestartButtonClicked);
            Observer.RemoveEvent(UIEventID.OnTryAgainButtonClicked, OnTryAgainButtonClicked);
        }

        #endregion

        #region Button Handlers

        private void OnNextLevelButtonClicked(object param)
        {
            LevelManager.Instance.ClearCurrentLevel();
            LevelManager.Instance.LoadNextLevel();
            DOVirtual.DelayedCall(0.2f, () =>
            {
                LevelManager.Instance.LoadAllObjInLevel();
                ++_curLevel;
                UIManager.Instance.UpdateLevel(_curLevel);
                PlayerPrefs.SetInt("CurrentLevel", _curLevel);
            });
        }
        
        private void OnRestartButtonClicked(object param)
        {
            LevelManager.Instance.ClearCurrentLevel();
            DOVirtual.DelayedCall(0.2f, () =>
            {
                LevelManager.Instance.LoadAllObjInLevel();
            });
        }

        private void OnTryAgainButtonClicked(object param)
        {
            LevelManager.Instance.ClearCurrentLevel();
            LevelManager.Instance.LoadAllObjInLevel();
        }
        
        #endregion

        public void TestNextLevel()
        {
            OnNextLevelButtonClicked(null);
        }

        public void ResetLevel()
        {
            PlayerPrefs.SetInt("CurrentLevel", 0);
            OnRestartButtonClicked(null);
        }
    }
}