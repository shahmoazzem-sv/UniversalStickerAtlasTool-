using UnityEditor;
using UnityEngine;

namespace UniversalStickerAtlas.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(RoadSignDecal))]
    public class RoadSignDecalEditor : UnityEditor.Editor
    {
        private RoadSignDecal decal;
        private bool showInteractiveAtlas = true;
        private Vector2 dragStartUV;
        private bool isDraggingRect = false;

        private void OnEnable()
        {
            decal = (RoadSignDecal)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EnsureMaterial();

            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Surface Alignment & Snapping Controls", EditorStyles.boldLabel);

            float rayDist = decal.RaycastDistance;
            EditorGUI.BeginChangeCheck();
            rayDist = EditorGUILayout.Slider(new GUIContent("Raycast Search Range (m)", "Search radius range (in meters) for 360-degree surface raycast snapping."), rayDist, 1.0f, 50.0f);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (Object obj in targets)
                {
                    RoadSignDecal d = (RoadSignDecal)obj;
                    Undo.RecordObject(d, "Change Raycast Distance");
                    d.RaycastDistance = rayDist;
                    EditorUtility.SetDirty(d);
                }
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("🎯 Snap to Nearest Surface", "Search 360 degrees around decal and snap flat to closest surface face."), GUILayout.Height(30)))
            {
                foreach (Object obj in targets)
                {
                    RoadSignDecal d = (RoadSignDecal)obj;
                    Undo.RecordObject(d.transform, "Snap to Nearest Surface");
                    bool success = d.SnapToNearestSurface();
                    if (!success && SceneView.lastActiveSceneView != null)
                    {
                        Vector3 camPos = SceneView.lastActiveSceneView.camera.transform.position;
                        Vector3 dir = (d.transform.position - camPos).normalized;
                        RaycastHit hit;
                        if (Physics.Raycast(camPos, dir, out hit, 100f, ~0))
                        {
                            d.AlignToSurface(hit.point, hit.normal, d.ZOffset);
                        }
                    }
                    EditorUtility.SetDirty(d);
                }
            }

            string cycleLabel = decal.DetectedHitsCount > 1
                ? $"🔄 Cycle Face ({decal.CurrentHitIndex + 1}/{decal.DetectedHitsCount})"
                : "🔄 Cycle Next Face";

            if (GUILayout.Button(new GUIContent(cycleLabel, "Cycle to the next detected surface face if multiple faces are in range."), GUILayout.Height(30)))
            {
                foreach (Object obj in targets)
                {
                    RoadSignDecal d = (RoadSignDecal)obj;
                    Undo.RecordObject(d.transform, "Cycle Surface Face");
                    d.CycleNextSurfaceFace();
                    EditorUtility.SetDirty(d);
                }
            }

            if (GUILayout.Button(new GUIContent("↕️ Flip Face (180°)", "Rotate decal rotation 180 degrees to face opposite side."), GUILayout.Height(30)))
            {
                foreach (Object obj in targets)
                {
                    RoadSignDecal d = (RoadSignDecal)obj;
                    Undo.RecordObject(d.transform, "Flip Face");
                    d.FlipSurfaceNormal();
                    EditorUtility.SetDirty(d);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            showInteractiveAtlas = EditorGUILayout.Foldout(showInteractiveAtlas, "Interactive Atlas Visual Picker", true, EditorStyles.foldoutHeader);

            if (showInteractiveAtlas && decal.AtlasTexture != null)
            {
                DrawInteractiveAtlasPicker(decal.AtlasTexture);
            }
            else if (decal.AtlasTexture == null)
            {
                EditorGUILayout.HelpBox("Assign an Atlas Texture above to enable the interactive visual picker.", MessageType.Info);
            }

            EditorGUILayout.Space(10);
            if (GUILayout.Button("Fit Mesh Scale to Sign Aspect Ratio", GUILayout.Height(30)))
            {
                foreach (Object obj in targets)
                {
                    RoadSignDecal targetDecal = (RoadSignDecal)obj;
                    Undo.RecordObject(targetDecal.transform, "Adjust Sign Aspect Ratio");
                    targetDecal.AdjustTransformAspect();
                    EditorUtility.SetDirty(targetDecal);
                }
            }

            if (GUILayout.Button("Open Universal Sticker Atlas Window", GUILayout.Height(25)))
            {
                RoadSignAtlasWindow.OpenWindow(decal);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void EnsureMaterial()
        {
            MeshRenderer renderer = decal.GetComponent<MeshRenderer>();
            if (renderer != null && (renderer.sharedMaterial == null || renderer.sharedMaterial.shader.name != "Custom/RoadSignAtlasURP"))
            {
                EditorGUILayout.HelpBox("MeshRenderer needs a Material using 'Custom/RoadSignAtlasURP' shader to render atlas crops properly.", MessageType.Warning);
                if (GUILayout.Button("Assign/Create RoadSignAtlas Material"))
                {
                    Shader atlasShader = Shader.Find("Custom/RoadSignAtlasURP");
                    if (atlasShader != null)
                    {
                        Material mat = new Material(atlasShader);
                        mat.name = "RoadSignAtlas_Mat";

                        string path = "Assets/UniversalStickerAtlasTool/Materials";
                        if (!AssetDatabase.IsValidFolder(path))
                        {
                            System.IO.Directory.CreateDirectory(path);
                            AssetDatabase.Refresh();
                        }

                        string matPath = path + "/RoadSignAtlas_Mat.mat";
                        Material existing = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                        if (existing != null)
                        {
                            renderer.sharedMaterial = existing;
                        }
                        else
                        {
                            AssetDatabase.CreateAsset(mat, matPath);
                            renderer.sharedMaterial = mat;
                        }
                    }
                }
            }
        }

        private void DrawInteractiveAtlasPicker(Texture2D texture)
        {
            EditorGUILayout.HelpBox("Click cell in Grid mode, or Drag box in Custom mode to crop a sign from the atlas!", MessageType.None);

            float aspect = (float)texture.width / texture.height;
            Rect viewRect = GUILayoutUtility.GetAspectRect(aspect, GUILayout.MaxHeight(300));

            GUI.DrawTexture(viewRect, texture, ScaleMode.ScaleToFit);

            Event evt = Event.current;
            Vector2 mousePos = evt.mousePosition;

            bool useGrid = decal.UseGrid;
            int cols = decal.GridCols;
            int rows = decal.GridRows;
            Rect signRect = decal.SignRect;

            Handles.color = new Color(1f, 1f, 1f, 0.4f);
            if (useGrid)
            {
                float colWidth = viewRect.width / cols;
                float rowHeight = viewRect.height / rows;

                for (int i = 1; i < cols; i++)
                {
                    Handles.DrawLine(new Vector3(viewRect.x + i * colWidth, viewRect.y, 0), new Vector3(viewRect.x + i * colWidth, viewRect.yMax, 0));
                }
                for (int j = 1; j < rows; j++)
                {
                    Handles.DrawLine(new Vector3(viewRect.x, viewRect.y + j * rowHeight, 0), new Vector3(viewRect.xMax, viewRect.y + j * rowHeight, 0));
                }

                int index = decal.CellIndex;
                int cellX = index % cols;
                int cellY = index / cols;

                Rect highlightRect = new Rect(
                    viewRect.x + cellX * colWidth,
                    viewRect.y + cellY * rowHeight,
                    colWidth,
                    rowHeight
                );

                Handles.DrawSolidRectangleWithOutline(highlightRect, new Color(0.2f, 0.8f, 1f, 0.25f), Color.cyan);
            }
            else
            {
                float rectX = viewRect.x + signRect.x * viewRect.width;
                float rectY = viewRect.y + (1.0f - signRect.y - signRect.height) * viewRect.height;
                float rectW = signRect.width * viewRect.width;
                float rectH = signRect.height * viewRect.height;

                Rect highlightRect = new Rect(rectX, rectY, rectW, rectH);
                Handles.DrawSolidRectangleWithOutline(highlightRect, new Color(1f, 0.8f, 0.2f, 0.25f), Color.yellow);
            }

            if (viewRect.Contains(mousePos))
            {
                Vector2 localPos = mousePos - new Vector2(viewRect.x, viewRect.y);
                float normalizedU = Mathf.Clamp01(localPos.x / viewRect.width);
                float normalizedV = Mathf.Clamp01(1.0f - (localPos.y / viewRect.height));

                if (evt.type == EventType.MouseDown && evt.button == 0)
                {
                    foreach (Object obj in targets)
                    {
                        RoadSignDecal targetDecal = (RoadSignDecal)obj;
                        Undo.RecordObject(targetDecal, "Select Atlas Sign Region");

                        if (useGrid)
                        {
                            int clickedCol = Mathf.FloorToInt(normalizedU * cols);
                            int clickedRow = Mathf.FloorToInt((1.0f - normalizedV) * rows);
                            int newIndex = clickedRow * cols + clickedCol;

                            targetDecal.CellIndex = newIndex;
                        }
                        else
                        {
                            dragStartUV = new Vector2(normalizedU, normalizedV);
                            isDraggingRect = true;
                        }

                        targetDecal.ApplyProperties();
                        EditorUtility.SetDirty(targetDecal);
                    }
                    evt.Use();
                }
                else if (evt.type == EventType.MouseDrag && isDraggingRect && !useGrid)
                {
                    float minU = Mathf.Min(dragStartUV.x, normalizedU);
                    float maxU = Mathf.Max(dragStartUV.x, normalizedU);
                    float minV = Mathf.Min(dragStartUV.y, normalizedV);
                    float maxV = Mathf.Max(dragStartUV.y, normalizedV);

                    foreach (Object obj in targets)
                    {
                        RoadSignDecal targetDecal = (RoadSignDecal)obj;
                        Undo.RecordObject(targetDecal, "Drag Sign Rect");
                        targetDecal.SignRect = new Rect(minU, minV, Mathf.Max(0.01f, maxU - minU), Mathf.Max(0.01f, maxV - minV));
                        targetDecal.ApplyProperties();
                        EditorUtility.SetDirty(targetDecal);
                    }
                    evt.Use();
                }
                else if (evt.type == EventType.MouseUp && isDraggingRect)
                {
                    isDraggingRect = false;
                    evt.Use();
                }
            }
        }
    }
}
