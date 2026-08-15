using UnityEditor;
using UnityEngine;

namespace UniversalStickerAtlas.Editor
{
    public class RoadSignAtlasShaderGUI : ShaderGUI
    {
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            Material targetMat = materialEditor.target as Material;

            // Decal Info Banner
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("🏷️ Universal Sticker & Decal Atlas Material", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "This material is shared across Sticker Decal objects.\n" +
                "• Changing 'Color Tint' below will automatically sync to any selected Sticker Decal in your scene.\n" +
                "• You can also adjust individual decals directly on the 'Sticker Decal' component on your GameObject.",
                MessageType.Info
            );
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(6);

            // Find properties
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

            // 1. Base Map & Color Tint (Synchronized to selected Decals!)
            if (baseMap != null) materialEditor.TexturePropertySingleLine(new GUIContent("Base Map (Atlas)"), baseMap);

            if (baseColor != null)
            {
                EditorGUI.BeginChangeCheck();
                materialEditor.ColorProperty(baseColor, "Color Tint");
                if (EditorGUI.EndChangeCheck())
                {
                    // Sync color change to currently selected RoadSignDecal instances
                    SyncColorToSelectedDecals(baseColor.colorValue);
                }
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
                if (gridCols != null) materialEditor.IntSliderProperty(gridCols, 1, 16, new GUIContent("Grid Columns"));
                if (gridRows != null) materialEditor.IntSliderProperty(gridRows, 1, 16, new GUIContent("Grid Rows"));
                if (cellIndex != null) materialEditor.IntSliderProperty(cellIndex, 0, 64, new GUIContent("Cell Index (0-based)"));
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

        private void SyncColorToSelectedDecals(Color newColor)
        {
            if (Selection.gameObjects == null) return;

            foreach (GameObject go in Selection.gameObjects)
            {
                if (go == null) continue;
                RoadSignDecal decal = go.GetComponent<RoadSignDecal>();
                if (decal != null)
                {
                    Undo.RecordObject(decal, "Change Sticker Decal Tint from Material");
                    decal.TintColor = newColor;
                    decal.ApplyProperties();
                    EditorUtility.SetDirty(decal);
                }
            }
        }
    }
}
