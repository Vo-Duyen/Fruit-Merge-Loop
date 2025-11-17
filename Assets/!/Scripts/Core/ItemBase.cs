using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace LongNC
{
    public class ItemBase<TState> : SerializedMonoBehaviour
        where TState : Enum
    {
        [OdinSerialize] protected TState _state;
        [OdinSerialize] protected Collider _collider;

        public Transform TF => transform;

        public virtual bool IsState<T>(T t)
        {
            return _state.Equals((TState)(object)t);
        }
        
        public virtual bool IsState<T>(params T[] t)
        {
            return t.Any(IsState);
        }

        public virtual void ChangeState<T>(T t)
        {
            _state = (TState)(object)t;
        }

        
        
#if UNITY_EDITOR
        [Button]
        protected virtual void Setup()
        {
            if (_collider == null)
            {
                var queue = new Queue<Transform>();
                queue.Enqueue(transform);
                while (_collider == null && queue.Count > 0)
                {
                    var target = queue.Dequeue();
                    if (target.TryGetComponent<Collider>(out var col))
                    {
                        _collider = col;
                        break;
                    }

                    foreach (Transform childTrans in target)
                    {
                        queue.Enqueue(childTrans);
                    }
                }
            }
        }
#endif
    }
}