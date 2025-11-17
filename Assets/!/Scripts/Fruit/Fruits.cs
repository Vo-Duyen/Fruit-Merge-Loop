namespace LongNC.Fruit
{
    public class Fruits : ItemMovingBase<Fruits.State>
    {
        public enum State
        {
            Ban,
            InQueue,
            AnimInPlatform, InPlatform,
            AnimMerge, Merge,
        }

        public override bool IsCanMove => IsState(State.InQueue);

        public override void ChangeState<T>(T t)
        {
            base.ChangeState(t);
            switch (_state)
            {
                case State.Ban:
                    break;
                case State.InQueue:
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
    }
}