namespace Aspekt.Drones
{
    public interface IWorker
    {
        float WorkerSkill { get; }

        void JobComplete();
    }
}