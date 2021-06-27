using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aspekt.AI.AgentEditor
{
    public class ActionViewer<L, V> : Page
    {
        public override string Title => "Viewer";
        public override string TemplateName => "ActionViewer";

        private AIAgent<L, V> aiAgent;

        private VisualElement header;
        private VisualElement viewContainer;

        private readonly List<IAIGoal<L, V>> expandedGoals = new List<IAIGoal<L, V>>();
        private readonly List<IAIAction<L, V>> expandedActions = new List<IAIAction<L, V>>();

        private List<IAIGoal<L, V>> goals;
        private List<IAIAction<L, V>> actions;

        private L selectedCondition;

        protected override void Setup()
        {
            header = new VisualElement();

            var refreshButton = new Button {text = "Refresh"};
            refreshButton.clicked += UpdateContents;
            header.Add(refreshButton);

            Root.Add(header);
            
            viewContainer = new VisualElement();
            Root.Add(viewContainer);
        }

        public override void UpdateContents()
        {
            viewContainer.Clear();
            
            if (Application.isPlaying)
            {
                SetupData();
                ShowAISetup();
            }
            else
            {
                aiAgent = null;
                viewContainer.Add(new Label("AI Agent info is only available in play mode"));
            }
        }

        private void SetupData()
        {
            aiAgent = Object.FindObjectOfType<AIAgent<L, V>>();
            goals = aiAgent.Goals.GetGoals();
            actions = aiAgent.Actions.GetActions();
        }

        private void ShowAISetup()
        {
            if (aiAgent == null)
            {
                viewContainer.Add(new Label("No AI Agent found"));
                return;
            }
            
            var aiName = new Label(aiAgent.GetType().Name);
            viewContainer.Add(aiName);

            if (aiAgent.Actions == null || aiAgent.Goals == null)
            {
                viewContainer.Add(new Label("AI Agent actions/goals are empty."));
                return;
            }

            foreach (var goal in goals)
            {
                var goalLabel = new Button {text = $"{goal} ({goal.Priority})"};
                viewContainer.Add(goalLabel);
                
                goalLabel.clicked += () => {

                    if (expandedGoals.Contains(goal))
                    {
                        expandedGoals.Remove(goal);
                    }
                    else
                    {
                        expandedGoals.Add(goal);
                    }
                    UpdateContents();
                };

                if (expandedGoals.Contains(goal))
                {
                    var goalDetails = GetGoalDetails(goal);
                    viewContainer.Add(goalDetails);
                }
            }
            
            foreach (var action in actions)
            {
                viewContainer.Add(GetActionDetails(action));
            }
        }

        private VisualElement GetGoalDetails(IAIGoal<L, V> goal)
        {
            var details = new VisualElement();
            var conditions = goal.GetConditions();
            foreach (var condition in conditions)
            {
                details.Add(CreateConditionElement(condition));
            }
            return details;
        }

        private VisualElement GetActionDetails(IAIAction<L, V> action)
        {
            var actionContainer = new VisualElement();
            actionContainer.AddToClassList("action");
            
            var title = new Button {text = action.ToString()};
            title.AddToClassList("action-title");
            actionContainer.Add(title);

            title.clicked += () => {
                if (expandedActions.Contains(action))
                {
                    expandedActions.Remove(action);
                }
                else
                {
                    expandedActions.Add(action);
                }
                UpdateContents();
            };

            if (expandedActions.Contains(action))
            {
                var componentsSuccess = action.CheckComponents();
                actionContainer.Add(new Label(componentsSuccess ? "Has Components" : "Components check failed"));
                
                var actionDetails = new VisualElement();
                actionDetails.AddToClassList("action-details");
                
                var preconditions = action.GetPreconditions();
                var effects = action.GetEffects();

                var preconditionsContainer = GetConditionContainer("Preconditions", preconditions);
                actionDetails.Add(preconditionsContainer);
            
                var effectsContainer = GetConditionContainer("Effects", effects);
                actionDetails.Add(effectsContainer);
                
                actionContainer.Add(actionDetails);
            }
            
            return actionContainer;
        }

        private VisualElement GetConditionContainer(string title, Dictionary<L, V> values)
        {
            var container = new VisualElement();
            container.AddToClassList("condition-container");

            var titleLabel = new Label(title);
            titleLabel.AddToClassList("condition-container-heading");
            container.Add(titleLabel);

            foreach (var v in values)
            {
                container.Add(CreateConditionElement(v));
            }
            return container;
        }

        private Button CreateConditionElement(KeyValuePair<L, V> condition)
        {
            var conditionLabel = new Button {text = $"{condition.Key}"};
            conditionLabel.AddToClassList("attribute");
            
            conditionLabel.clicked += () =>
            {
                selectedCondition = condition.Key;
                UpdateContents();
            };
            
            var isMet = aiAgent.Memory.IsMatch(condition);
            conditionLabel.AddToClassList(isMet ? "attribute-met" : "attribute-unmet");
            
            if (condition.Key.Equals(selectedCondition))
            {
                conditionLabel.AddToClassList("attribute-selected");
            }

            return conditionLabel;
        }
    }
}