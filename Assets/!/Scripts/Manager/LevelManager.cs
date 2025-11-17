using System;
using System.Collections;
using System.Collections.Generic;
using DesignPattern;
using DesignPattern.ObjectPool;
using DesignPattern.Observer;
using LongNC.Data;
using LongNC.Items;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace LongNC.Manager
{
    public enum GameEvent
    {
        FruitInPlatform,
        CheckMerge, MergeFruit,
    }
    public class LevelManager : Singleton<LevelManager>
    {
        private class Info
        {
            public IItemIdleBase itemIdle;
            public IItemMovingBase itemMove;
        }
        [Title("Core")]
        [ShowInInspector, ReadOnly] private List<Info> _arr = new List<Info>();
        
        [Title("Prefab object")]
        [SerializeField] private GameObject _backgroundObj;
        [SerializeField] private GameObject _platformQueueObj;
        [SerializeField] private Vector3 _posPlatformQueue;
        
        [Title("Map")]
        [SerializeField] private List<GameObject> _maps;
        
        [SerializeField] private List<GameObject> _fruitObj = new List<GameObject>();
        [SerializeField] private List<Transform> _platformTransforms = new List<Transform>();
        [SerializeField] private List<int> _arrFruit = new List<int>();
        
        [Title("Queue Fruit")]
        [SerializeField] private int _cntFruitQueue;
        [SerializeField, ReadOnly] private Transform _parentPlatformQueue;
        [ShowInInspector, ReadOnly] private List<Queue<(int id, IItemMovingBase item)>> _queueFruits = new List<Queue<(int, IItemMovingBase)>>();
        [SerializeField, ReadOnly] private List<GameObject> _parentQueues = new List<GameObject>();
        [SerializeField] private Vector3 _ofset;
        [SerializeField] private float _distanceFruitInQueue;
        [SerializeField] private Transform parentFruitQueue;
        
        private int _currentLevel;
        private LevelData _dataCurrentLevel;
        private GameObject _currentMap;
        
        private ObserverManager<GameEvent> Observer = ObserverManager<GameEvent>.Instance;
        
        private void OnEnable()
        {
            GetLevel();
            LoadLevel();
            LoadAllObjInLevel();
            Observer.RegisterEvent(GameEvent.CheckMerge, CheckMerge);
        }

        private void OnDisable()
        {
            Observer.RemoveEvent(GameEvent.CheckMerge, CheckMerge);
        }

        private void CheckMerge(object param)
        {
            if (param is IItemIdleBase itemIdle)
            {
                var idItemIdle = -1;
                for (var i = 0; i < _arr.Count; ++i)
                {
                    if (itemIdle == _arr[i].itemIdle)
                    {
                        idItemIdle = i;
                        break;
                    }
                }
                // TODO: Check Merge right/left
                var typeFruit = _arr[idItemIdle].itemMove.GetItemType<Fruits.ItemType>();
                var idRight = idItemIdle + 1;
                var idLeft = idItemIdle - 1;
                var isRight = false;
                var isLeft = false;
                if (idRight >= _arr.Count)
                {
                    idRight = 0;
                }

                if (idLeft < 0)
                {
                    idLeft = _arr.Count - 1;
                }
                // Right
                while (idRight != idItemIdle)
                {
                    if (_arr[idRight].itemIdle.IsState(Platforms.State.HaveFruit))
                    {
                        isRight = _arr[idRight].itemMove.IsType(typeFruit);
                        break;
                    }
                    ++idRight;
                    if (idRight >= _arr.Count)
                    {
                        idRight = 0;
                    }
                }
                
                // Left
                while (idLeft != idItemIdle)
                {
                    if (_arr[idLeft].itemIdle.IsState(Platforms.State.HaveFruit))
                    {
                        isLeft = _arr[idLeft].itemMove.IsType(typeFruit);
                        break;
                    }
                    ++idLeft;
                    if (idLeft >= _arr.Count)
                    {
                        idLeft = 0;
                    }
                }
                
                // TODO: check
            }
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
            SpawnBackground();
            SpawnMap();
            SpawnFruitInMap();
            SpawnQueueAndFruit();
            SetFruitInQueue();
        }

        private void SpawnBackground()
        {
            PoolingManager.Spawn(_backgroundObj, Vector3.zero, Quaternion.identity, transform);
        }

        private void SpawnMap()
        {
            var idMap = (int) _dataCurrentLevel.mapType;
            _currentMap = PoolingManager.Spawn(_maps[idMap], Vector3.forward * 0.4f, Quaternion.identity, transform);
            
            // Setup arr
            _arr.Clear();
            for (var i = 0; i < _currentMap.transform.childCount; ++i)
            {
                var curItemIdle = _currentMap.transform.GetChild(i).GetComponent<IItemIdleBase>();
                _arr.Add(new Info()
                {
                    itemIdle = curItemIdle,
                    itemMove = null,
                    typeFruit = 0,
                });
            }
        }
        
        private void SpawnFruitInMap()
        {
            var arrFruitCore = _dataCurrentLevel.arrFruitCore;
            parentFruitQueue = new GameObject(name: "ParentFruit")
            {
                transform =
                {
                    parent = transform,
                    position = Vector3.zero,
                }
            }.transform;
            
            foreach (var (idPos, typeFruit) in arrFruitCore)
            {
                var itemIdle = _arr[idPos].itemIdle;
                var currentPlatform = itemIdle.GetTransform();
                var posFruit = currentPlatform.position;
                posFruit.z = 0f;
                var currentFruit = PoolingManager.Spawn(_fruitObj[typeFruit - 1], posFruit, Quaternion.identity, parentFruitQueue);
                var itemMove = currentFruit.GetComponent<IItemMovingBase>();
                itemMove.ChangeState(Fruits.State.InPlatform);
                currentFruit.transform.rotation = currentPlatform.rotation;
                
                _arr[idPos].itemIdle.ChangeState(Platforms.State.HaveFruit);
                _arr[idPos].itemMove = itemMove;
            }
        }

        private void SpawnQueueAndFruit()
        {
            _cntFruitQueue = _dataCurrentLevel.cntFruitQueue;

            _parentPlatformQueue = new GameObject(name: "ParentPlatformQueue")
            {
                transform =
                {
                    parent = transform,
                    position = Vector3.zero,
                    rotation = Quaternion.identity,
                }
            }.transform;

            var posPlatformQueues = new List<Vector3>();
            switch (_cntFruitQueue)
            {
                case 1:
                    posPlatformQueues.Add(_posPlatformQueue);
                    break;
                case 2:
                    var pos0 = _posPlatformQueue + Vector3.left * 0.5f;
                    var pos1 = _posPlatformQueue + Vector3.right * 0.5f;
                    posPlatformQueues.Add(pos0);
                    posPlatformQueues.Add(pos1);
                    break;
                case 3:
                    pos0 = _posPlatformQueue + Vector3.left;
                    pos1 = _posPlatformQueue + Vector3.right;
                    posPlatformQueues.Add(pos0);
                    posPlatformQueues.Add(_posPlatformQueue);
                    posPlatformQueues.Add(pos1);
                    break;
            }

            foreach (var posPlatformQueue in posPlatformQueues)
            {
                var currentPlatformQueue = PoolingManager.Spawn(_platformQueueObj, posPlatformQueue, Quaternion.identity, _parentPlatformQueue);
                currentPlatformQueue.GetComponent<IItemIdleBase>().ChangeState(Platforms.State.HaveFruit);
            }
            
            _queueFruits.Clear();
            _parentQueues.Clear();
            // var cntShowFruitQueue = 3 * _cntFruitQueue;
            for (var i = 0; i < _cntFruitQueue; ++i)
            {
                _parentQueues.Add(new GameObject($"Queue_{i}")
                {
                    transform =
                    {
                        parent = transform,
                        position = Vector3.zero,
                        rotation = Quaternion.identity,
                    }
                });
            }

            for (var i = 0; i < _cntFruitQueue; ++i)
            {
                var value = new Queue<(int, IItemMovingBase)>();
                var child = (0, (IItemMovingBase)null);
                var posFruitInQueue = _posPlatformQueue + _ofset;
                for (var j = i; j < _dataCurrentLevel.arrFruit.Count; j += _cntFruitQueue)
                {
                    child.Item1 =  _dataCurrentLevel.arrFruit[j];

                    var nFruit = PoolingManager.Spawn(_fruitObj[child.Item1], posFruitInQueue, Quaternion.identity,
                        _parentQueues[i].transform);
                    posFruitInQueue += Vector3.down * _distanceFruitInQueue;

                    child.Item2 = nFruit.GetComponent<IItemMovingBase>();
                    
                    value.Enqueue(child);
                }
                
                _queueFruits.Add(value);
            }
        }

        private void SetFruitInQueue()
        {
            foreach (var queueFruit in _queueFruits)
            {
                var fruitInQueue = queueFruit.Peek().item;
                fruitInQueue.ChangeState(LongNC.Items.Fruits.State.InQueue);
            }
        }
    }
}