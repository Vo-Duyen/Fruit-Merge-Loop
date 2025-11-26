using Sirenix.Serialization;
using UnityEngine;

namespace LongNC
{
    public interface IItemIdleBase : IItemBase
    {
        void OnHover(IItemMovingBase item);
        bool OnTake(IItemMovingBase item);
    }
    public class VatPhamDungImCoBan<TState, TType> : VatPhamCoBan<TState,TType>, IItemIdleBase
        where TState : System.Enum
        where TType : System.Enum
    {
        public virtual bool OnTake(IItemMovingBase item)
        {
            return false;
        }

        public virtual void OnHover(IItemMovingBase item)
        {
            Debug.Log("Hover");
        }
    }
}