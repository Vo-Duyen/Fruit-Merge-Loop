using DesignPattern.Observer;
using DG.Tweening;
using LongNC.UI.Data;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LongNC.UI.Panel
{
    public class GiaoDienThua : BangGiaoDienCoBan
    {
        [Title("Button")]
        [OdinSerialize]
        private Button _tryAgainButton;
        
        private void Awake()
        {
            SetupButtons();
        }
        
        private void SetupButtons()
        {
            _tryAgainButton?.onClick.AddListener(OnTryAgainButtonClicked);
        }
        
        private void OnTryAgainButtonClicked()
        {
            Observer.PhatSuKien(SuKienTrongGiaoDien.OnTryAgainButtonClicked, _tryAgainButton.transform);
        }
    }
}