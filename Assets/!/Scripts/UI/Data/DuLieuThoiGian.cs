using UnityEngine;

namespace LongNC.UI.Data
{
    public class DuLieuThoiGian : DuLieuTrongGiaoDien
    {
        public float CurrentTime { get; set; }
        public float MaxTime { get; set; }
        
        public DuLieuThoiGian(float currentTime, float maxTime)
        {
            CurrentTime = currentTime;
            MaxTime = maxTime;
        }
        
        public string GetTimeString()
        {
            var minutes = Mathf.FloorToInt(CurrentTime / 60);
            var seconds = Mathf.FloorToInt(CurrentTime % 60);
            return $"{minutes:00}:{seconds:00}";
        }
    }
}