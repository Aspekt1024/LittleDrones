namespace Aspekt.Drones
{
    public interface IWorker : IAbility
    {
        float WorkerSkill { get; }

        void JobComplete();
    }
}