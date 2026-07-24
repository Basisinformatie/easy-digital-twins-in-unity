# Included sample features
- A functional Boat controller, made through a small adjustment on the Car controller.
- Showing building data in a UI.
- Painting on buildings.
- Deleting meshes.
- Ground snap.
- Teleportation on 3D Tiles (for VR).
- Y-axis mover script.
- Controller support and remappable keys.

### Boat controller
A simple boat controller is provided, based on a model adjustment of the Car controller. You can find it in the folder: Runtime > Prefabs > Features > Boat. The prefab is called CustomBoatControllerRig and can be used as-is. It is a standalone prefab that can be dragged straight into the Hierarchy or Scene window.

### Building Selector
The BuildingSelector is a script that shows address information or the name of bridges in a UI when the user clicks with the mouse. It can be added as a component to a controller.

### Mesh Painter
The MeshPainter lets users draw or paint directly onto buildings. It can be added as a component to a controller.

### Building Deleter
The Building Deleter makes it possible to make meshes disappear at runtime. It can be added as a component to a controller.

### Ground Snap
When added as a component to an object, the Ground Snap script will place that object on top of the surface beneath it (with a small offset).

### Cesium Teleporter
The landscape can be made teleportable for VR navigation by adding the custom made Cesium Teleporter component.

### Y-Axis Mover
A script that lets you dynamically raise or lower an object along the Y-axis — for example, to change the water level at runtime.

### Gamepad and joystick support
Gamepad and joystick support is integrated into the custom controller, alongside the standard XR and keyboard-and-mouse controls. You can remap keys directly in the UI, or check the scripts for more options. For controller support in web builds, I recommend following a dedicated guide, since it works slightly differently.

### CesiumIon
Although not a feature of the Toolkit itself, CesiumIon is compatible. Meaning you can import maps and different tilesets from google, bing and other open services using CesiumIon.
