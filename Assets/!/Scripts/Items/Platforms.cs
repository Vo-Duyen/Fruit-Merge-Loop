using System;
using System.Collections;
using LongNC.Manager;
using Sirenix.Serialization;
using UnityEngine;

namespace LongNC.Items
{
    public class Platforms : ItemIdleBase<Platforms.State, Platforms.ItemType>
    {
        public enum State
        {
            Idle,
            AnimGetFruit, HaveFruit,
        }
        public enum ItemType
        {
            
        }
        private MeshRenderer _meshRenderer;
        private IItemMovingBase _currentItems;
        private Coroutine _coroutine;

        private void OnEnable()
        {
            _meshRenderer = GetComponentInChildren<MeshRenderer>();
        }

        public override void ChangeState<T>(T t)
        {
            base.ChangeState(t);
            switch (_state)
            {
                case State.Idle:
                    break;
                case State.AnimGetFruit:
                    var posFruit = TF.position;
                    posFruit.z = 0;
                    Observer.PostEvent(GameEvent.FruitInPlatform, ((IItemIdleBase) this, _currentItems));
                    Observer.PostEvent(GameEvent.FruitInPlatform, ((IItemIdleBase) this, _currentItems, posFruit, transform.rotation));
                    break;
                case State.HaveFruit:
                    if (_currentItems != null)
                    {
                        Observer.PostEvent(GameEvent.CheckMerge, _currentItems);
                        _currentItems = null;
                    }
                    break;
            }
        }

        public override void OnHover(IItemMovingBase item)
        {
            if (IsState(State.Idle))
            {
                var mats = _meshRenderer.materials;
                mats[0].color = Color.yellow;
                if (_coroutine != null) StopCoroutine(_coroutine);
                _coroutine = StartCoroutine(IERemoveMaterialYellow());
            }
        }

        private IEnumerator IERemoveMaterialYellow()
        {
            yield return WaitForSecondCache.Get(0.1f);
            
            var mats = _meshRenderer.materials;
            mats[0].color = Color.white;
        }

        public override bool OnTake(IItemMovingBase item)
        {
            if (item is Fruits && item.IsState(Fruits.State.InQueue) && IsState(State.Idle))
            {
                _currentItems = item;
                item.ChangeState(Fruits.State.AnimInPlatform);
                ChangeState(State.AnimGetFruit);
                return true;
            }
            
            return base.OnTake(item);
        }
    }
}