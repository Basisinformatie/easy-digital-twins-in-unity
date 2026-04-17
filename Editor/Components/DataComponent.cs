using UnityEngine;
using UnityEngine.UIElements;

namespace Rotterdam.DigitalTwins.Editor
{
    public class DataComponent : VisualElement
    {
        public DataComponent()
        {
            style.flexGrow = 1;
            Label label = new Label("Data Shopping");
            label.style.fontSize = 16;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            Add(label);

            Label info = new Label("Kies Digital Twin of specifiek dataset-type.");
            info.style.whiteSpace = WhiteSpace.Normal;
            info.style.marginTop = 10;
            Add(info);
        }
    }
}