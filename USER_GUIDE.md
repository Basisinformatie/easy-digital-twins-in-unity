# Usage Guide

## a. Toolkit UI Overview
This section provides an overview of the "Easy Digital Twins" toolkit interface in Unity.

### Main Menu
The entry point for the toolkit, accessible via Rotterdam Digital Twins > Launch UI.
- START: Opens the Shopping Wizard to configure your scene.
- Settings: Configure project detection, graphics presets, and web deployment.
- ReadMe: View the project's documentation directly in Unity.
- Samples: Explore sample scenes and implementations.

### Shopping Wizard (Tabs)
After pressing START, the Shopping Wizard contains three tabs for switching between the main functionalities of the Toolkit.

#### Data Catalogue
Search and add live city data from the Open Urban Platform (OUP).
- Catalog Dropdown: Switch between different data providers (for example, OUP or Mock).
- Type Dropdown: Filter between "Digital Twins" and "Datasets". A Digital Twin is a collection of resources with metadata and preconfigured by the owner. A Dataset contains a specific resource, sometimes options for different variants of resources within the same dataset are offered.
- Search Field: Find data by title, description, or tags.
- Hub / Category Dropdown: Filter results by specific organizational hubs or categories.
- Only Supported Resources: When enabled, hides entries that do not have compatible 3D formats (3D Tiles, WMS, etc.).
- Add to Scene: Click on a result to instantly add it to your Unity scene.

#### Controller
Select how you want to navigate your Digital Twin.
Controller Type — choose between:
- None: Uses a standard Main Camera.
- First Person: Standard walking character.
- Third Person: Character with a following camera.
- Car: Driveable vehicle controller.
- Helicopter: Flyable helicopter controller.
Note: Selecting a new type automatically replaces the existing controller in your scene and adds a platform that prevents the controller from falling through the environment during loading.

#### Location
Define where your experience is situated in the world.
- Latitude / Longitude / Height: Manually set the georeference coordinates.
- Set Georeference to Rotterdam: Quickly reset the location to the center of Rotterdam.
- Adaptive Lighting: Enable dynamic lighting based on time. 
    - Rotation Mode: Choose between "Specific Time" or "Full Day Cycle".
    - Time Slider: Manually adjust the time of day.

#### Settings
- Project Detection: Automatically detects whether you are targeting VR, Web, or Mobile.
- Graphics and Performance: Apply presets (Low, Medium, High) to optimize 3D Tileset loading and camera clipping.
- Web Deployment: Specific optimizations for WebGL builds.

---

## b. Step by Step example usecase
Let’s get to know the toolkit by making a simple 3D flyer experience.

**Step 1. Press START in the toolkit window and let’s take a look at the Data Catalogue:**
When we open de Shopping Wizard, through pressing START, it opens de first tab which is the Data Catalogue. By default the selected Catalog is set to Open Urban Platform, Type is set to Digital Twins and the checkbox is set to show Only supported Resources. After a short loading duration we can browse the available results, each result is displayed in a Card Component containing: a preview if available, blue information icon which directs you to the clearlyhub page, the name, the publishing hub and a button to add.

**Step 2. Lets add our first Digital Twin to our experience:**
Filter on hub and set it to Hub van Joris Koolen.
Locate the result Bombarie op de Boompjes and press the green Add button.

In the Hierarchy Window a CesiumGeoreference object has been created. This object contains more objects, these are Cesium 3D Tilesets which build the different resources or layers that make up the meshes of the Digital Twin.

Note: When developing it is advised to only configure the Georeference and 3D tiles, when making or adding your own objects do this outside/seperate of the CesiumGeoreference object hierarchy.

**Step 3. Removing and adding individual resources:**
In the Scene View we can see that the Terrain is not loaded correctly, this can have a multitude of reasons – for now we will delete it.* Selecting the Terrain 3D Tileset we can uncheck it in the Inspector Window or simply delete it. We can repeat this step for the Tileset with the Trees. 
(*note: this Digital Twin also contains a topographical terrain element, BGT which will act as our Terrain instead)

Reopening the Data Catalogue, set Type to Datasets, set the hub to 3D Rotterdam and locate the dataset 3D Rotterdam – Bridges, this has two resources Add one of them.

**Step 4. Adding a controller:**
Now that the experience is populated with a map, buildings, bridges and some custom shapes we will want to move through it using a controller. Navigate to the Controller tab in the Shopping Wizard. Select the Helicopter in the Controller Type dropdown menu. This removes the Camera object, places a StartingPlatform and places a Helicopter-Rig prefab which contains a new Camera object. 
- By default a Camera with a MainCamera tag is always necessary to load 3D tiles correctly. Based on radius around it and looking distance the 3D tilesets are visualised dynamically.
- A StartingPlatform is placed so the controller doesn’t fall through the floor during 3D tile loading. It contains a Ground Snap script which places it on top of the mesh/floor below it.
- The Helicopter-Rig is placed just above the StartingPlatform. Make sure to double check no clipping occurs and manually reposition if necessary. This controller has keybindings which can be found and adjusted in the Custom Helicoper Controller script or directly in the Inspector Window.

Optional: You can add an extra feature to the Helicopter-Rig that show building addresses or bridge names. In the Inspector Window press add component or simply drag and drop the script Building Selector on the object. 

When a controller/rig is added through the toolkit it will remove any objects it 

**Step 5. Georeferencing and Location**
Navigate to the Location tab in the Shopping Wizard. Here you can change the gps coordinates where your experience should take place through Latitude, Longitude and Height. When you have added a Digital Twin the location is usually already set but you can overwrite this to your liking. There is also a Set Georeference to Rotterdam Button which takes you to a default location op Laan op Zuid. Using a gps website like www.mapcoordinates.net you can pick a location on a map and configure them yourself.

With the adaptive lighting feature checkbox checked it adds a simple skybox with directive lighting to the environement. Now you can select a a specific time or make it continuous. 
Note: This is an optional aeshetical feature and not accurate to the actual sun position timings or orbit.

**Step 6. Testing our experience in the editor**
Pressing the play button above the Scene view lets you run the experience and fly through the city.
Controls are with WASD, SPACE for more power and SHIFT for less. Alternatively you could use a gamepad. 
When you have added the Building Selector you can also click on a building or a bridge to show more information about it.

A demo of a similar experience made with the Toolkit deployed for web can be tried here: https://hub.clearly.app/apps/6a2ffd3d6b336761f2c4aba4/details 

---

## c. Samples and Extra Features

The toolkit comes with a variety of small features and samples that are not part of the core functionality. However, they can be fun to use or useful as a starting point to build upon. Some of these features are meant to showcase how something is implemented, giving you a better understanding of what is possible and how to write your own scripting. In some cases they may be directly useful for your own usecase. All assets can be found in Easy Digital Twin Toolkit Folder in the Project Window.

For more details on specific samples, please refer to the [Samples Guide](SAMPLES.md) or click the **Samples** button in the Main Menu.

---
