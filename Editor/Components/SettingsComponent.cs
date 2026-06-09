using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Rotterdam.DigitalTwins.Editor.Setup;
using Rotterdam.DigitalTwins.Editor.Utilities;

namespace Rotterdam.DigitalTwins.Editor
{
    public class SettingsComponent : VisualElement
    {
        private readonly System.Action _onBackToMenu;
        private VisualElement _webSection;

        public SettingsComponent(System.Action onBackToMenu)
        {
            _onBackToMenu = onBackToMenu;

            Label titleLabel = new Label("Settings");
            titleLabel.style.fontSize = 16;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.marginBottom = 10;
            Add(titleLabel);

            Label placeholderLabel = new Label("Settings Menu.");
            placeholderLabel.style.marginBottom = 20;
            placeholderLabel.style.whiteSpace = WhiteSpace.Normal;
            Add(placeholderLabel);
            
            // Project Type Detection Section
            VisualElement projectSection = new VisualElement();
            projectSection.style.marginTop = 20;
            projectSection.style.paddingTop = 10;
            projectSection.style.borderTopWidth = 1;
            projectSection.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f);
            Add(projectSection);

            Label projectTitle = new Label("Project Detection");
            projectTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            projectSection.Add(projectTitle);

            Label projectTypeLabel = new Label("Detecting project type...");
            projectTypeLabel.style.whiteSpace = WhiteSpace.Normal;
            projectTypeLabel.style.marginTop = 5;
            projectSection.Add(projectTypeLabel);

            this.RegisterCallback<AttachToPanelEvent>(evt => RefreshDetection(projectTypeLabel));
           
            Label projectInfoLabel = new Label("The detection is only indicative and based on available build profiles and installed packages. Refer to the documentation for more information on how to set up your project and trouble shooting.");
            projectInfoLabel.style.fontSize = 10;
            projectInfoLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
            projectInfoLabel.style.whiteSpace = WhiteSpace.Normal;
            projectSection.Add(projectInfoLabel);

            // Graphics & Performance Section
            VisualElement graphicsSection = new VisualElement();
            graphicsSection.style.marginTop = 20;
            graphicsSection.style.paddingTop = 10;
            graphicsSection.style.borderTopWidth = 1;
            graphicsSection.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f);
            Add(graphicsSection);

            Label graphicsTitle = new Label("Graphics & Performance");
            graphicsTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            graphicsSection.Add(graphicsTitle);

            Label graphicsDesc = new Label("Configure 3D Tilesets and Camera settings for the selected target.");
            graphicsDesc.style.fontSize = 10;
            graphicsDesc.style.color = new Color(0.7f, 0.7f, 0.7f);
            graphicsDesc.style.marginBottom = 5;
            graphicsSection.Add(graphicsDesc);

            void AddPresetButton(string text, GraphicsSettingsService.GraphicsPreset preset)
            {
                Button btn = new Button(() => GraphicsSettingsService.ApplyPreset(preset)) { text = text };
                btn.style.marginTop = 2;
                btn.style.marginBottom = 2;
                graphicsSection.Add(btn);
            }

            AddPresetButton("No Preset / Default", GraphicsSettingsService.GraphicsPreset.None);
            AddPresetButton("Low (VR, AR)", GraphicsSettingsService.GraphicsPreset.Low);
            AddPresetButton("Medium (Desktop, Android, Web)", GraphicsSettingsService.GraphicsPreset.Medium);
            AddPresetButton("High (Desktop, High End)", GraphicsSettingsService.GraphicsPreset.High);

            // Web Deployment Section
            _webSection = new VisualElement();
            _webSection.style.marginTop = 20;
            _webSection.style.paddingTop = 10;
            _webSection.style.borderTopWidth = 1;
            _webSection.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f);
            _webSection.style.display = DisplayStyle.None; 
            Add(_webSection);

            Label webTitle = new Label("Web Deployment");
            webTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            _webSection.Add(webTitle);

            Label webDesc = new Label("Optimize project settings for WebGL builds (Brotli, 1024MB RAM, Wasm 2023).");
            webDesc.style.fontSize = 10;
            webDesc.style.color = new Color(0.7f, 0.7f, 0.7f);
            webDesc.style.marginBottom = 5;
            webDesc.style.whiteSpace = WhiteSpace.Normal;
            _webSection.Add(webDesc);

            Button webConfigButton = new Button(WebDeploymentService.ConfigureForWebDeployment) { text = "Configure for web deployment" };
            webConfigButton.style.marginTop = 2;
            webConfigButton.style.marginBottom = 2;
            _webSection.Add(webConfigButton);

            Button backButton = new Button(_onBackToMenu) { text = "Back to Main Menu" };
            backButton.style.marginTop = 10;
            backButton.style.paddingTop = 8;
            backButton.style.paddingBottom = 8;
            Add(backButton);
            
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

        private void RefreshDetection(Label label)
        {
            label.text = "Detecting project type...";
            ProjectTypeDetector.GetProjectType((type) => {
                label.text = $"Project type: {ProjectTypeDetector.GetProjectTypes(type)}\n" +
                             $"Compatibility: {ProjectTypeDetector.GetCompatibility(type)}";
                
                _webSection.style.display = type.HasFlag(ProjectTypeDetector.ProjectType.WebApp) 
                    ? DisplayStyle.Flex 
                    : DisplayStyle.None;
            });
        }
    }
}