using System;
using DG.Tweening;
using LongNC.Manager;
using UnityEngine;

namespace LongNC.Items
{
    public class Fruits : ItemMovingBase<Fruits.State, Platforms.ItemType>
    {
        public enum State
        {
            Ban,
            InQueue,
            AnimInPlatform,
            InPlatform,
            AnimMerge,
            Merge,
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

        protected override void OnEnable()
        {
            base.OnEnable();
            Observer.RegisterEvent(GameEvent.FruitInPlatform, FruitInPlatform);
            Observer.RegisterEvent(GameEvent.MergeFruit, MergerFruit);
        }

        private void OnDisable()
        {
            Observer.RemoveEvent(GameEvent.FruitInPlatform, FruitInPlatform);
            Observer.RemoveEvent(GameEvent.MergeFruit, MergerFruit);
        }

        private void FruitInPlatform(object param)
        {
            if (param is (IItemIdleBase itemIdle, IItemMovingBase item, Vector3 pos, Quaternion rot) && (Fruits)item == this && IsState(State.AnimInPlatform))
            {
                AnimInX(pos, rot, callback: () =>
                {
                    itemIdle.ChangeState(Platforms.State.HaveFruit);
                });
            }
        }

        private void MergerFruit(object param)
        {
            
        }

        public override void ChangeState<T>(T t)
        {
            base.ChangeState(t);
            switch (_state)
            {
                case State.Ban:
                    break;
                case State.InQueue:
                    OnSavePoint();
                    break;
                case State.AnimInPlatform:
                    break;
                case State.InPlatform:
                    break;
                case State.AnimMerge:
                    break;
                case State.Merge:
                    break;
            }
        }

        private void AnimInX(Vector3 pos, Quaternion rot, float time = 0.2f, Action callback = null)
        {
            DOTween.Sequence().Append(TF.DOJump(pos, 1f, 1, time))
                .Join(TF.DORotateQuaternion(rot, time))
                .AppendCallback(() => callback?.Invoke());
        }

        public override void OnClickDown()
        {
            base.OnClickDown();
            TF.DOMoveZ(TF.position.z - 0.7f, 0.2f);
        }
    }
}