using System;
using LongNC.UI.Data;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.UI;

namespace LongNC.UI.Panel
{
    public class GiaoDienTroGiup : BangGiaoDienCoBan
    {
        [Title("Close Settings")]
        [OdinSerialize]
        private Button _closeButton;
        [OdinSerialize]
        private Button _thankButton;

        private void Awake()
        {
            SetupButtons();
        }

        private void SetupButtons()
        {
            _closeButton?.onClick.AddListener(OnCloseButtonClicked);
            _thankButton?.onClick.AddListener(OnCloseButtonClicked);
        }

        private void OnCloseButtonClicked()
        {
            Observer.PhatSuKien(SuKienTrongGiaoDien.OnCloseHelpClicked, _closeButton);
        }
    }
}