using DesignPattern.Observer;
using DG.Tweening;
using LongNC.Data;
using LongNC.UI.Data;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LongNC.UI.Panel
{
     public class GiaoDienChienThang : BangGiaoDienCoBan
    {
        [Title("Display")]
        [OdinSerialize] 
        private TextMeshProUGUI _titleText;
        [OdinSerialize] 
        private TextMeshProUGUI _levelText;
        
        [Title("Buttons")]
        [OdinSerialize] 
        private Button _nextLevelButton;
        
        
        private void Awake()
        {
            SetupButtons();
        }

        #region SetupButtons

        private void SetupButtons()
        {
            _nextLevelButton?.onClick.AddListener(OnNextLevelClicked);
        }
        
        private void OnNextLevelClicked()
        {
            Observer.PhatSuKien(SuKienTrongGiaoDien.OnNextLevelButtonClicked, _nextLevelButton.transform);
        }
        
        #endregion
    }
}