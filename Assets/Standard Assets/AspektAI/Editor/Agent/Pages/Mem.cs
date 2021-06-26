using UnityEngine.UIElements;

namespace Aspekt.AI.AgentEditor
{
    public class Mem : Page
    {
        public override string Title => "Mem";
        public override string TemplateName => "Mem";

        public override void UpdateContents()
        {
            Root.Add(new Label("mem"));
        }

    }
}