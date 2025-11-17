using DG.Tweening;
using Sirenix.Serialization;
using UnityEngine;

namespace LongNC
{
    public interface IItemMovingBase : IItemBase
    {
        bool IsCanMove { get; }
        void OnClickDown();
        void OnDrag();
        void OnClickTake();
        void OnDrop();
        void OnBack();
        void OnSavePoint(float timeDelay = 0f);
    }
    public class ItemMovingBase<TState, TType> : ItemBase<TState, TType>, IItemMovingBase
        where TState : System.Enum
        where TType : System.Enum
    {
        [OdinSerialize] protected bool _isBackOnDrop = true;
        private Vector3 _savePosition;
        protected Vector3 _scaleClick = new Vector3(1.2f, 1.2f, 1.2f);
        protected Vector3 _pivot;
        public virtual bool IsCanMove => false;

        protected virtual void OnEnable()
        {
            OnSavePoint();
        }
        public virtual void OnClickDown()
        {
            TF.DOScale(_scaleClick, 0.2f);
            var posMouse = Input.mousePosition;
            if (Camera.main == null)
            {
                Debug.LogError("No main camera");
                return;
            }
            posMouse.z = Camera.main.WorldToScreenPoint(TF.position).z;
            var posMouseInWorld = Camera.main.ScreenToWorldPoint(posMouse);
            _pivot = TF.position - posMouseInWorld;
        }

        public virtual void OnDrag()
        {
            var posMouse = Input.mousePosition;
            if (Camera.main == null)
            {
                Debug.LogError("No main camera");
                return;
            }
            posMouse.z = Camera.main.WorldToScreenPoint(TF.position).z;
            var posMouseInWorld = Camera.main.ScreenToWorldPoint(posMouse);
            TF.position = posMouseInWorld + _pivot;
        }

        public virtual void OnClickTake()
        {
            TF.DOScale(Vector3.one, 0.2f);
        }
        
        public virtual void OnDrop()
        {
            if (_isBackOnDrop)
            {
                OnBack();
            }

            TF.DOScale(Vector3.one, 0.2f);
        }

        public virtual void OnBack()
        {
            DOTween.Kill(TF);
            TF.DOMove(_savePosition, 0.2f);
            TF.DORotateQuaternion(Quaternion.identity, 0.2f);
        }

        public virtual void OnSavePoint(float timeDelay = 0f)
        {
            if (timeDelay == 0f)
            {
                _savePosition = transform.position;
            }
            else
            {
                DOVirtual.DelayedCall(timeDelay, () =>
                {
                    _savePosition = transform.position;
                });
            }
        }
    }
}