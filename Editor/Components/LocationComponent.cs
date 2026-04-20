using UnityEngine;
using UnityEngine.UIElements;

namespace Rotterdam.DigitalTwins.Editor
{
    public class LocationComponent : VisualElement
    {
        public LocationComponent()
        {
            style.flexGrow = 1;
            Label label = new Label("Locatie & Omgeving");
            label.style.fontSize = 16;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            Add(label);

            Label info = new Label("Automatische instelling van CesiumGeoreference, zonpositie en een tijdelijk plateau.");
            info.style.whiteSpace = WhiteSpace.Normal;
            info.style.marginTop = 10;
            info.style.marginBottom = 10;
            Add(info);

            Button setRotterdamButton = new Button(() => CesiumSceneHelper.SetGeoreferenceToRotterdam());
            setRotterdamButton.text = "Zet Georeference naar Rotterdam";
            setRotterdamButton.tooltip = "Zet de CesiumGeoreference naar de coördinaten van Rotterdam (51.90759, 4.490608, 6.1).";
            setRotterdamButton.style.height = 30;
            setRotterdamButton.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);
            setRotterdamButton.style.borderBottomLeftRadius = 5;
            setRotterdamButton.style.borderBottomRightRadius = 5;
            setRotterdamButton.style.borderTopLeftRadius = 5;
            setRotterdamButton.style.borderTopRightRadius = 5;
            setRotterdamButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            Add(setRotterdamButton);
        }
    }
}