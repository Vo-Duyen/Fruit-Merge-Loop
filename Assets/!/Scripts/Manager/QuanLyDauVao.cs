using System;
using System.Collections;
using System.Collections.Generic;
using DesignPattern;
using Sirenix.OdinInspector;
using UnityEngine;
using LongNC.Items;
using UnityEngine.EventSystems;

namespace LongNC.Cube
{
    public interface IInputManager
    {
        Transform GetItemIdle();
        Transform GetItemMoving();
    }
    
    public class QuanLyDauVao : DuyNhat<QuanLyDauVao>, IInputManager
    {
        private bool coDuocDieuKhien;
        private RaycastHit[] _hits = new RaycastHit[10];
        private Coroutine _coroutine;
        private IItemIdleBase _itemIdle;
        private IItemMovingBase _itemMoving;

        // private void OnEnable()
        // {
        //     _isCanControl = true;
        // }

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
            if (coDuocDieuKhien)
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
                if (Input.GetMouseButton(0))
                {
                    if (!IsPointerOverUIElement())
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
                }
                if (Input.GetMouseButtonUp(0) || IsPointerOverUIElement())
                {
                    if (_itemMoving is { IsCanMove: true })
                    {
                        var itemIdle = GetItemIdle();
                        if (itemIdle == null)
                        {
                            _itemMoving.OnDrop();
                            _itemMoving = null;
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

        private bool IsPointerOverUIElement()
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, results);

            foreach (RaycastResult unused in results)
            {
                if (unused.gameObject.layer == LayerMask.NameToLayer("UI"))
                {
                    return true;
                }
            }

            return false;
        }

        private IEnumerator IEDelay(float time, Action callback)
        {
            yield return ToiChoBan.Get(time);

            callback?.Invoke();
        }
        
        [Button]
        public void SetIsCanControl(bool isCanControl = true, float timeDelay = 0f)
        {
            if (timeDelay == 0f)
            {
                coDuocDieuKhien = isCanControl;
            }
            else
            {
                _coroutine ??= StartCoroutine(IEDelay(timeDelay, () =>
                {
                    coDuocDieuKhien = isCanControl;
                    _coroutine = null;
                }));
            }
        }
    }
}
