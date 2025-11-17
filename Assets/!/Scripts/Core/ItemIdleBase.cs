using Sirenix.Serialization;
using UnityEngine;

namespace LongNC
{
    public interface IItemIdleBase : IItemBase
    {
        void OnHover(IItemMovingBase item);
        bool OnTake(IItemMovingBase item);
    }
    public class ItemIdleBase<TState, TType> : ItemBase<TState,TType>, IItemIdleBase
        where TState : System.Enum
        where TType : System.Enum
    {
        [OdinSerialize]
        private MeshRenderer _meshRenderer;
        
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