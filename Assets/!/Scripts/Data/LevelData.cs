using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

namespace LongNC.Data
{
    [CreateAssetMenu(fileName = "DataLevel0", menuName = "Data/New Data Level")]
    public class LevelData : SerializedScriptableObject
    {
        public int levelId;
        public MapType mapType;
        public List<(int, int)> arrFruitCore = new List<(int, int)>();
        public int cntFruitQueue;
        public List<int> arrFruit = new List<int>();
        public int pointWin;
    }

    public enum MapType
    {
        Circle,
        Square,
        HorizontalOval,
        VerticalOval,
    }
}