
namespace Aspekt.Drones
{
    public class WorkerComponent : IWorker
    {
        public float WorkerSkill { get; } = 1f;
        public bool Enabled { get; set; } = true;
        public IWorkstation Workstation { get; private set; }

        private readonly IUnit unit;
        
        public WorkerComponent(IUnit unit)
        {
            this.unit = unit;
        }
        
        public void JobComplete()
        {
            Workstation = null;
        }
    }
}