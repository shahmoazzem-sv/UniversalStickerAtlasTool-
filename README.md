# 🌟 Universal Surface Sticker & Decal Atlas Tool for Unity URP

[![Unity](https://img.shields.io/badge/Unity-2022.3%2B%20%7C%206.0-blue.svg)](https://unity.com/)
[![Render Pipeline](https://img.shields.io/badge/Pipeline-URP-brightgreen.svg)](https://unity.com/srp/Universal-Render-Pipeline)
[![License](https://img.shields.io/badge/License-MIT-orange.svg)](LICENSE)
[![Version](https://img.shields.io/badge/Version-1.0.0-informational.svg)]()

Stop cropping textures in Photoshop! **Universal Surface Sticker & Decal Atlas Tool** is an all-in-one, highly optimized Unity URP package to crop, scale, remove solid background colors, snap, rotate, batch-edit, and stamp stickers or road signs directly inside Unity from a single texture atlas image.

---

## ✨ Features

- 🎨 **Visual Atlas Selector**: Click grid cells or drag custom crop boxes over texture atlas previews directly in the Inspector or Editor Window.
- 🎯 **360° Multi-Directional Surface Snapping**: Raycasts 360 degrees around decals to snap and align them flat against ANY surface normal (ground, vertical walls, billboards, sloped roofs, terrain).
- 🔄 **Face Cycling & 180° Flipping**: Cycle between nearby surface faces with a single click (`🔄 Cycle Next Face`) or flip orientation 180 degrees (`↕️ Flip Face`).
- 🖌️ **Scene View Stamp / Paint Mode**: Hover over any surface in the Scene View and left-click to paint stickers live directly onto scene geometry.
- 🎭 **Chroma Key (Solid Background Removal)**: Custom URP shader dynamically converts solid black, white, or colored background textures into transparent alpha with customizable tolerance and edge softness.
- ⚡ **Zero Material Duplication**: Uses `MaterialPropertyBlock` so all stickers across your scene share **1 single material and 1 texture atlas**, maximizing SRP Batching and reducing VRAM usage.
- 🖼️ **Normal Map Atlas Support**: Full PBR normal map support matching your diffuse atlas grid layout for realistic 3D surface depth and lighting.
- 📦 **3D Solid Panel Generator**: Choose between flat 2D Planes, 2D Quads, or solid 3D Box Signboards with thickness.
- 👥 **Multi-Object Batch Stamping**: Select multiple target objects in your Hierarchy (e.g. 10 billboard frames) and attach aligned stickers to all of them at once.

---

## 🚀 Quick Installation

### Option 1: Install via Unity Package Manager (Git URL)
1. In Unity, open **Window > Package Manager**.
2. Click **`+ > Add package from git URL...`**.
3. Paste the repository URL:
   ```text
   https://github.com/yourusername/UniversalStickerAtlasTool.git
   ```
4. Unity will automatically fetch and install the package!

### Option 2: Install via `.unitypackage`
1. Download `UniversalStickerAtlasTool.unitypackage` from Releases.
2. Double-click or drag the package into your Unity project and click **Import**.

---

## 📖 Quick Start

### 1. Open the Tool Window
In the top Unity menu bar, go to:
**`Tools > Universal Sticker Atlas Tool`**

### 2. Crop & Select Signs
- Assign your **Atlas Base Texture** (e.g. `RoadLines019A_4K-JPG_Opacity 1.jpg`).
- (Optional) Assign a **Normal Map** atlas.
- Choose **Grid Mode** (4x4, 8x8) or drag a box visually over the preview to crop a sticker.

### 3. Remove Solid Backgrounds (Chroma Key)
If your image has a solid background (e.g., solid black or solid white):
- Check **"Remove Solid Background"**.
- Select the background color and adjust **Tolerance** (`0.1`) and **Softness** (`0.05`).

### 4. Snap to Surfaces
- Select your sticker object in the Hierarchy.
- Click **`🎯 Snap to Nearest Surface`** — it searches 360° around the decal and aligns it flat to the closest surface face (works on ground, walls, and collider-less meshes).
- Click **`🔄 Cycle Next Face`** to hop between multiple nearby faces.

---

## 📁 Repository Structure

```
Assets/UniversalStickerAtlasTool/
├── Runtime/
│   └── RoadSignDecal.cs           # Main component (with Tooltips)
├── Editor/
│   ├── RoadSignDecalEditor.cs      # Inspector Editor & Visual Picker
│   └── RoadSignAtlasWindow.cs     # Editor Window Tool
├── Shaders/
│   └── RoadSignAtlasURP.shader    # Custom URP Lit Shader
├── Textures/
│   ├── RoadLines019A_4K-JPG_Opacity 1.jpg  # Sample Texture Atlas
│   └── RoadLines019A_4K-JPG_NormalDX.jpg   # Sample Normal Map Atlas
├── Tests/
│   └── StickerAtlasTests.cs       # Automated NUnit Unit Tests
├── package.json                   # UPM Manifest
├── USER_GUIDE.md                  # Comprehensive Documentation
└── README.md                      # Repository Overview
```

---

## ⚙️ Technical Requirements

- **Unity**: 2022.3 LTS or Unity 6+
- **Render Pipeline**: Universal Render Pipeline (URP)

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
