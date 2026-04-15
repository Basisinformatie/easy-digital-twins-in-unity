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

            string readmeContent = "README.md niet gevonden.";
            if (System.IO.File.Exists(readmePath))
            {
                readmeContent = System.IO.File.ReadAllText(readmePath);
            }

            ScrollView scrollView = new ScrollView();
            scrollView.style.flexGrow = 1;
            scrollView.style.marginBottom = 10;

            Label contentLabel = new Label(readmeContent);
            contentLabel.style.whiteSpace = WhiteSpace.Normal;
            scrollView.Add(contentLabel);
            Add(scrollView);

            Button backButton = new Button(_onBackToMenu) { text = "Terug naar Menu" };
            backButton.style.marginTop = 10;
            backButton.style.paddingTop = 8;
            backButton.style.paddingBottom = 8;
            Add(backButton);
        }
    }
}
