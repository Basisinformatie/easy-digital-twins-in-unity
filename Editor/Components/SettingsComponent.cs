using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Rotterdam.DigitalTwins.Editor.Setup;

namespace Rotterdam.DigitalTwins.Editor
{
    public class SettingsComponent : VisualElement
    {
        private readonly System.Action _onBackToMenu;

        public SettingsComponent(System.Action onBackToMenu)
        {
            _onBackToMenu = onBackToMenu;

            Label titleLabel = new Label("Settings");
            titleLabel.style.fontSize = 16;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.marginBottom = 10;
            Add(titleLabel);

            Label placeholderLabel = new Label("Dit is het settings component placeholder bericht (geladen uit SettingsComponent.cs).");
            placeholderLabel.style.marginBottom = 20;
            placeholderLabel.style.whiteSpace = WhiteSpace.Normal;
            Add(placeholderLabel);

            Button backButton = new Button(_onBackToMenu) { text = "Terug naar Menu" };
            backButton.style.marginTop = 10;
            backButton.style.paddingTop = 8;
            backButton.style.paddingBottom = 8;
            Add(backButton);

            // Experimental Section
            VisualElement experimentalSection = new VisualElement();
            experimentalSection.style.marginTop = 30;
            experimentalSection.style.paddingTop = 10;
            experimentalSection.style.borderTopWidth = 1;
            experimentalSection.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f);
            Add(experimentalSection);

            Label experimentalTitle = new Label("Experimental: Patched Cesium");
            experimentalTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            experimentalTitle.style.color = new Color(1f, 0.5f, 0f);
            experimentalSection.Add(experimentalTitle);

            Label disclaimer = new Label("Cesium for Unity has a known bug with instanced tiles (I3DM) positioning. A community patch is available from 360Fabriek, but it must be built manually from source (C++) to work correctly.");
            disclaimer.style.whiteSpace = WhiteSpace.Normal;
            disclaimer.style.fontSize = 11;
            disclaimer.style.marginTop = 5;
            disclaimer.style.marginBottom = 10;
            disclaimer.style.color = new Color(0.8f, 0.8f, 0.8f);
            experimentalSection.Add(disclaimer);

            bool isForked = CesiumSetupService.IsForkInstalled();
            if (isForked)
            {
                Button restoreButton = new Button(() => OnRestoreClicked()) { text = "Restore Official Cesium (Fix Errors)" };
                restoreButton.style.paddingTop = 6;
                restoreButton.style.paddingBottom = 6;
                restoreButton.style.backgroundColor = new Color(0.3f, 0.1f, 0.1f);
                experimentalSection.Add(restoreButton);
                
                Label statusLabel = new Label("Currently using a non-registry version of Cesium.");
                statusLabel.style.fontSize = 10;
                statusLabel.style.marginTop = 5;
                statusLabel.style.color = new Color(0.5f, 0.8f, 0.5f);
                experimentalSection.Add(statusLabel);
            }
            else
            {
                Button buildButton = new Button(() => CesiumPatchService.StartPatchBuild()) { text = "Automated Build & Apply Patch" };
                buildButton.style.paddingTop = 6;
                buildButton.style.paddingBottom = 6;
                buildButton.style.backgroundColor = new Color(0.1f, 0.3f, 0.1f);
                experimentalSection.Add(buildButton);
                
                Label infoLabel = new Label("This will attempt to clone, build and install the patch automatically. Requires git, dotnet, and cmake.");
                infoLabel.style.fontSize = 10;
                infoLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
                infoLabel.style.marginTop = 5;
                experimentalSection.Add(infoLabel);
            }
        }

        private void OnRestoreClicked()
        {
            if (EditorUtility.DisplayDialog("Restore Official Cesium", 
                "This will attempt to restore the official Cesium Registry version to fix compilation errors. Unity will re-import the package. Continue?", "Yes", "Cancel"))
            {
                CesiumSetupService.InstallOfficialCesium();
            }
        }
    }
}