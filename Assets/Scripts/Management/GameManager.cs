using UnityEngine;

namespace Aspekt.Drones
{
    /// <summary>
    /// Handles the operation of the game from initialisation to completion
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
#pragma warning disable 649
        [SerializeField] private UIManager uiManager;
#pragma warning restore 649
        
        private ObjectManager objectManager;
        private UnitManager unitManager;
        private GameplayManager gameplayManager;
        private CameraManager cameraManager;

        public static ObjectManager Objects => instance.objectManager;
        public static UnitManager Units => instance.unitManager;
        public static UIManager UI => instance.uiManager;
        public static GameplayManager Gameplay => instance.gameplayManager;
        public static CameraManager Camera => instance.cameraManager;

        private enum States
        {
            Initialising,
            Running,
            Paused,
        }

        private States state = States.Initialising;

        protected override void Init()
        {
            Debug.Log("GameManager bootstrap initiated.");
            state = States.Initialising;
            CreateAndInitializeManagers();
            Debug.Log("GameManager and Managers initialised.");
        }

        private void Update()
        {
            if (state == States.Initialising)
            {
                StartGame();
            }
        }

        private void StartGame()
        {
            state = States.Running;
        }

        private void CreateAndInitializeManagers()
        {
            objectManager = new ObjectManager();
            unitManager = new UnitManager();
            gameplayManager = new GameplayManager();
            cameraManager = new CameraManager();
            
            objectManager.Init();
            unitManager.Init();
            uiManager.Init();
            gameplayManager.Init();
            cameraManager.Init();
        }
        
    }
}