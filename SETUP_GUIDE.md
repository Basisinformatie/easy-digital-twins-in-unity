# Setup Guide

## a. Prerequisite

### Unity Engine
The toolkit is optimized for Unity 6 (version 6000.3.9f). While it is compatible with earlier and probably later versions, some implementation steps (for example, deployment to specific platforms) may vary.
- **Download Unity Hub:** [https://unity.com/](https://unity.com/)
- **Unity Hub installation support documentation:** [https://docs.unity.com/en-us/hub](https://docs.unity.com/en-us/hub)

### Git (Recommended)
While not strictly required, using Git is highly recommended for easy updates and version management within your project.
- **Download Git:** [https://git-scm.com/install/](https://git-scm.com/install/)

---

## b. Installation

The toolkit is designed to work with the following Unity templates:
- Universal 3D (URP/SRP)
- AR Mobile
- VR
- Multiplayer variants of the above

### Adding the Package:
1. Open your Unity project.
2. Open the Package Manager (Window > Package Manager).
3. Click the plus (+) button in the top-left corner.
4. Select "Add package from git URL...".
5. Enter the following URL: `https://github.com/Basisinformatie/easy-digital-twins-in-unity.git`
6. Wait for the installation to complete.

Alternatively, you can download the zip from the npm branch from the repository on github, unpack it, and import the package manually.

---

## c. Initial Launch

Once installed, a new menu item will appear in the Unity top menu:
1. Navigate to Rotterdam Digital Twins > Launch UI.
2. The Toolkit Window will open and automatically begin installing necessary dependencies, such as Cesium for Unity.
3. Note on popups: You may see warnings regarding a "scoped registry" or a "missing signature" related to the Cesium plugin. These are expected and can safely be disregarded and closed.
4. Once the setup is complete, the loading animation will stop, the Main Menu functions will be unlocked and you can begin using the toolkit.

---

## d. Hardware and Storage Requirements

Depending on your target platform (Web, AR, VR, etc.), additional modules may be required.

### Development and Build Space
- Standard Development: less than 5 GB
- VR Multiplayer Module: approximately 25 GB
- AR Multiplayer Module: approximately 12 GB
- iOS Deployment (Xcode and temporary storage): approximately 20 GB

### Runtime Storage (3D Tiles and Caching)
The toolkit uses a 3D Tile system (via Cesium) to stream high-detail environments.
- Digital Twin Loading: Averages between 2 GB and 8 GB of temporary disk space, depending on the resource complexity.
- Mobile / Standalone Optimization: For devices with limited memory (for example, the Meta Quest 3 with a 512 MB cache), the toolkit includes performance optimization settings. For complex applications, you may need to manually tune the mipmap configuration, caching, and tile loading to fit within your device's limits.
