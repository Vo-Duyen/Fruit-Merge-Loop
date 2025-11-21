using System;
using System.Collections;
using System.Collections.Generic;
using DesignPattern;
using DesignPattern.ObjectPool;
using DesignPattern.Observer;
using DG.Tweening;
using LongNC.Data;
using LongNC.Items;
using LongNC.UI.Data;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

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

    public class InfoFruitInQueue
    {
        public int idFruit;
        public IItemMovingBase itemMove;
    }
    public class LevelManager : Singleton<LevelManager>
    {
        [Title("Core")]
        [ShowInInspector, ReadOnly] private int _curPoint;
        [ShowInInspector, ReadOnly] private List<Info> _arr = new List<Info>();
        [SerializeField] private List<GameObject> _fruitObj = new List<GameObject>();
        [SerializeField, ReadOnly] private Transform _parentMap;
        [SerializeField, ReadOnly] private Transform _parentBackground;
        [SerializeField, ReadOnly] private Transform _currentLevelTrans;
        [SerializeField, ReadOnly] private Transform _parentFruitInMap;
        [SerializeField, ReadOnly] private Transform _parentFruitInQueue;
        [SerializeField, ReadOnly] private Transform _parentPlatformQueue;
        [SerializeField] private int _cntFruitShowInQueue = 10;
        
        [Title("Prefab object")]
        [SerializeField] private GameObject _backgroundObj;
        [SerializeField] private GameObject _platformQueueObj;
        [SerializeField] private Vector3 _posPlatformQueue;
        
        [Title("Map")]
        [SerializeField] private List<GameObject> _maps;
        
        [Title("Queue Fruit")]
        [SerializeField] private int _cntFruitQueue;
        [ShowInInspector, ReadOnly] private List<List<InfoFruitInQueue>> _queueFruits = new List<List<InfoFruitInQueue>>();
        [SerializeField, ReadOnly] private List<GameObject> _parentQueues = new List<GameObject>();
        [SerializeField] private Vector3 _ofset;
        [SerializeField] private float _distanceFruitInQueue;
        
        private int _currentLevel;
        private LevelData _dataCurrentLevel;
        private GameObject _currentMap;
        private Dictionary<int, int> _checkSpawnFruit = new Dictionary<int, int>();
        private bool _isWinGame;
        
        private ObserverManager<GameEvent> Observer = ObserverManager<GameEvent>.Instance;
        
        private void OnEnable()
        {
            // GetLevel();
            // LoadLevel();
            // LoadAllObjInLevel();
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
                if (_isWinGame) return; 
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
                    
                    // Check Lose
                    var isLose = true;
                    foreach (var info in _arr)
                    {
                        if (info.itemMove == null)
                        {
                            isLose = false;
                            break;
                        }
                    }

                    if (isLose)
                    {
                        ObserverManager<UIEventID>.Instance.PostEvent(UIEventID.OnLoseGame, 0.2f);
                    }
                }
            }
        }

        private void Merges(int idArr, int cntMerge)
        {
            if (_isWinGame) return;
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
            if (_isWinGame) return;
            SoundManager.Instance.PlayFX(SoundId.Merge, 0.1f);
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

            // TODO: Point
            PlusPoint(nextItemType + 1);
            
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

        public void GetLevel()
        {
            var currentLevel = PlayerPrefs.GetInt("CurrentLevel");
            if (currentLevel < 1)
            {
                currentLevel = 1;
                PlayerPrefs.SetInt("CurrentLevel", currentLevel);
            }

            _currentLevel = currentLevel;
        }

        public void LoadLevel()
        {
            _dataCurrentLevel = Resources.Load<LevelData>($"LevelData/DataLevel_{_currentLevel}");
        }

        public void LoadNextLevel()
        {
            ++_currentLevel;
            if (_currentLevel > 20)
            {
                _currentLevel = 1;
            }
            LoadLevel();
        }

        public void LoadAllObjInLevel()
        {
            _isWinGame = false;
            _curPoint = 0;
            if (_currentLevelTrans == null)
            {
                _currentLevelTrans = new GameObject($"Level{_currentLevel}")
                {
                    transform =
                    {
                        parent = transform,
                    }
                }.transform;
            }
            else
            {
                _currentLevelTrans.name = $"Level{_currentLevel}";
            }
            SpawnBackground();
            SpawnMap();
            SpawnFruitInMap();
            SpawnQueueAndFruit();
            SetFruitInQueue();
        }

        public void ClearCurrentLevel()
        {
            PoolingManager.Despawn(_currentMap);
            foreach (Transform fruitInMap in _parentFruitInMap)
            {
                DOTween.Kill(fruitInMap);
                PoolingManager.Despawn(fruitInMap.gameObject);
            }

            foreach (Transform platformInQueue in _parentPlatformQueue)
            {
                PoolingManager.Despawn(platformInQueue.gameObject);
            }
            
            foreach (Transform fruitInQueue in _parentFruitInQueue)
            {
                foreach (Transform fruit in fruitInQueue)
                {
                    DOTween.Kill(fruit);
                    PoolingManager.Despawn(fruit.gameObject);
                }
            }
        }

        private void SpawnBackground()
        {
            if (_parentFruitInMap == null)
            {
                _parentBackground = new GameObject(name: "ParentBackground")
                {
                    transform =
                    {
                        parent = _currentLevelTrans,
                    }
                }.transform;
                PoolingManager.Spawn(_backgroundObj, Vector3.zero, Quaternion.identity, _parentBackground);
            }
        }

        private void SpawnMap()
        {
            if (_parentMap == null)
            {
                _parentMap = new GameObject(name: "ParentMap")
                {
                    transform =
                    {
                        parent = _currentLevelTrans,
                    }
                }.transform;
            }
            var idMap = (int) _dataCurrentLevel.mapType;
            _currentMap = PoolingManager.Spawn(_maps[idMap], Vector3.forward * 0.4f, Quaternion.identity, _parentMap);
            
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
            if (_parentFruitInMap == null)
            {
                _parentFruitInMap = new GameObject(name: "ParentFruitInMap")
                {
                    transform =
                    {
                        parent = _currentLevelTrans,
                    }
                }.transform;
            }
            var arrFruitCore = _dataCurrentLevel.arrFruitCore;
            
            foreach (var (idPos, typeFruit) in arrFruitCore)
            {
                Debug.Log(idPos - 1);
                var itemIdle = _arr[idPos - 1].itemIdle;
                var currentPlatform = itemIdle.GetTransform();
                var posFruit = currentPlatform.position;
                posFruit.z = 0f;
                var currentFruit = PoolingManager.Spawn(_fruitObj[typeFruit - 1], posFruit, Quaternion.identity, _parentFruitInMap);
                var itemMove = currentFruit.GetComponent<IItemMovingBase>();
                itemMove.ChangeState(Fruits.State.InPlatform);
                currentFruit.transform.rotation = currentPlatform.rotation;
                
                _arr[idPos - 1].itemIdle.ChangeState(Platforms.State.HaveFruit);
                _arr[idPos - 1].itemMove = itemMove;
            }
        }

        private void SpawnQueueAndFruit()
        {
            _cntFruitQueue = _dataCurrentLevel.cntFruitQueue;

            if (_parentPlatformQueue == null)
            {
                _parentPlatformQueue = new GameObject(name: "ParentPlatformQueue")
                {
                    transform =
                    {
                        parent = _currentLevelTrans,
                    }
                }.transform;
            }

            if (_parentFruitInQueue == null)
            {
                _parentFruitInQueue = new GameObject(name: "ParentFruitInQueue")
                {
                    transform =
                    {
                        parent = _currentLevelTrans,
                    }
                }.transform;
            }

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
            for (var i = 0; i < _parentFruitInQueue.childCount; ++i)
            {
                _parentQueues.Add(_parentFruitInQueue.GetChild(i).gameObject);
            }
            for (var i = _parentFruitInQueue.childCount; i < _cntFruitQueue; ++i)
            {
                _parentQueues.Add(new GameObject($"Queue_{i}")
                {
                    transform =
                    {
                        parent = _parentFruitInQueue,
                    }
                });
            }

            for (var i = 0; i < _cntFruitQueue; ++i)
            {
                var value = new List<InfoFruitInQueue>();
                var posFruitInQueue = posPlatformQueues[i] + _ofset;
                var k = 0;
                if (_dataCurrentLevel.isRandom)
                {
                    k = 1;
                }
                else
                {
                    k = 21;
                }

                var id = 0;
                var cntFruitInQueue = 0;
                for (var cnt = 0; cnt < k; ++cnt)
                {
                    for (var j = i; j < _dataCurrentLevel.arrFruitQueue.Count; j += _cntFruitQueue, ++ id, ++ cntFruitInQueue)
                    {
                        var infoFruitInQueue = new InfoFruitInQueue();
                        var curId = _dataCurrentLevel.arrFruitQueue[j];
                        if (cntFruitInQueue < _cntFruitShowInQueue)
                        {
                           var nFruit = PoolingManager.Spawn(_fruitObj[curId], posFruitInQueue, Quaternion.identity,
                                                        _parentQueues[i].transform);
                           infoFruitInQueue.itemMove = nFruit.GetComponent<IItemMovingBase>();
                        }
                        posFruitInQueue += Vector3.down * _distanceFruitInQueue;
                        _checkSpawnFruit[curId] = id;

                        infoFruitInQueue.idFruit = curId;
                        value.Add(infoFruitInQueue);
                    }
                }

                if (_dataCurrentLevel.isRandom)
                {
                    for (var j = 0; j < 20; ++j, ++ id, ++ cntFruitInQueue)
                    {
                        var idFruit = Random.Range(_dataCurrentLevel.startPoint - 1, _dataCurrentLevel.endPoint);
                        if (_checkSpawnFruit.ContainsKey(idFruit))
                        {
                            while (id - _checkSpawnFruit[idFruit] < _dataCurrentLevel.cntSpawnMax + 1)
                            {
                                idFruit = Random.Range(_dataCurrentLevel.startPoint - 1, _dataCurrentLevel.endPoint);
                                if (_checkSpawnFruit.ContainsKey(idFruit) == false) break;
                            }
                        }

                        _checkSpawnFruit[idFruit] = id;
                        var infoFruitInQueue = new InfoFruitInQueue();
                        if (cntFruitInQueue < _cntFruitShowInQueue)
                        {
                            var nFruit = PoolingManager.Spawn(_fruitObj[idFruit], posFruitInQueue, Quaternion.identity,
                                _parentQueues[i].transform);
                            infoFruitInQueue.itemMove = nFruit.GetComponent<IItemMovingBase>();
                        }
                        posFruitInQueue += Vector3.down * _distanceFruitInQueue;

                        value.Add(infoFruitInQueue);
                    }
                }
                
                _queueFruits.Add(value);
            }
        }

        private void SetFruitInQueue(bool value = true)
        {
            foreach (var queueFruit in _queueFruits)
            {
                var fruitInQueue = queueFruit[0];
                fruitInQueue.itemMove.ChangeState(value ? Fruits.State.InQueue : Fruits.State.Ban);
            }
        }

        private void NextFruitInQueue(object param)
        {
            if (param is IItemMovingBase currentFruit)
            {
                for (var i = 0; i < _queueFruits.Count; ++ i)
                {
                    var queueFruit = _queueFruits[i];
                    var fruitInQueue = queueFruit[0].itemMove;
                    if (fruitInQueue == currentFruit)
                    {
                        var idNewFruit = queueFruit[_cntFruitShowInQueue].idFruit;
                        var tranFruitBefore = queueFruit[_cntFruitShowInQueue - 1].itemMove.GetTransform();
                        var nFruit = PoolingManager.Spawn(_fruitObj[idNewFruit], tranFruitBefore.position + _distanceFruitInQueue * Vector3.down, Quaternion.identity, tranFruitBefore.parent);
                        queueFruit[_cntFruitShowInQueue].itemMove = nFruit.GetComponent<IItemMovingBase>();
                        queueFruit.RemoveAt(0);
                        SetFruitInQueue(false);
                        for (var j = 0; j < queueFruit.Count; ++j)
                        {
                            var item = queueFruit[j].itemMove;
                            if (item != null)
                            {
                                item.GetTransform().DOMoveY(item.GetTransform().position.y + _distanceFruitInQueue, 0.2f);
                            }
                        }
                        break;
                    }
                }
            }
        }

        private void PlusPoint(int amount)
        {
            _curPoint += amount;
            if (_curPoint >= _dataCurrentLevel.pointWin)
            {
                ObserverManager<UIEventID>.Instance.PostEvent(UIEventID.OnWinGame, 0.2f);
                _isWinGame = true;
            }
        }
    }
}