using UnityEngine;
using UnityEngine.UIElements;

namespace Rotterdam.DigitalTwins.Editor
{
    public class ControllerComponent : VisualElement
    {
        public ControllerComponent()
        {
            style.flexGrow = 1;
            Label label = new Label("Experience & Controller");
            label.style.fontSize = 16;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            Add(label);

            Label info = new Label("Kies uit VR, AR of Desktop prefabs.");
            info.style.whiteSpace = WhiteSpace.Normal;
            info.style.marginTop = 10;
            Add(info);
        }
    }
}