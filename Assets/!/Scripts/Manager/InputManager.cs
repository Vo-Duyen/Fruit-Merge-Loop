using System;
using System.Collections;
using DesignPattern;
using Sirenix.OdinInspector;
using UnityEngine;
using LongNC.Items;

namespace LongNC.Cube
{
    public interface IInputManager
    {
        Transform GetItemIdle();
        Transform GetItemMoving();
    }
    
    public class InputManager : Singleton<InputManager>, IInputManager
    {
        private bool _isCanControl;
        private RaycastHit[] _hits = new RaycastHit[10];
        private Coroutine _coroutine;
        private IItemIdleBase _itemIdle;
        private IItemMovingBase _itemMoving;

        private void OnEnable()
        {
            _isCanControl = true;
        }

        public Transform GetItemIdle()
        {
            Transform ans = null;
            if (Camera.main == null)
            {
                Debug.LogWarning("No Main Camera");
                return null;
            }
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var size = Physics.RaycastNonAlloc(ray, _hits, Mathf.Infinity);
            
            for (var i = 0; i < size; ++ i)
            {
                var hit = _hits[i];
                // Debug.Log(hit.collider.gameObject.name);
                if (hit.transform.TryGetComponent<IItemIdleBase>(out var itemIdle))
                {
                    ans = hit.transform;
                    break;
                }
            }
            return ans;
        }

        public Transform GetItemMoving()
        {
            Transform ans = null;
            if (Camera.main == null)
            {
                Debug.LogWarning("No Main Camera");
                return null;
            }
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var size = Physics.RaycastNonAlloc(ray, _hits, Mathf.Infinity);
            
            for (var i = 0; i < size; ++ i)
            {
                var hit = _hits[i];
                // Debug.Log(hit.collider.gameObject.name);
                if (hit.transform.TryGetComponent<IItemMovingBase>(out var itemMoving))
                {
                    ans = hit.transform;
                    break;
                }
            }
            return ans;
        }

        private void Update()
        {
            if (_isCanControl)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    var itemMoving = GetItemMoving();
                    if (itemMoving == null)
                    {
                        return;
                    }
                    _itemMoving = itemMoving.GetComponent<IItemMovingBase>();
                    if (_itemMoving.IsCanMove)
                    {
                        _itemMoving.OnClickDown();
                    }
                }
                else if (Input.GetMouseButton(0))
                {
                    if (_itemMoving is { IsCanMove: true })
                    {
                        _itemMoving.OnDrag();
                        var itemIdle = GetItemIdle();
                        if (itemIdle == null)
                        {
                            return;
                        }

                        _itemIdle = itemIdle.GetComponent<IItemIdleBase>();
                        _itemIdle.OnHover(_itemMoving);
                    }
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    if (_itemMoving is { IsCanMove: true })
                    {
                        var itemIdle = GetItemIdle();
                        if (itemIdle == null)
                        {
                            _itemMoving.OnDrop();
                            return;
                        }

                        _itemIdle = itemIdle.GetComponent<IItemIdleBase>();
                        if (_itemIdle.OnTake(_itemMoving))
                        {
                            _itemMoving.OnClickTake();
                        }
                        else
                        {
                            _itemMoving.OnDrop();
                        }
                        _itemMoving = null;
                    }
                }
            }
        }

        private IEnumerator IEDelay(float time, Action callback)
        {
            yield return WaitForSecondCache.Get(time);

            callback?.Invoke();
        }
        
#if UNITY_EDITOR
        [Button]
        public void SetIsCanControl(bool isCanControl = true, float timeDelay = 0f)
        {
            if (timeDelay == 0f)
            {
                _isCanControl = isCanControl;
            }
            else
            {
                _coroutine ??= StartCoroutine(IEDelay(timeDelay, () =>
                {
                    _isCanControl = isCanControl;
                    _coroutine = null;
                }));
            }
        }
#endif
    }
}
