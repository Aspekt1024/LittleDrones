using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aspekt.AI
{
    public abstract class TabbedWindow : EditorWindow
    {
        public abstract string DirectoryRoot { get; }
        public abstract string CoreDirectoryRoot { get; }
        public abstract string PagesSubDirectory { get; }

        protected VisualElement Root;
        private Toolbar toolbar;
        
        private readonly List<Page> pages = new List<Page>();

        protected static void Display<T>(string title, Vector2 minSize) where T : TabbedWindow
        {
            var window = GetWindow<T>();
            window.titleContent = new GUIContent(title);
            window.Show();

            window.minSize = minSize;
        }
        
        protected virtual void OnEnable()
        {
            Root = rootVisualElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(CoreDirectoryRoot, "TabbedWindow.uxml"));
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.Combine(CoreDirectoryRoot, "TabbedWindow.uss"));

            if (visualTree == null || styleSheet == null)
            {
                Debug.LogError($"Failed to load tabbed window template at {CoreDirectoryRoot}. Did you move the folder without updating the Editor's directory references?");
            }

            visualTree.CloneTree(Root);
            Root.styleSheets.Add(styleSheet);

            toolbar = new Toolbar(Root, CoreDirectoryRoot);
            
            AddPages();
            
            toolbar.Init();

            Undo.undoRedoPerformed += DataFilesUpdated;
            this.SetAntiAliasing(4);
        }

        private void DataFilesUpdated()
        {
            pages.ForEach(p => p.UpdateContents());
        }
        

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= DataFilesUpdated;
        }
        
        protected abstract void AddPages();

        protected void AddPage(Page page)
        {
            page.Init(this, Root, page.TemplateName);
            pages.Add(page);
            toolbar.AddPage(page);
        }
    }
}