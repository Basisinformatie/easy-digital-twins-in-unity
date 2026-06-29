# Getting Started / Setup Guide

This guide will help you install and configure the Easy Digital Twins Toolkit in your Unity project.

## 1. Prerequisites

### Unity Engine
The toolkit is optimized for **Unity 6 (6000.39 or higher)**. While it is compatible with earlier versions, some implementation steps (e.g., deployment to specific platforms) may vary.
- **Download:** [Unity Hub](https://unity.com/)
- **Documentation:** [Unity Hub Installation Support](https://docs.unity.com/en-us/hub)

### Git (Recommended)
While not strictly required, using Git is highly recommended for easy updates and version management within your project.
- **Download:** [Git SCM](https://git-scm.com/install/)

---

## 2. Installation

### Project Templates
The toolkit is designed to work with the following Unity templates:
- Universal 3D (URP)
- AR Mobile
- VR
- Multiplayer variants of the above

### Adding the Package
1. Open your Unity project.
2. Open the **Package Manager** (`Window > Package Manager`).
3. Click the **+** button in the top-left corner.
4. Select **"Add package from git URL..."**.
5. Enter the following URL:
   `https://github.com/Basisinformatie/easy-digital-twins-in-unity.git`
6. Wait for the installation to complete.

Alternatively, you can download the zip from npm branch, unpack it and import the package manually.

---

## 3. Initial Launch

Once installed, a new menu item will appear in the Unity top menu:
1. Navigate to **Rotterdam Digital Twins > Launch UI**.
2. The **Toolkit Window** will open and automatically begin installing necessary dependencies, such as **Cesium for Unity**.
3. **Note on Popups:** You may see warnings regarding a "scoped registry" or "missing signature" related to the Cesium plugin. These are expected; you can safely disregard and close them.
4. Once the setup is complete, the Main Menu functions will be unlocked, and you can begin using the toolkit.

---

## 4. Hardware & Storage Requirements

Depending on your target platform (Web, AR, VR, etc.), additional modules may be required.

### Development & Build Space
- **Standard Development:** < 5GB
- **VR Multiplayer Module:** ~25GB
- **AR Multiplayer Module:** ~12GB
- **iOS Deployment (Xcode & temporary storage):** ~20GB

### Runtime Storage (3D Tiles & Caching)
The toolkit utilizes a 3D Tile system (via Cesium) to stream high-detail environments.
- **Digital Twin Loading:** Averages **2GB to 8GB** of temporary disk space depending on the resource complexity.
- **Mobile/Standalone Optimization:** For devices with limited memory (e.g., Meta Quest 3 with 512MB cache), the toolkit includes performance optimization settings. For complex applications, you may need to manually tune mipmap configurations, caching, and tile loading to fit within your device's limits.
