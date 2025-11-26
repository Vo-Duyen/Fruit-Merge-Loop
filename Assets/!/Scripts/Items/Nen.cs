using System;
using System.Collections;
using LongNC.Manager;
using Sirenix.Serialization;
using UnityEngine;

namespace LongNC.Items
{
    public class Nen : VatPhamDungImCoBan<Nen.State, Nen.ItemType>
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
                    Observer.PhatSuKien(GameEvent.FruitInPlatform, ((IItemIdleBase) this, _currentItems));
                    Observer.PhatSuKien(GameEvent.FruitInPlatform, ((IItemIdleBase) this, _currentItems, posFruit, transform.rotation));
                    break;
                case State.HaveFruit:
                    if (_currentItems != null)
                    {
                        Observer.PhatSuKien(GameEvent.CheckMerge, _currentItems);
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
                mats[0].color = ColorUtility.TryParseHtmlString("#FFD500", out var color) ? color : Color.white;
                if (_coroutine != null) StopCoroutine(_coroutine);
                _coroutine = StartCoroutine(IERemoveMaterialYellow());
            }
        }

        private IEnumerator IERemoveMaterialYellow()
        {
            yield return ToiChoBan.Get(0.1f);
            
            var mats = _meshRenderer.materials;
            mats[0].color = Color.white;
        }

        public override bool OnTake(IItemMovingBase item)
        {
            if (item is Qua && item.IsState(Qua.State.InQueue) && IsState(State.Idle))
            {
                _currentItems = item;
                item.ChangeState(Qua.State.AnimInPlatform);
                ChangeState(State.AnimGetFruit);
                return true;
            }
            
            return base.OnTake(item);
        }
    }
}