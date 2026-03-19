using MonopolyLite.Data;

namespace MonopolyLite.Logic
{
    public interface ITargetProvider
    {
        TargetProfile GetRandomTarget(int boardIndex);
    }
}
