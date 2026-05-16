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
        private VisualElement _adaptiveLightingOptions;
        private EnumField _rotationModeField;
        private Slider _timeSlider;
        private TextField _timeTextField;
        private FloatField _cycleSecondsField;

        public LocationComponent()
        {
            style.flexGrow = 1;
            Label label = new Label("Set Location with Georeference");
            label.style.fontSize = 16;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            Add(label);

            Label info = new Label("The georeference is used to position the experience in the city. Coordinates are set by default according to the selected Digital Twin or can be customised here.");
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
            setRotterdamButton.text = "Set Georeference to Rotterdam";
            setRotterdamButton.tooltip = "Rotterdam - Wilhelminakade (51.90759, 4.490608, 6.1).";
            setRotterdamButton.style.height = 30;
            setRotterdamButton.style.marginTop = 10;
            setRotterdamButton.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);
            setRotterdamButton.style.borderBottomLeftRadius = 5;
            setRotterdamButton.style.borderBottomRightRadius = 5;
            setRotterdamButton.style.borderTopLeftRadius = 5;
            setRotterdamButton.style.borderTopRightRadius = 5;
            setRotterdamButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            Add(setRotterdamButton);
            
            Label info2 = new Label("Gps map to find the coordinates: https://www.mapcoordinates.net/ ");
            info2.style.whiteSpace = WhiteSpace.Normal;
            info2.style.marginTop = 10;
            info2.style.marginBottom = 10;
            Add(info2);

            Label lightingLabel = new Label("Adaptive Lighting");
            lightingLabel.style.fontSize = 16;
            lightingLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            lightingLabel.style.marginTop = 20;
            Add(lightingLabel);

            Toggle adaptiveLightingToggle = new Toggle("Enable Adaptive Lighting")
            {
                value = SceneControllerUtil.IsAdaptiveLightingEnabled()
            };
            adaptiveLightingToggle.RegisterValueChangedCallback(evt =>
            {
                SceneControllerUtil.SetAdaptiveLighting(evt.newValue);
                UpdateAdaptiveLightingUI();
            });
            Add(adaptiveLightingToggle);

            _adaptiveLightingOptions = new VisualElement();
            _adaptiveLightingOptions.style.marginLeft = 15;
            Add(_adaptiveLightingOptions);

            var sunRotation = SceneControllerUtil.GetSunRotation();

            _rotationModeField = new EnumField("Rotation Mode", sunRotation != null ? sunRotation.mode : SunRotation.RotationMode.SpecificTime);
            _rotationModeField.RegisterValueChangedCallback(evt =>
            {
                SceneControllerUtil.SetSunRotationMode((SunRotation.RotationMode)evt.newValue);
                UpdateAdaptiveLightingUI();
            });
            _adaptiveLightingOptions.Add(_rotationModeField);

            // Specific Time UI
            VisualElement specificTimeContainer = new VisualElement();
            _timeSlider = new Slider("Time of Day", 0, 24) { value = sunRotation != null ? sunRotation.timeOfDay : 12f };
            _timeSlider.RegisterValueChangedCallback(evt =>
            {
                SceneControllerUtil.SetSunRotationTime(evt.newValue);
                _timeTextField.SetValueWithoutNotify(ConvertFloatToTime(evt.newValue));
            });
            specificTimeContainer.Add(_timeSlider);

            _timeTextField = new TextField("Time (HH:mm)") { value = ConvertFloatToTime(_timeSlider.value) };
            _timeTextField.RegisterValueChangedCallback(evt =>
            {
                if (TryParseTime(evt.newValue, out float time))
                {
                    SceneControllerUtil.SetSunRotationTime(time);
                    _timeSlider.SetValueWithoutNotify(time);
                }
            });
            specificTimeContainer.Add(_timeTextField);
            _adaptiveLightingOptions.Add(specificTimeContainer);

            VisualElement continuousContainer = new VisualElement();
            float initialCycle = 10f;
            if (sunRotation != null && sunRotation.rotationSet.x != 0)
            {
                initialCycle = 0.5f / sunRotation.rotationSet.x;
            }
            _cycleSecondsField = new FloatField("Cycle in Seconds") { value = initialCycle };
            _cycleSecondsField.RegisterValueChangedCallback(evt =>
            {
                SceneControllerUtil.SetSunRotationCycle(evt.newValue);
            });
            continuousContainer.Add(_cycleSecondsField);
            _adaptiveLightingOptions.Add(continuousContainer);

            UpdateAdaptiveLightingUI();
        }

        private void UpdateAdaptiveLightingUI()
        {
            bool enabled = SceneControllerUtil.IsAdaptiveLightingEnabled();
            _adaptiveLightingOptions.style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;

            if (enabled)
            {
                var sunRotation = SceneControllerUtil.GetSunRotation();
                if (sunRotation != null)
                {
                    bool isSpecificTime = sunRotation.mode == SunRotation.RotationMode.SpecificTime;
                    _adaptiveLightingOptions.ElementAt(1).style.display = isSpecificTime ? DisplayStyle.Flex : DisplayStyle.None;
                    _adaptiveLightingOptions.ElementAt(2).style.display = !isSpecificTime ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }
        }

        private string ConvertFloatToTime(float value)
        {
            int hours = Mathf.FloorToInt(value);
            int minutes = Mathf.FloorToInt((value - hours) * 60);
            return $"{hours:D2}:{minutes:D2}";
        }

        private bool TryParseTime(string timeStr, out float time)
        {
            time = 0;
            var parts = timeStr.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[0], out int hours) && int.TryParse(parts[1], out int minutes))
            {
                if (hours >= 0 && hours < 24 && minutes >= 0 && minutes < 60)
                {
                    time = hours + (minutes / 60f);
                    return true;
                }
            }
            return false;
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