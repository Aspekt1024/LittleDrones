using UnityEngine.UIElements;

namespace Aspekt.AI.AgentEditor
{
    public class Mem : Page
    {
        public override string Title => "Mem";
        public override string TemplateName => "Mem";

        private VisualElement memContainer;
        
        protected override void Setup()
        {
            memContainer = new VisualElement();
        }
        
        public override void UpdateContents()
        {
            memContainer.Add(new Label("mem"));
        }

    }
}