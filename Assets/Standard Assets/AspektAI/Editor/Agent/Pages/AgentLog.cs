using UnityEngine;
using UnityEngine.UIElements;

namespace Aspekt.AI.AgentEditor
{
    public class AgentLog<L, V> : Page, AILogger.IObserver
    {
        public override string Title => "Log";
        public override string TemplateName => "AgentLog";

        private AIAgent<L, V> aiAgent;

        private VisualElement header;
        private VisualElement logContainer;

        private bool traceEnabled;
        private bool infoEnabled;
        
        protected override void Setup()
        {
            header = Root.Q("Header");
            logContainer = Root.Q("LogContents");

            var clearButton = new Button {text = "Clear Log"};
            clearButton.clicked += UpdateContents;
            header.Add(clearButton);
            
            var traceButton = new Button {text = "ToggleTrace"};
            traceButton.clicked += () => traceEnabled = !traceEnabled;
            header.Add(traceButton);
            
            var infoButton = new Button {text = "ToggleInfo"};
            infoButton.clicked += () => infoEnabled = !infoEnabled;
            header.Add(infoButton);
        }
        
        public override void UpdateContents()
        {
            logContainer.Clear();
            
            if (Application.isPlaying)
            {
                RegisterToAgentLog();
                if (aiAgent == null)
                {
                    logContainer.Add(new Label("Unable to find AI Agent!"));
                }
            }
            else
            {
                logContainer.Add(new Label("Logging only available in play mode"));
            }
        }

        private void RegisterToAgentLog()
        {
            aiAgent = Object.FindObjectOfType<AIAgent<L, V>>();
            if (aiAgent == null) return;

            aiAgent.Logger.UnregisterObserver(this);
            aiAgent.Logger.RegisterObserver(this);
        }


        public void OnLogMessageReceived(AILogType type, string parent, string message)
        {
            if (type == AILogType.Trace && !traceEnabled) return;
            if (type == AILogType.Info && !infoEnabled) return;
            
            var t = type switch
            {
                AILogType.Info => "INFO",
                AILogType.Trace => "TRACE",
                AILogType.KeyInfo => "KEY",
                _ => "UNDEF"
            };
            logContainer.Add(new Label($"{System.DateTime.Now} {t} {parent}: {message}"));
        }
    }
}