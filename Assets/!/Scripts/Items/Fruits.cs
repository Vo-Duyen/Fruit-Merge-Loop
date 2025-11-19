using System;
using System.Collections;
using DG.Tweening;
using LongNC.Manager;
using UnityEngine;

namespace LongNC.Items
{
    public class Fruits : ItemMovingBase<Fruits.State, Fruits.ItemType>
    {
        public enum State
        {
            Ban,
            InQueue,
            AnimInPlatform,
            InPlatform,
            AnimMerge,
            Done,
        }
        
        public enum ItemType
        {
            Berry,
            Chery,
            Dau,
            Nho,
            Chuoi,
            Tao,
            Cam,
            Le,
            Xoai,
        }

        public override bool IsCanMove => IsState(State.InQueue);
        private IItemIdleBase _itemIdleTargetMerge;

        protected override void OnEnable()
        {
            base.OnEnable();
            AnimSpawn();
            
            Observer.RegisterEvent(GameEvent.FruitInPlatform, FruitInPlatform);
            // Observer.RegisterEvent(GameEvent.MergeFruit, MergerFruit);
            Observer.RegisterEvent(GameEvent.AnimMerge, AnimMerge);
        }

        private void OnDisable()
        {
            Observer.RemoveEvent(GameEvent.FruitInPlatform, FruitInPlatform);
            // Observer.RemoveEvent(GameEvent.MergeFruit, MergerFruit);
            Observer.RemoveEvent(GameEvent.AnimMerge, AnimMerge);
            
            AnimDespawn();
        }

        private void FruitInPlatform(object param)
        {
            if (param is (IItemIdleBase itemIdle, IItemMovingBase item, Vector3 pos, Quaternion rot) && (Fruits)item == this && IsState(State.AnimInPlatform))
            {
                AnimInX(pos, rot, 0.2f, () =>
                {
                    itemIdle.ChangeState(Platforms.State.HaveFruit);
                });
            }
        }

        private void MergerFruit(object param)
        {
            
        }

        private void AnimMerge(object param)
        {
            if (param is (Info info, IItemIdleBase targetMerge) && (Fruits)info.itemMove == this)
            {
                _itemIdleTargetMerge = targetMerge;
                ChangeState(State.AnimMerge);
            }
        }
        
        public override void ChangeState<T>(T t)
        {
            base.ChangeState(t);
            switch (_state)
            {
                case State.Ban:
                    _collider.enabled = false;
                    break;
                case State.InQueue:
                    _collider.enabled = true;
                    OnSavePoint();
                    break;
                case State.AnimInPlatform:
                    _collider.enabled = false;
                    Observer.PostEvent(GameEvent.NextFruitInQueue, (IItemMovingBase) this);
                    break;
                case State.InPlatform:
                    _collider.enabled = false;
                    break;
                case State.AnimMerge:
                    var targetTF = _itemIdleTargetMerge.GetTransform();
                    var posTarget = targetTF.position;
                    posTarget.z = 0;
                    AnimInX(posTarget, targetTF.rotation, 0.2f);
                    break;
                case State.Done:
                    break;
            }
        }

        private void AnimInX(Vector3 pos, Quaternion rot, float time = 0.2f, Action callback = null)
        {
            TF.DOKill();
            DOTween.Sequence().Append(TF.DOJump(pos, 1f, 1, time))
                .Join(TF.DORotateQuaternion(rot, time))
                .AppendCallback(() =>
                {
                    try
                    {
                        callback?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                });
        }

        public override void OnClickDown()
        {
            base.OnClickDown();
            TF.DOMoveZ(TF.position.z - 0.7f, 0.2f);
        }

        private void AnimSpawn()
        {
            TF.localScale = Vector3.zero;
            TF.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
        }

        private void AnimDespawn()
        {
            TF.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
            {
                TF.localScale = Vector3.one;
            });
        }
    }
}