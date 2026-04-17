using System.Collections.Generic;
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

            Label info = new Label("Choose a controller type to use in the scene.");
            info.style.whiteSpace = WhiteSpace.Normal;
            info.style.marginTop = 10;
            info.style.marginBottom = 15;
            Add(info);

            List<string> options = new List<string> { "None", "First Person", "Third Person" };
            string currentType = SceneControllerUtil.GetCurrentControllerType();
            
            DropdownField dropdown = new DropdownField("Controller Type", options, currentType);
            dropdown.RegisterValueChangedCallback(evt =>
            {
                switch (evt.newValue)
                {
                    case "First Person":
                        SceneControllerUtil.ReplaceController("FirstPerson-Rig");
                        break;
                    case "Third Person":
                        SceneControllerUtil.ReplaceController("ThirdPerson-Rig");
                        break;
                    case "None":
                    default:
                        SceneControllerUtil.RemoveExistingControllers();
                        break;
                }
            });

            Add(dropdown);
        }
    }
}