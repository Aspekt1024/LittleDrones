using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace Aspekt.AI
{
    public class Toolbar
    {
        private readonly VisualElement items;
        private readonly VisualElement modifyIndicator;
        
        private static Toolbar current;
        private readonly List<Button> buttons = new List<Button>();
        private readonly List<Page> pages = new List<Page>();

        private const string CurrentPagePref = "currentPage";
        
        public Toolbar(VisualElement editorRoot, string directoryRoot)
        {
            current = this;
            
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(directoryRoot, "Components/Toolbar.uxml"));
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.Combine(directoryRoot, "Components/Toolbar.uss"));

            visualTree.CloneTree(editorRoot);
            var toolbarRoot = editorRoot.Q("Toolbar");
            toolbarRoot.styleSheets.Add(styleSheet);
            
            items = toolbarRoot.Q("ItemContainer");
            modifyIndicator = toolbarRoot.Q("ModifyIndicator");
        }

        public void Init()
        {
            var currentPage = EditorPrefs.GetString(CurrentPagePref);
            if (pages.Any())
            {
                if (!string.IsNullOrEmpty(currentPage))
                {
                    var pageIndex = pages.FindIndex(p => p.Title == currentPage);
                    if (pageIndex < 0)
                    {
                        SelectDefaultPage();
                    }
                    else
                    {
                        HighlightButton(buttons[pageIndex]);
                        OnPageSelected(pages[pageIndex]);
                    }
                }
                else
                {
                    SelectDefaultPage();
                }
            }
        }

        private void SelectDefaultPage()
        {
            HighlightButton(buttons[0]);
            OnPageSelected(pages[0]);
            EditorPrefs.SetString(CurrentPagePref, pages[0].Title);
        }

        public void AddPage(Page page)
        {
            pages.Add(page);
            CreateToolbarButton(page);
        }

        private static string[] OnWillSaveAssets(string[] paths)
        {
            if (current == null) return null;
            current.modifyIndicator.RemoveFromClassList("modify-indicator");
            return paths;
        }

        public void ShowModified()
        {
            modifyIndicator.AddToClassList("modify-indicator");
        }
        
        private Button CreateToolbarButton(Page page)
        {
            var btn = new Button { text = page.Title };
            btn.clicked += () =>
            {
                HighlightButton(btn);
                OnPageSelected(page);
                
            };
            items.Add(btn);
            buttons.Add(btn);
            
            return btn;
        }

        private void OnPageSelected(Page selectedPage)
        {
            EditorPrefs.SetString(CurrentPagePref, selectedPage.Title);
            foreach (var page in pages)
            {
                if (page.Root == selectedPage.Root)
                {
                    ElementUtil.ShowElement(page.Root);
                }
                else
                {
                    ElementUtil.HideElement(page.Root);
                }
            }
        }

        private void HighlightButton(Button button)
        {
            const string activeClassName = "active-button";
            
            foreach (var btn in buttons)
            {
                if (btn == button)
                {
                    button.AddToClassList(activeClassName);
                }
                else
                {
                    btn.RemoveFromClassList(activeClassName);
                }
            }
        }
    }
}