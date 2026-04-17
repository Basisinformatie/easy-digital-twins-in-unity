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
            Add(info);
        }
    }
}