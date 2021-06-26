using UnityEditor;
using UnityEngine;

namespace Aspekt.AI.AgentEditor
{
    public class AIAgentEditor : TabbedWindow
    {
        public override string DirectoryRoot => "Assets/Standard Assets/AspektAI/Editor/Agent/";
        public override string CoreDirectoryRoot => "Assets/Standard Assets/AspektAI/Editor/Core/";
        public override string PagesSubDirectory => "Pages";

        [MenuItem("Tools/AI Agent Editor _%#I")]
        private static void ShowWindow()
        {
            Display<AIAgentEditor>("AI Agent", new Vector2(500, 400));
        }
        
        protected override void AddPages()
        {
            AddPage(new Planner());
            AddPage(new Mem());
        }

    }
}