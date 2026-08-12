# Universal Surface Sticker & Decal Atlas Tool - User Guide

A complete, production-ready Unity URP package to crop, scale, remove solid backgrounds, snap, rotate, batch-edit, and stamp stickers or road signs directly inside Unity without needing Photoshop!

---

## 📁 Package Contents

```
Assets/UniversalStickerAtlasTool/
├── Runtime/
│   └── RoadSignDecal.cs           # Main MonoBehaviour component (with Tooltips)
├── Editor/
│   ├── RoadSignDecalEditor.cs      # Custom Inspector Editor & Visual Picker
│   └── RoadSignAtlasWindow.cs     # Editor Window (Tools > Universal Sticker Atlas Tool)
├── Shaders/
│   └── RoadSignAtlasURP.shader    # Custom URP Lit Shader (Atlas cropping + Chroma key + Normal maps)
├── Textures/
│   ├── RoadLines019A_4K-JPG_Opacity 1.jpg  # Sample Atlas Texture (Black Background)
│   └── RoadLines019A_4K-JPG_NormalDX.jpg   # Sample Normal Map Atlas
├── Materials/                      # Generated materials storage
├── Tests/
│   └── StickerAtlasTests.cs       # NUnit Automated Unit Tests
├── package.json                   # Unity Package Manager manifest
└── USER_GUIDE.md                  # Comprehensive Documentation
```

---

## 🚀 Step-by-Step Guide

### 1. Opening the Tool Window
Go to the top Unity menu bar:
**`Tools > Universal Sticker Atlas Tool`**

---

### 2. Using Atlas Textures & Normal Maps
1. Drag **`RoadLines019A_4K-JPG_Opacity 1.jpg`** into the **Atlas Base Texture** slot.
2. Drag **`RoadLines019A_4K-JPG_NormalDX.jpg`** into the **Normal Map (Optional)** slot.
3. Select **Grid Mode** (e.g. `4x4 Grid`) or drag a box visually over the texture preview to select a sticker/sign (e.g., "SLOW", "AHEAD", "KMH ENTRY").

---

### 3. Removing Solid Backgrounds (Chroma Key / Color Key)
If your texture atlas has a solid background (e.g. **`RoadLines019A_4K-JPG_Opacity 1.jpg`** has a solid black background):
1. Check **"Remove Solid Background"**.
2. Click **Color to Remove** and select **Black (0,0,0)** or **White (1,1,1)**.
3. Adjust **Tolerance** (e.g. `0.1`) and **Softness** (e.g. `0.05`) to make the background completely transparent while keeping clean anti-aliased edges!

---

### 4. Snapping to Surfaces (Ground, Walls, Billboards)
When placing a sticker near any object (building, ground, vertical wall, sloped roof):
1. Select the sticker object in the Hierarchy.
2. In the Inspector under **Surface Alignment & Snapping Controls**:
   - Set **Raycast Search Range (m)** (e.g. `15m` or `25m`).
   - Click **`🎯 Snap to Nearest Surface`** — the tool searches 360° around the sticker and snaps it flat against the closest surface face (even on collider-less meshes)!

---

### 5. Cycling & Rotating / Flipping Faces
- **`🔄 Cycle Next Face`**: If multiple surfaces/faces are detected nearby (e.g. front face, back wall, side face), click **Cycle Next Face** to hop between faces.
- **`↕️ Flip Face (180°)`**: Click to rotate the sticker 180 degrees to face the opposite direction if it snapped facing inward.

---

### 6. Scene View Paint & Stamp Mode
1. In the tool window (`Tools > Universal Sticker Atlas Tool`), click **"ENABLE Scene View Stamp Mode"**.
2. Hover over any ground, building wall, or billboard in Scene View — a live green ring preview appears.
3. **Left-Click** anywhere to stamp stickers live on the surface!

---

### 7. Multi-Object Batch Stamping
1. Select multiple objects in your Hierarchy (e.g., 10 billboard frames or 10 wall segments).
2. Click **"Attach Sticker to All Selected (N Objects)"** in the window.
3. All selected objects will instantly receive aligned stickers!

---

## 📦 How to Export & Share as Unity Package

1. Right-click the **`UniversalStickerAtlasTool`** folder in Unity's Project window.
2. Select **Export Package...**
3. Ensure all files inside `Assets/UniversalStickerAtlasTool` are checked.
4. Click **Export...** and save your `.unitypackage` file to share with team members or customers!
