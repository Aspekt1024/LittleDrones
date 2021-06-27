using System.Collections.Generic;
using System.Linq;
using Aspekt.AI.Planning;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aspekt.AI.AgentEditor
{
    public abstract class Planner<L, V> : Page, AIAgent<L, V>.IObserver
    {
        public override string Title => "Planner";
        public override string TemplateName => "Planner";

        private AIAgent<L, V> aiAgent;
        private ActionPlan<L, V> actionPlan;
        private PlannerDiagnosticData<L, V> diagnostics;

        private readonly List<ActionPlan<L, V>> previousPlans = new List<ActionPlan<L, V>>();
        private readonly List<PlannerDiagnosticData<L, V>> previousDiagnostics = new List<PlannerDiagnosticData<L, V>>();

        private bool diagnosticsEnabled;
        private VisualElement header;
        private VisualElement planInfo;
        private VisualElement diagnosticsInfo;

        protected override void Setup()
        {
            header = new VisualElement();
            
            var refreshButton = new Button { text = "Refresh" };
            refreshButton.clicked += UpdateContents;
            header.Add(refreshButton);
            
            Root.Add(header);
            
            planInfo = new VisualElement();
            Root.Add(planInfo);
            
            diagnosticsInfo = new VisualElement();
            diagnosticsInfo.AddToClassList("diagnostics-container");
            Root.Add(diagnosticsInfo);
        }

        public override void UpdateContents()
        {
            SetupData();
            
            diagnosticsInfo.Clear();
            planInfo.Clear();
            
            if (Application.isPlaying)
            {
                RegisterToAgent();
                
                if (aiAgent != null)
                {
                    var diagnosticsButton = new Button {text = diagnosticsEnabled ? "Diagnostics Enabled" : "Diagnostics Disabled"};
                    diagnosticsButton.clicked += ToggleDiagnostics;
                    diagnosticsInfo.Add(diagnosticsButton);
                    DisplayActionInfo();
                }
            }
            else
            {
                if (aiAgent != null) aiAgent.UnregisterObserver(this);
                aiAgent = null;
            }
        }

        private void SetupData()
        {
            actionPlan = new ActionPlan<L, V>();
            previousPlans.Clear();
        }

        public void OnAgentPlanCalculated(Queue<IAIAction<L, V>> actions, IAIGoal<L, V> goal)
        {
            DisplayDiagnosticsInfo(aiAgent.GetDiagnostics());
            
            if (actionPlan.IsValid)
            {
                previousPlans.Add(actionPlan);
            }
            
            actionPlan = new ActionPlan<L, V>
            {
                Goal = goal,
                Actions = new Queue<IAIAction<L, V>>(actions),
            };
            
            DisplayActionInfo();
        }

        public void OnAgentPlanCalculationFailure()
        {
            DisplayDiagnosticsInfo(aiAgent.GetDiagnostics());
        }

        private void DisplayActionInfo()
        {
            if (actionPlan.IsValid)
            {
                planInfo.Add(new Label(actionPlan.Goal.ToString()));
                foreach (var action in actionPlan.Actions)
                {
                    planInfo.Add(new Label(action.ToString()));
                }
            }
            else
            {
                planInfo.Add(new Label("No action plan yet"));
            }
        }

        private void DisplayDiagnosticsInfo(PlannerDiagnosticData<L, V> newDiagnostics)
        {
            if (diagnostics != null)
            {
                previousDiagnostics.Add(diagnostics);
            }
            diagnostics = newDiagnostics;
            
            diagnosticsInfo.Clear();
            if (diagnostics == null || !diagnostics.GoalInfo.Any()) return;
            
            diagnostics.GoalInfo.ForEach(g =>
            {
                diagnosticsInfo.Add(new Label(g.Goal.ToString()));
                g.Messages.ForEach(m => diagnosticsInfo.Add(new Label(m)));
            });
        }

        private void RegisterToAgent()
        {
            aiAgent = Object.FindObjectOfType<AIAgent<L, V>>();
            if (aiAgent == null)
            {
                header.Add(new Label("Could not find ai agent!"));
                return;
            }
            aiAgent.RegisterObserver(this);
        }

        private void ToggleDiagnostics()
        {
            diagnosticsEnabled = !diagnosticsEnabled;
            aiAgent.SetDiagnosticsStatus(diagnosticsEnabled);
            UpdateContents();
        }

        ~Planner()
        {
            if (aiAgent != null)
            {
                aiAgent.UnregisterObserver(this);
            }
        }
    }
}