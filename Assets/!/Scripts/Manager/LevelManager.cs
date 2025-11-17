using System;
using DesignPattern;
using LongNC.Data;
using UnityEngine;

namespace LongNC.Manager
{
    public class LevelManager : Singleton<LevelManager>
    {
        private int _currentLevel;
        private LevelData _dataCurrentLevel;
        
        private void OnEnable()
        {
            GetLevel();
            LoadLevel();
            LoadAllObjInLevel();
        }

        private void GetLevel()
        {
            var currentLevel = PlayerPrefs.GetInt("CurrentLevel");
            if (currentLevel < 1)
            {
                currentLevel = 1;
                PlayerPrefs.SetInt("CurrentLevel", currentLevel);
            }

            _currentLevel = currentLevel;
        }

        private void LoadLevel()
        {
            _dataCurrentLevel = Resources.Load<LevelData>($"LevelData/DataLevel{_currentLevel}");
        }

        private void LoadNextLevel()
        {
            ++_currentLevel;
            if (_currentLevel > 20)
            {
                _currentLevel = 1;
            }
            LoadLevel();
        }

        private void LoadAllObjInLevel()
        {
            
        }
    }
}