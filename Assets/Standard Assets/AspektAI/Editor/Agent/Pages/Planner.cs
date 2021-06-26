using UnityEngine.UIElements;

namespace Aspekt.AI.AgentEditor
{
    public class Planner : Page
    {
        public override string Title => "Planner";
        public override string TemplateName => "Planner";

        public override void UpdateContents()
        {
            Root.Add(new Label("hello"));
        }

    }
}