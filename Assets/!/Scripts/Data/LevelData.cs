using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace LongNC.Data
{
    [CreateAssetMenu(fileName = "DataLevel0", menuName = "Data/New Data Level")]
    public class LevelData : SerializedScriptableObject
    {
        public int levelId;
        public int cntPlatforms;
        public int cntFruitPlatforms;
        public List<int> arrFruit = new List<int>();
    }
}