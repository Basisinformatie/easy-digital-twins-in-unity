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

            bool isForked = CesiumSetupService.IsForkInstalled();
            string buttonText = isForked ? "Restore Official Cesium" : "Apply Instancing Patch (Fork)";
            Button patchButton = new Button(() => OnPatchClicked(isForked)) { text = buttonText };
            patchButton.style.paddingTop = 6;
            patchButton.style.paddingBottom = 6;
            experimentalSection.Add(patchButton);
        }

        private void OnPatchClicked(bool isForked)
        {
            string title = isForked ? "Restore Official Cesium" : "Apply Experimental Patch";
            string message = isForked 
                ? "This will restore the official Cesium Registry version. Unity will re-import the package, which may take several minutes. Continue?"
                : "This will replace the official Cesium package with a community-patched version from GitHub. \n\nWARNING: This is an experimental feature. \n\nContinue?";

            if (EditorUtility.DisplayDialog(title, message, "Yes", "Cancel"))
            {
                if (isForked)
                    CesiumSetupService.InstallOfficialCesium();
                else
                    CesiumSetupService.InstallForkedCesium();
            }
        }
    }
}