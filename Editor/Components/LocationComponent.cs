using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Rotterdam.DigitalTwins.Editor
{
    public class LocationComponent : VisualElement
    {
        private DoubleField _latField;
        private DoubleField _lonField;
        private DoubleField _heightField;

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

            var coords = CesiumSceneHelper.GetGeoreference();

            _latField = new DoubleField("Latitude") { value = coords.lat };
            _latField.RegisterValueChangedCallback(evt => UpdateGeoreference());
            Add(_latField);

            _lonField = new DoubleField("Longitude") { value = coords.lon };
            _lonField.RegisterValueChangedCallback(evt => UpdateGeoreference());
            Add(_lonField);

            _heightField = new DoubleField("Height") { value = coords.height };
            _heightField.RegisterValueChangedCallback(evt => UpdateGeoreference());
            Add(_heightField);

            Button setRotterdamButton = new Button(() =>
            {
                CesiumSceneHelper.SetGeoreferenceToRotterdam();
                UpdateFieldsFromScene();
            });
            setRotterdamButton.text = "Zet Georeference naar Rotterdam";
            setRotterdamButton.tooltip = "Zet de CesiumGeoreference naar de coördinaten van Rotterdam (51.90759, 4.490608, 6.1).";
            setRotterdamButton.style.height = 30;
            setRotterdamButton.style.marginTop = 10;
            setRotterdamButton.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);
            setRotterdamButton.style.borderBottomLeftRadius = 5;
            setRotterdamButton.style.borderBottomRightRadius = 5;
            setRotterdamButton.style.borderTopLeftRadius = 5;
            setRotterdamButton.style.borderTopRightRadius = 5;
            setRotterdamButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            Add(setRotterdamButton);
        }

        private void UpdateGeoreference()
        {
            CesiumSceneHelper.SetGeoreference(_latField.value, _lonField.value, _heightField.value);
        }

        private void UpdateFieldsFromScene()
        {
            var coords = CesiumSceneHelper.GetGeoreference();
            _latField.SetValueWithoutNotify(coords.lat);
            _lonField.SetValueWithoutNotify(coords.lon);
            _heightField.SetValueWithoutNotify(coords.height);
        }
    }
}