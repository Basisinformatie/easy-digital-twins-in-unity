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

            Label disclaimer = new Label("Cesium for Unity has a known bug with instanced tiles (I3DM) positioning. A community patch is available but not yet officially merged. Swapping to this patch will fix instancing but requires a full re-import of the Cesium package.");
            disclaimer.style.whiteSpace = WhiteSpace.Normal;
            disclaimer.style.fontSize = 11;
            disclaimer.style.marginTop = 5;
            disclaimer.style.marginBottom = 10;
            disclaimer.style.color = new Color(0.8f, 0.8f, 0.8f);
            experimentalSection.Add(disclaimer);

            bool isLocal = CesiumPatchService.IsLocalPackageInstalled();
            bool isBuilding = CesiumPatchService.IsBuilding;

            Label statusLabel = new Label(isLocal ? "Current: Patched (Local Build)" : "Current: Official Registry");
            statusLabel.style.fontSize = 11;
            statusLabel.style.marginBottom = 5;
            experimentalSection.Add(statusLabel);

            if (isBuilding)
            {
                Label buildingLabel = new Label("Building... Please wait (10-20 min)");
                buildingLabel.style.color = Color.yellow;
                experimentalSection.Add(buildingLabel);
            }
            else
            {
                Button buildButton = new Button(CesiumPatchService.BuildAndApplyPatch) 
                { 
                    text = isLocal ? "Rebuild & Update Patch" : "Automated Build & Apply Patch" 
                };
                buildButton.style.paddingTop = 6;
                buildButton.style.paddingBottom = 6;
                experimentalSection.Add(buildButton);

                if (isLocal)
                {
                    Button restoreButton = new Button(CesiumSetupService.InstallOfficialCesium) 
                    { 
                        text = "Restore Official Cesium" 
                    };
                    restoreButton.style.marginTop = 5;
                    restoreButton.style.paddingTop = 4;
                    restoreButton.style.paddingBottom = 4;
                    experimentalSection.Add(restoreButton);
                }
            }
        }

    }
}