using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rotterdam.DigitalTwins.Editor
{
    public class ReadMeComponent : VisualElement
    {
        private readonly System.Action _onBackToMenu;

        public ReadMeComponent(System.Action onBackToMenu)
        {
            _onBackToMenu = onBackToMenu;

            style.flexGrow = 1;

            Label titleLabel = new Label("ReadMe");
            titleLabel.style.fontSize = 16;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.marginBottom = 10;
            Add(titleLabel);

            string readmePath = "Packages/com.rotterdam.digital-twins/README.md";
            if (!System.IO.File.Exists(readmePath))
            {
                readmePath = "README.md";
            }

            string readmeContent = "README.md not found.";
            if (System.IO.File.Exists(readmePath))
            {
                readmeContent = System.IO.File.ReadAllText(readmePath);
            }

            ScrollView scrollView = new ScrollView();
            scrollView.style.flexGrow = 1;
            scrollView.style.marginBottom = 10;

            VisualElement contentContainer = new VisualElement();
            ParseAndAddMarkdown(contentContainer, readmeContent);
            scrollView.Add(contentContainer);
            Add(scrollView);

            Button backButton = new Button(_onBackToMenu) { text = "Back to Menu" };
            backButton.style.marginTop = 10;
            backButton.style.paddingTop = 8;
            backButton.style.paddingBottom = 8;
            Add(backButton);
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
            // Simple removal of common markdown markers for a cleaner look
            string cleaned = line.Replace("**", "");
            cleaned = cleaned.Replace("__", "");
            cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\[(.*?)\]\((.*?)\)", "$1");
            cleaned = cleaned.Replace("`", "");
            return cleaned.Trim();
        }
    }
}
