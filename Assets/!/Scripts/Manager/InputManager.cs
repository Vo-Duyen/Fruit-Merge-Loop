using System;
using System.Collections;
using DesignPattern;
using Sirenix.OdinInspector;
using UnityEngine;
using LongNC.Fruit;

namespace LongNC.Cube
{
    public interface IInputManager
    {
        Transform GetItems();
    }
    
    public class InputManager : Singleton<InputManager>, IInputManager
    {
        private bool _isCanControl;
        private RaycastHit[] _hits = new RaycastHit[10];
        private Coroutine _coroutine;
        private Fruits _fruits;

        private void OnEnable()
        {
            _isCanControl = true;
        }

        public Transform GetItems()
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
                if (hit.transform.TryGetComponent<Fruits>(out var fruit))
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
                    var items = GetItems();
                    if (items == null)
                    {
                        return;
                    }
                    _fruits = items.GetComponent<Fruits>();
                    if (_fruits.IsCanMove)
                    {
                        _fruits.OnClickDown();
                    }
                }
                else if (Input.GetMouseButton(0))
                {
                    if (_fruits != null && _fruits.IsCanMove)
                    {
                        _fruits.OnDrag();
                    }
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    if (_fruits != null && _fruits.IsCanMove)
                    {
                        _fruits.OnDrop();
                        _fruits = null;
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
