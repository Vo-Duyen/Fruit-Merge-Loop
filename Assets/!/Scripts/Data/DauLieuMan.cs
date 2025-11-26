using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

namespace LongNC.Data
{
    [CreateAssetMenu(fileName = "DataLevel0", menuName = "Data/New Data Level")]
    public class DauLieuMan : SerializedScriptableObject
    {
        public int levelId;
        public MapType mapType;
        public List<(int, int)> arrFruitCore = new List<(int, int)>();
        public int cntFruitQueue;
        public List<int> arrFruitQueue = new List<int>();
        public int pointWin;
        public bool isRandom = false;
        public int startPoint = -1;
        public int endPoint = -1;
        public int cntSpawnMax = 0;

        public bool isTip = false;
        public string textTip;
    }

    public enum MapType
    {
        Circle,
        Square,
        HorizontalOval,
        VerticalOval,
        BigSquare,
    }
}