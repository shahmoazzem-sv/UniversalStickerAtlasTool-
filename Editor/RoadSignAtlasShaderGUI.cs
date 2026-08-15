using UnityEditor;
using UnityEngine;

namespace UniversalStickerAtlas.Editor
{
    public class RoadSignAtlasShaderGUI : ShaderGUI
    {
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            Material targetMat = materialEditor.target as Material;

            // Check if any RoadSignDecal object is currently selected
            RoadSignDecal selectedDecal = Selection.activeGameObject != null
                ? Selection.activeGameObject.GetComponent<RoadSignDecal>()
                : null;

            // Shared Material Notice Banner
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("🏷️ Universal Sticker & Decal Atlas Material", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "⚠️ SHARED MATERIAL ASSET\n" +
                "This material is shared across multiple stickers in your scene.\n" +
                "• Changing 'Global Material Color' below affects ALL stickers sharing this material.\n" +
                "• To change color for ONLY one sticker, use 'Per-Sticker Tint' on the Sticker Decal component.",
                MessageType.Warning
            );

            // If a decal is selected, offer direct per-sticker color control right here!
            if (selectedDecal != null)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("🎨 Selected Sticker Quick Color:", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                Color newDecalColor = EditorGUILayout.ColorField(
                    new GUIContent("Per-Sticker Tint (Selected Only)", "Changes the tint for the currently selected sticker only without affecting other stickers."),
                    selectedDecal.TintColor
                );
                if (EditorGUI.EndChangeCheck())
                {
                    foreach (GameObject go in Selection.gameObjects)
                    {
                        if (go == null) continue;
                        RoadSignDecal d = go.GetComponent<RoadSignDecal>();
                        if (d != null)
                        {
                            Undo.RecordObject(d, "Change Sticker Decal Tint");
                            d.TintColor = newDecalColor;
                            d.ApplyProperties();
                            EditorUtility.SetDirty(d);
                        }
                    }
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);

            // Find shader properties
            MaterialProperty baseMap = FindProperty("_BaseMap", properties, false);
            MaterialProperty baseColor = FindProperty("_BaseColor", properties, false);
            MaterialProperty enableNormal = FindProperty("_EnableNormalMap", properties, false);
            MaterialProperty bumpMap = FindProperty("_BumpMap", properties, false);
            MaterialProperty bumpScale = FindProperty("_BumpScale", properties, false);

            MaterialProperty useColorKey = FindProperty("_UseColorKey", properties, false);
            MaterialProperty colorKey = FindProperty("_ColorKey", properties, false);
            MaterialProperty colorKeyTol = FindProperty("_ColorKeyTolerance", properties, false);
            MaterialProperty colorKeySoft = FindProperty("_ColorKeySoftness", properties, false);

            MaterialProperty signRect = FindProperty("_SignRect", properties, false);
            MaterialProperty useGrid = FindProperty("_UseGrid", properties, false);
            MaterialProperty gridCols = FindProperty("_GridCols", properties, false);
            MaterialProperty gridRows = FindProperty("_GridRows", properties, false);
            MaterialProperty cellIndex = FindProperty("_CellIndex", properties, false);

            MaterialProperty clampToRect = FindProperty("_ClampToRect", properties, false);
            MaterialProperty edgeMargin = FindProperty("_EdgeMargin", properties, false);
            MaterialProperty cutoff = FindProperty("_Cutoff", properties, false);
            MaterialProperty smoothness = FindProperty("_Smoothness", properties, false);
            MaterialProperty metallic = FindProperty("_Metallic", properties, false);
            MaterialProperty zOffset = FindProperty("_ZOffset", properties, false);

            // 1. Base Map & Global Material Color Tint
            if (baseMap != null) materialEditor.TexturePropertySingleLine(new GUIContent("Base Map (Atlas)"), baseMap);

            if (baseColor != null)
            {
                materialEditor.ColorProperty(baseColor, "Global Material Color (All Stickers)");
            }

            EditorGUILayout.Space(8);

            // 2. Normal Map Settings
            if (enableNormal != null)
            {
                materialEditor.ShaderProperty(enableNormal, "Enable Normal Map");
                if (enableNormal.floatValue > 0.5f)
                {
                    if (bumpMap != null) materialEditor.TexturePropertySingleLine(new GUIContent("Normal Map (Bump Map)"), bumpMap);
                    if (bumpScale != null) materialEditor.FloatProperty(bumpScale, "Normal Scale");
                }
            }

            EditorGUILayout.Space(8);

            // 3. Color Key Background Removal
            if (useColorKey != null)
            {
                materialEditor.ShaderProperty(useColorKey, "Remove Solid Background Color");
                if (useColorKey.floatValue > 0.5f)
                {
                    if (colorKey != null) materialEditor.ColorProperty(colorKey, "Background Color to Key Out");
                    if (colorKeyTol != null) materialEditor.RangeProperty(colorKeyTol, "Color Key Tolerance");
                    if (colorKeySoft != null) materialEditor.RangeProperty(colorKeySoft, "Color Key Softness");
                }
            }

            EditorGUILayout.Space(8);

            // 4. Atlas Region Controls
            EditorGUILayout.LabelField("Atlas Region Controls", EditorStyles.boldLabel);
            if (useGrid != null) materialEditor.ShaderProperty(useGrid, "Use Grid Mode");

            if (useGrid != null && useGrid.floatValue > 0.5f)
            {
                if (gridCols != null) materialEditor.ShaderProperty(gridCols, "Grid Columns");
                if (gridRows != null) materialEditor.ShaderProperty(gridRows, "Grid Rows");
                if (cellIndex != null) materialEditor.ShaderProperty(cellIndex, "Cell Index (0-based)");
            }
            else
            {
                if (signRect != null) materialEditor.VectorProperty(signRect, "Sign UV Rect (X, Y, Width, Height)");
            }

            EditorGUILayout.Space(8);

            // 5. Advanced Settings
            if (clampToRect != null) materialEditor.ShaderProperty(clampToRect, "Clamp Edges (Prevent Bleeding)");
            if (edgeMargin != null) materialEditor.RangeProperty(edgeMargin, "Edge Padding (Fraction)");
            if (cutoff != null) materialEditor.RangeProperty(cutoff, "Alpha Cutoff");
            if (smoothness != null) materialEditor.RangeProperty(smoothness, "Smoothness");
            if (metallic != null) materialEditor.RangeProperty(metallic, "Metallic");
            if (zOffset != null) materialEditor.RangeProperty(zOffset, "Vertex Normal Lift Offset");

            // Render Queue & Double Sided options
            materialEditor.RenderQueueField();
            materialEditor.DoubleSidedGIField();
        }
    }
}
