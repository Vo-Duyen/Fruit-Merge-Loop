using System;
using System.Collections;
using System.Collections.Generic;
using DesignPattern;
using DesignPattern.ObjectPool;
using DesignPattern.Observer;
using DG.Tweening;
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
        CheckMerge, AnimMerge,
        NextFruitInQueue,
    }
    public class Info
    {
        public IItemIdleBase itemIdle;
        public IItemMovingBase itemMove;
    }
    public class LevelManager : Singleton<LevelManager>
    {
        [Title("Core")]
        [ShowInInspector, ReadOnly] private List<Info> _arr = new List<Info>();
        [SerializeField] private List<GameObject> _fruitObj = new List<GameObject>();
        [SerializeField, ReadOnly] private Transform _currentLevelTrans;
        [SerializeField, ReadOnly] private Transform _parentFruitInMap;
        [SerializeField, ReadOnly] private Transform _parentFruitInQueue;
        [SerializeField, ReadOnly] private Transform _parentPlatformQueue;
        
        [Title("Prefab object")]
        [SerializeField] private GameObject _backgroundObj;
        [SerializeField] private GameObject _platformQueueObj;
        [SerializeField] private Vector3 _posPlatformQueue;
        
        [Title("Map")]
        [SerializeField] private List<GameObject> _maps;
        
        [Title("Queue Fruit")]
        [SerializeField] private int _cntFruitQueue;
        [ShowInInspector, ReadOnly] private List<Queue<IItemMovingBase>> _queueFruits = new List<Queue<IItemMovingBase>>();
        [SerializeField, ReadOnly] private List<GameObject> _parentQueues = new List<GameObject>();
        [SerializeField] private Vector3 _ofset;
        [SerializeField] private float _distanceFruitInQueue;
        
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
            Observer.RegisterEvent(GameEvent.FruitInPlatform, FruitInPlatform);
            Observer.RegisterEvent(GameEvent.NextFruitInQueue, NextFruitInQueue);
        }

        private void OnDisable()
        {
            Observer.RemoveEvent(GameEvent.CheckMerge, CheckMerge);
            Observer.RemoveEvent(GameEvent.FruitInPlatform, FruitInPlatform);
            Observer.RemoveEvent(GameEvent.NextFruitInQueue, NextFruitInQueue);
        }
        
        private void FruitInPlatform(object param)
        {
            if (param is (IItemIdleBase itemIdleBase, IItemMovingBase itemMovingBase))
            {
                for (var i = 0; i < _arr.Count; ++i)
                {
                    if (_arr[i].itemIdle == itemIdleBase || _arr[i].itemMove == itemMovingBase)
                    {
                        _arr[i].itemIdle = itemIdleBase;
                        _arr[i].itemMove = itemMovingBase;
                    }
                }
            }
        }

        private void CheckMerge(object param)
        {
            if (param is IItemMovingBase itemMove)
            {
                var idItemMove = -1;
                for (var i = 0; i < _arr.Count; ++i)
                {
                    if (itemMove == _arr[i].itemMove)
                    {
                        idItemMove = i;
                        break;
                    }
                }

                if (idItemMove == -1)
                {
                    Debug.Log("Ga");
                    return;
                }
                // TODO: Check Merge right/left
                var typeFruit = _arr[idItemMove].itemMove.GetItemType<Fruits.ItemType>();
                var idRight = idItemMove + 1;
                var idLeft = idItemMove - 1;
                var isRight = false;
                var isLeft = false;
                var cntRight = 0;
                var cntLeft = 0;
                if (idRight >= _arr.Count)
                {
                    idRight = 0;
                }

                if (idLeft < 0)
                {
                    idLeft = _arr.Count - 1;
                }
                // Right
                while (idRight != idItemMove)
                {
                    ++cntRight;
                    if (_arr[idRight].itemIdle.IsState(Platforms.State.HaveFruit))
                    {
                        isRight = _arr[idRight].itemMove.IsType(typeFruit);
                        break;
                    }
                    ++ idRight;
                    if (idRight >= _arr.Count)
                    {
                        idRight = 0;
                    }
                }
                
                // Left
                while (idLeft != idItemMove)
                {
                    ++cntLeft;
                    if (_arr[idLeft].itemIdle.IsState(Platforms.State.HaveFruit))
                    {
                        isLeft = _arr[idLeft].itemMove.IsType(typeFruit);
                        break;
                    }
                    -- idLeft;
                    if (idLeft < 0)
                    {
                        idLeft = _arr.Count - 1;
                    }
                }
                
                // TODO: check
                if (isRight && isLeft)
                {
                    if (cntRight < cntLeft)
                    {
                        Merges(idItemMove, cntRight);
                    }
                    else
                    {
                        Merges(idItemMove, cntLeft * -1);
                    }
                }
                else if (isRight)
                {
                    Merges(idItemMove, cntRight);
                }
                else if (isLeft)
                {
                    Merges(idItemMove, cntLeft * -1);
                }
                else
                {
                    // Not merge
                    SetFruitInQueue();
                }
            }
        }

        private void Merges(int idArr, int cntMerge)
        {
            // Debug.Log(cntMerge);
            var sequence = DOTween.Sequence();
            var curId = idArr;
            for (var i = 0; i < Mathf.Abs(cntMerge); ++i)
            {
                if (i > 0)
                {
                    sequence.AppendInterval(0.2f);
                }

                int nextIdCalc = cntMerge > 0 ? curId + 1 : curId - 1;
                if (nextIdCalc < 0)
                {
                    nextIdCalc = _arr.Count - 1;
                }
                else if (nextIdCalc >= _arr.Count)
                {
                    nextIdCalc = 0;
                }

                var finalCurId = curId;
                var finalNextId = nextIdCalc;
                sequence.AppendCallback(() => Merge(finalCurId, finalNextId));

                curId = nextIdCalc;
            }

            sequence.Play();
        }

        private void Merge(int idArr, int nextId)
        {
            Observer.PostEvent(GameEvent.AnimMerge, (_arr[idArr], _arr[nextId].itemIdle));
            if (_arr[nextId].itemMove != null)
            {
                StartCoroutine(IEMerge(nextId, _arr[idArr].itemMove, _arr[nextId].itemMove));
            }
            _arr[idArr].itemIdle.ChangeState(Platforms.State.Idle);
            _arr[nextId].itemIdle.ChangeState(Platforms.State.HaveFruit);
            _arr[nextId].itemMove = _arr[idArr].itemMove;
            _arr[idArr].itemMove = null;
        }

        private IEnumerator IEMerge(int curId, IItemMovingBase currentItem, IItemMovingBase nextItem)
        {
            yield return WaitForSecondCache.Get(0.2f);
            
            currentItem.ChangeState(Fruits.State.Done);
            nextItem.ChangeState(Fruits.State.Done);
            
            yield return WaitForSecondCache.Get(0.2f);
            
            var nextItemType = (int) currentItem.GetItemType<Fruits.ItemType>() + 1;

            if (nextItemType >= _fruitObj.Count)
            {
                // Debug.Log("Max fruit");
                nextItemType = 0;
            }
            
            var targetTrans = _arr[curId].itemIdle.GetTransform();
            var targetPos = targetTrans.position;
            targetPos.z = 0;
            var targetRot = targetTrans.rotation;
            var newFruit = PoolingManager.Spawn(_fruitObj[nextItemType], targetPos, targetRot, _parentFruitInMap);

            var newItemMove = newFruit.GetComponent<IItemMovingBase>();
            newItemMove.ChangeState(Fruits.State.InPlatform);
            _arr[curId].itemMove = newItemMove;
            
            PoolingManager.Despawn(currentItem.GetTransform().gameObject);
            PoolingManager.Despawn(nextItem.GetTransform().gameObject);
            
            yield return WaitForSecondCache.Get(0.2f);
            
            CheckMerge(newItemMove);
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
            _dataCurrentLevel = Resources.Load<LevelData>($"LevelData/DataLevel_{_currentLevel}");
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
            _currentLevelTrans = new GameObject($"Level{_currentLevel}")
            {
                transform =
                {
                    parent = transform,
                }
            }.transform;
            SpawnBackground();
            SpawnMap();
            SpawnFruitInMap();
            SpawnQueueAndFruit();
            SetFruitInQueue();
        }

        private void SpawnBackground()
        {
            PoolingManager.Spawn(_backgroundObj, Vector3.zero, Quaternion.identity, _currentLevelTrans);
        }

        private void SpawnMap()
        {
            var idMap = (int) _dataCurrentLevel.mapType;
            _currentMap = PoolingManager.Spawn(_maps[idMap], Vector3.forward * 0.4f, Quaternion.identity, _currentLevelTrans);
            
            // Setup arr
            _arr.Clear();
            for (var i = 0; i < _currentMap.transform.childCount; ++i)
            {
                var curItemIdle = _currentMap.transform.GetChild(i).GetComponent<IItemIdleBase>();
                _arr.Add(new Info()
                {
                    itemIdle = curItemIdle,
                    itemMove = null,
                });
            }
        }
        
        private void SpawnFruitInMap()
        {
            var arrFruitCore = _dataCurrentLevel.arrFruitCore;
            _parentFruitInMap = new GameObject(name: "ParentFruitInMap")
            {
                transform =
                {
                    parent = _currentLevelTrans,
                }
            }.transform;
            
            foreach (var (idPos, typeFruit) in arrFruitCore)
            {
                var itemIdle = _arr[idPos].itemIdle;
                var currentPlatform = itemIdle.GetTransform();
                var posFruit = currentPlatform.position;
                posFruit.z = 0f;
                var currentFruit = PoolingManager.Spawn(_fruitObj[typeFruit - 1], posFruit, Quaternion.identity, _parentFruitInMap);
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
                var value = new Queue<IItemMovingBase>();
                var posFruitInQueue = _posPlatformQueue + _ofset;
                for (var j = i; j < _dataCurrentLevel.arrFruitQueue.Count; j += _cntFruitQueue)
                {
                    var nFruit = PoolingManager.Spawn(_fruitObj[_dataCurrentLevel.arrFruitQueue[j]], posFruitInQueue, Quaternion.identity,
                        _parentQueues[i].transform);
                    posFruitInQueue += Vector3.down * _distanceFruitInQueue;

                    value.Enqueue(nFruit.GetComponent<IItemMovingBase>());
                }
                
                _queueFruits.Add(value);
            }
        }

        private void SetFruitInQueue(bool value = true)
        {
            foreach (var queueFruit in _queueFruits)
            {
                var fruitInQueue = queueFruit.Peek();
                fruitInQueue.ChangeState(value ? Fruits.State.InQueue : Fruits.State.Ban);
            }
        }

        private void NextFruitInQueue(object param)
        {
            if (param is IItemMovingBase currentFruit)
            {
                foreach (var queueFruit in _queueFruits)
                {
                    var fruitInQueue = queueFruit.Peek();
                    if (fruitInQueue == currentFruit)
                    {
                        queueFruit.Dequeue();
                        SetFruitInQueue(false);
                        var queueMove = new Queue<IItemMovingBase>(queueFruit);
                        while (queueMove.Count > 0)
                        {
                            var item = queueMove.Dequeue();
                            item.GetTransform().DOMoveY(item.GetTransform().position.y + 1f, 0.2f);
                        }
                        break;
                    }
                }
            }
        }
    }
}