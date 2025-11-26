using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DesignPattern.Observer;
using DG.Tweening;
using LongNC.Manager;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace LongNC
{
    public interface IItemBase
    {
        Transform GetTransform();
        bool IsState<T>(T t);
        bool IsState<T>(params T[] t);
        bool IsType<T>(T t);
        bool IsType<T>(params T[] t);
        TItemType GetItemType<TItemType>();
        void ChangeState<T>(T t);
    }
    public class VatPhamCoBan<TState, TType> : SerializedMonoBehaviour, IItemBase
        where TState : Enum
        where TType : Enum
    {
        [OdinSerialize] protected TState _state;
        [OdinSerialize] protected TType _type;
        [OdinSerialize] protected Collider _collider;
        
        protected ToiLaAi<GameEvent> Observer = ToiLaAi<GameEvent>.Instance;

        public Transform TF => transform;

        public Transform GetTransform()
        {
            return TF;
        }

        public virtual bool IsState<T>(T t)
        {
            return _state.Equals((TState)(object)t);
        }
        
        public virtual bool IsState<T>(params T[] t)
        {
            return t.Any(IsState);
        }
        
        public virtual bool IsType<T>(T t)
        {
            return _type.Equals((TType)(object)t);
        }
        
        public virtual bool IsType<T>(params T[] t)
        {
            return t.Any(IsType);
        }

        public TItemType GetItemType<TItemType>()
        {
            return (TItemType)(object)_type;
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