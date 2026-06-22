### User Interface Guide

This guide provides an overview of the "Easy Digital Twins" toolkit interface in Unity.

#### Main Menu
The entry point for the toolkit, accessible via `Rotterdam Digital Twins > Launch UI`.

- **START**: Opens the Shopping Wizard to configure your scene.
- **Settings**: Configure project detection, graphics presets, and web deployment.
- **ReadMe**: View the project's documentation directly in Unity.
- **Samples**: Explore sample scenes and implementations.

---

#### Shopping Wizard (Tabs)
After pressing START: the Shopping Wizard is divided into three main functional tabs:

##### 1. Data Catalogue
Search and add live city data from the Open Urban Platform (OUP).
- **Catalog Dropdown**: Switch between different data providers (e.g., OUP, Mock).
- **Type Dropdown**: Filter between "Digital Twins" and "Datasets".
- **Search Field**: Find data by title, description, or tags.
- **Hub/Category Dropdown**: Filter results by specific organizational hubs or categories.
- **Only supported Resources**: When enabled, hides entries that don't have compatible 3D formats (3D Tiles, WMS, etc.).
- **Add to Scene**: Click on a result to instantly add it to your Unity scene.

##### 2. Controller
Select how you want to navigate your Digital Twin.
- **Controller Type**: Choose between:
  - **None**: Uses a standard Main Camera.
  - **First Person**: Standard walking character.
  - **Third Person**: Character with a following camera.
  - **Car**: Driveable vehicle controller.
  - **Helicopter**: Flyable helicopter controller.
- *Note: Selecting a new type automatically replaces the existing controller in your scene.*

##### 3. Location
Define where your experience is situated in the world.
- **Latitude / Longitude / Height**: Manually set the georeference coordinates.
- **Set Georeference to Rotterdam**: Quickly reset the location to the center of Rotterdam.
- **Adaptive Lighting**: Enable dynamic lighting based on time.
  - **Rotation Mode**: Choose between "Specific Time" or "Full Day Cycle".
  - **Time Slider**: Manually adjust the time of day.

---

#### Settings
- **Project Detection**: Automatically detects if you are targeting VR, Web, or Mobile.
- **Graphics & Performance**: Apply presets (Low/Medium/High) to optimize 3D Tileset loading and camera clipping.
- **Web Deployment**: Specific optimizations for WebGL builds.
