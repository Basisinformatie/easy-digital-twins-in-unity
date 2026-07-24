using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rotterdam.DigitalTwins.Editor
{
    /// <summary>
    /// UI Component for displaying Markdown documentation files.
    /// </summary>
    public class ReadMeComponent : VisualElement
    {
        private readonly System.Action _onBackToMenu;
        private readonly VisualElement _contentContainer;
        private readonly ScrollView _scrollView;

        private readonly string[] _docPaths = {
            "README.md",
            "SETUP_GUIDE.md",
            "USER_GUIDE.md",
            "SAMPLES.md"
        };

        public ReadMeComponent(System.Action onBackToMenu)
        {
            _onBackToMenu = onBackToMenu;

            style.flexGrow = 1;

            VisualElement header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.marginBottom = 10;
            Add(header);

            Label titleLabel = new Label("Documentation");
            titleLabel.style.fontSize = 16;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.Add(titleLabel);

            VisualElement dropdownContainer = new VisualElement();
            dropdownContainer.style.flexDirection = FlexDirection.Row;
            header.Add(dropdownContainer);

            _scrollView = new ScrollView();
            _scrollView.style.flexGrow = 1;
            _scrollView.style.marginBottom = 10;

            _contentContainer = new VisualElement();
            _scrollView.Add(_contentContainer);
            Add(_scrollView);

            VisualElement tabContainer = new VisualElement();
            tabContainer.style.flexDirection = FlexDirection.Row;
            tabContainer.style.marginBottom = 5;
            Add(tabContainer);

            foreach (var path in _docPaths)
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(path).Replace("_", " ");
                if (path == "README.md") fileName = "Overview";
                
                Button tabBtn = new Button(() => LoadDocument(path)) { text = fileName };
                tabBtn.style.flexGrow = 1;
                tabBtn.style.height = 30;
                tabBtn.style.marginBottom = 5;
                tabContainer.Add(tabBtn);
            }

            LoadDocument("README.md");

            VisualElement spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            Add(spacer);

            Button backButton = new Button(_onBackToMenu) { text = "Back to Menu" };
            backButton.style.marginTop = 10;
            backButton.style.paddingTop = 8;
            backButton.style.paddingBottom = 8;
            
            Add(backButton);
        }

        private void LoadDocument(string relativePath)
        {
            _contentContainer.Clear();
            _scrollView.scrollOffset = Vector2.zero;

            string fullPath = $"Packages/com.rotterdam.digital-twins/{relativePath}";
            if (!System.IO.File.Exists(fullPath))
            {
                fullPath = relativePath;
            }

            if (System.IO.File.Exists(fullPath))
            {
                string content = System.IO.File.ReadAllText(fullPath);
                ParseAndAddMarkdown(_contentContainer, content);
            }
            else
            {
                _contentContainer.Add(new Label($"Document not found: {relativePath}"));
            }
        }

        private void ParseAndAddMarkdown(VisualElement container, string content)
        {
            string[] lines = content.Split('\n');
            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine))
                {
                    container.Add(new VisualElement { style = { height = 10 } });
                    continue;
                }

                if (trimmedLine.StartsWith("#"))
                {
                    int level = 0;
                    while (level < trimmedLine.Length && trimmedLine[level] == '#') level++;
                    string text = trimmedLine.Substring(level).Trim();
                    
                    Label header = new Label(text);
                    header.style.unityFontStyleAndWeight = FontStyle.Bold;
                    header.style.marginTop = 10;
                    header.style.marginBottom = 5;
                    
                    if (level == 1) header.style.fontSize = 20;
                    else if (level == 2) header.style.fontSize = 18;
                    else header.style.fontSize = 14;
                    
                    container.Add(header);
                }
                else if (trimmedLine.StartsWith("- ") || trimmedLine.StartsWith("* "))
                {
                    VisualElement listItem = new VisualElement();
                    listItem.style.flexDirection = FlexDirection.Row;
                    listItem.style.marginLeft = 15;

                    Label bullet = new Label("• ");
                    Label text = new Label(CleanLine(trimmedLine.Substring(2)));
                    text.style.whiteSpace = WhiteSpace.Normal;
                    text.style.flexGrow = 1;

                    listItem.Add(bullet);
                    listItem.Add(text);
                    container.Add(listItem);
                }
                else if (trimmedLine.StartsWith("---"))
                {
                    VisualElement hr = new VisualElement();
                    hr.style.height = 1;
                    hr.style.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                    hr.style.marginTop = 10;
                    hr.style.marginBottom = 10;
                    container.Add(hr);
                }
                else
                {
                    Label paragraph = new Label(CleanLine(line));
                    paragraph.style.whiteSpace = WhiteSpace.Normal;
                    paragraph.style.marginBottom = 5;
                    container.Add(paragraph);
                }
            }
        }

        private string CleanLine(string line)
        {
            string cleaned = line.Replace("**", "");
            cleaned = cleaned.Replace("__", "");
            cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\[(.*?)\]\((.*?)\)", "$1");
            cleaned = cleaned.Replace("`", "");
            return cleaned.Trim();
        }
    }
}
