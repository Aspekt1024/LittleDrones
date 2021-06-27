using System.IO;
using UnityEditor;
using UnityEngine.UIElements;

namespace Aspekt.AI
{
    public abstract class Page
    {
        public abstract string Title { get; }
        public abstract string TemplateName { get; }
        public VisualElement Root { get; private set; }

        protected TabbedWindow Editor;

        public void Init(TabbedWindow editor, VisualElement root, string templateName)
        {
            Editor = editor;
            AddToRoot(root, templateName);
            UpdateContents();
        }

        private void AddToRoot(VisualElement editorRoot, string templateName)
        {
            var rootDir = Path.Combine(Editor.DirectoryRoot, Editor.PagesSubDirectory);
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(rootDir, $"{templateName}.uxml"));
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.Combine(rootDir, $"{templateName}.uss"));
            
            Root = new VisualElement();
            visualTree.CloneTree(Root);
            Root.styleSheets.Add(styleSheet);
            editorRoot.Add(Root);
            
            Setup();
        }

        protected abstract void Setup();
        
        public abstract void UpdateContents();
    }
}