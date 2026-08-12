using UnityEditor;
using UnityEngine;

namespace UniversalStickerAtlas.Editor
{
    public class RoadSignAtlasWindow : EditorWindow
    {
        private Texture2D atlasTexture;
        private Texture2D normalMap;
        private bool useColorKey = false;
        private Color colorKey = Color.black;
        private float colorKeyTolerance = 0.1f;
        private float colorKeySoftness = 0.05f;

        private bool useGrid = true;
        private int gridCols = 4;
        private int gridRows = 4;
        private int cellIndex = 0;
        private Rect signRect = new Rect(0f, 0f, 1f, 1f);
        private float defaultWidth = 2.5f;

        private bool stampModeEnabled = false;
        private DecalMeshType spawnMeshType = DecalMeshType.FlatPlane;

        private Vector2 scrollPos;
        private Vector2 dragStartUV;
        private bool isDraggingRect = false;

        private RoadSignDecal targetDecal;

        [MenuItem("Tools/Universal Sticker Atlas Tool")]
        public static void OpenWindow()
        {
            RoadSignAtlasWindow window = GetWindow<RoadSignAtlasWindow>("Universal Sticker Atlas");
            window.minSize = new Vector2(450, 650);
            window.Show();
        }

        public static void OpenWindow(RoadSignDecal decal)
        {
            RoadSignAtlasWindow window = GetWindow<RoadSignAtlasWindow>("Universal Sticker Atlas");
            window.targetDecal = decal;
            if (decal != null)
            {
                window.atlasTexture = decal.AtlasTexture;
                window.normalMap = decal.NormalMap;
                window.useColorKey = decal.UseColorKey;
                window.colorKey = decal.ColorKey;
                window.colorKeyTolerance = decal.ColorKeyTolerance;
                window.colorKeySoftness = decal.ColorKeySoftness;
                window.useGrid = decal.UseGrid;
                window.gridCols = decal.GridCols;
                window.gridRows = decal.GridRows;
                window.cellIndex = decal.CellIndex;
                window.signRect = decal.SignRect;
                window.defaultWidth = decal.TargetWidth;
            }
            window.Show();
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Universal Surface Sticker & Decal Tool", new GUIStyle(EditorStyles.boldLabel) { fontSize = 16, alignment = TextAnchor.MiddleCenter });

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            atlasTexture = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Atlas Base Texture", "The base diffuse/albedo texture atlas image containing sticker or road sign graphics."), atlasTexture, typeof(Texture2D), false);
            normalMap = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Normal Map (Optional)", "Optional normal map atlas matching the base texture layout for realistic 3D surface detail."), normalMap, typeof(Texture2D), false);
            defaultWidth = EditorGUILayout.FloatField(new GUIContent("Default Width (m)", "Target physical world width (in meters) for spawned stickers."), defaultWidth);
            spawnMeshType = (DecalMeshType)EditorGUILayout.EnumPopup(new GUIContent("Mesh Generation Type", "Choose between 2D Plane, 2D Quad, or 3D Solid Panel box with thickness."), spawnMeshType);

            EditorGUILayout.Space(5);
            useColorKey = EditorGUILayout.Toggle(new GUIContent("Remove Solid Background", "Enable chroma key background removal to make solid background colors transparent."), useColorKey);
            if (useColorKey)
            {
                EditorGUI.indentLevel++;
                colorKey = EditorGUILayout.ColorField(new GUIContent("Color to Remove", "The solid background color to key out and make transparent."), colorKey);
                colorKeyTolerance = EditorGUILayout.Slider(new GUIContent("Tolerance", "Color distance tolerance threshold for background removal."), colorKeyTolerance, 0f, 1f);
                colorKeySoftness = EditorGUILayout.Slider(new GUIContent("Softness", "Edge softness for smooth alpha transition when keying out solid backgrounds."), colorKeySoftness, 0.001f, 0.5f);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(5);
            useGrid = EditorGUILayout.Toggle(new GUIContent("Use Grid Mode", "Select sub-textures by cell column/row grid index instead of manual rect coordinates."), useGrid);

            if (useGrid)
            {
                gridCols = EditorGUILayout.IntSlider("Grid Columns", gridCols, 1, 16);
                gridRows = EditorGUILayout.IntSlider("Grid Rows", gridRows, 1, 16);
                cellIndex = EditorGUILayout.IntSlider("Cell Index", cellIndex, 0, Mathf.Max(0, gridCols * gridRows - 1));

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("4x4 Grid")) { gridCols = 4; gridRows = 4; }
                if (GUILayout.Button("4x8 Grid")) { gridCols = 4; gridRows = 8; }
                if (GUILayout.Button("8x8 Grid")) { gridCols = 8; gridRows = 8; }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                signRect = EditorGUILayout.RectField("Sign Rect (0..1)", signRect);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            if (atlasTexture != null)
            {
                DrawAtlasPreview(atlasTexture);

                EditorGUILayout.Space(15);

                GUI.backgroundColor = stampModeEnabled ? new Color(0.4f, 1f, 0.4f) : Color.white;
                if (GUILayout.Button(stampModeEnabled ? "DISABLE Scene View Stamp Mode" : "ENABLE Scene View Stamp Mode (Click in Scene to Paint)", GUILayout.Height(35)))
                {
                    stampModeEnabled = !stampModeEnabled;
                    SceneView.RepaintAll();
                }
                GUI.backgroundColor = Color.white;

                if (stampModeEnabled)
                {
                    EditorGUILayout.HelpBox("Stamp Mode active! Hover over any ground, wall, or billboard in Scene View and LEFT-CLICK to stamp sticker!", MessageType.Warning);
                }

                EditorGUILayout.Space(10);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Spawn Sticker Quad/Panel", GUILayout.Height(35)))
                {
                    SpawnSignQuadAtPivot();
                }

                int count = Selection.gameObjects != null ? Selection.gameObjects.Length : 0;
                string batchText = count > 1 ? $"Attach Sticker to All Selected ({count} Objects)" : "Apply to Selected Object";
                if (count > 0 && GUILayout.Button(batchText, GUILayout.Height(35)))
                {
                    ApplyToSelectedBatch();
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("Select a texture atlas image above to view and pick signs visually.", MessageType.Info);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawAtlasPreview(Texture2D texture)
        {
            EditorGUILayout.LabelField("Visual Sign Selector (Click cell or drag rect):", EditorStyles.boldLabel);

            float aspect = (float)texture.width / texture.height;
            Rect viewRect = GUILayoutUtility.GetAspectRect(aspect, GUILayout.MaxHeight(360));

            GUI.DrawTexture(viewRect, texture, ScaleMode.ScaleToFit);

            Event evt = Event.current;
            Vector2 mousePos = evt.mousePosition;

            Handles.color = new Color(1f, 1f, 1f, 0.4f);
            if (useGrid)
            {
                float colWidth = viewRect.width / gridCols;
                float rowHeight = viewRect.height / gridRows;

                for (int i = 1; i < gridCols; i++)
                {
                    Handles.DrawLine(new Vector3(viewRect.x + i * colWidth, viewRect.y, 0), new Vector3(viewRect.x + i * colWidth, viewRect.yMax, 0));
                }
                for (int j = 1; j < gridRows; j++)
                {
                    Handles.DrawLine(new Vector3(viewRect.x, viewRect.y + j * rowHeight, 0), new Vector3(viewRect.xMax, viewRect.y + j * rowHeight, 0));
                }

                int cellX = cellIndex % gridCols;
                int cellY = cellIndex / gridCols;

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
                    if (useGrid)
                    {
                        int clickedCol = Mathf.FloorToInt(normalizedU * gridCols);
                        int clickedRow = Mathf.FloorToInt((1.0f - normalizedV) * gridRows);
                        cellIndex = clickedRow * gridCols + clickedCol;
                    }
                    else
                    {
                        dragStartUV = new Vector2(normalizedU, normalizedV);
                        isDraggingRect = true;
                    }
                    Repaint();
                    evt.Use();
                }
                else if (evt.type == EventType.MouseDrag && isDraggingRect && !useGrid)
                {
                    float minU = Mathf.Min(dragStartUV.x, normalizedU);
                    float maxU = Mathf.Max(dragStartUV.x, normalizedU);
                    float minV = Mathf.Min(dragStartUV.y, normalizedV);
                    float maxV = Mathf.Max(dragStartUV.y, normalizedV);

                    signRect = new Rect(minU, minV, Mathf.Max(0.01f, maxU - minU), Mathf.Max(0.01f, maxV - minV));
                    Repaint();
                    evt.Use();
                }
                else if (evt.type == EventType.MouseUp && isDraggingRect)
                {
                    isDraggingRect = false;
                    evt.Use();
                }
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!stampModeEnabled) return;

            Event e = Event.current;
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Handles.color = Color.green;
                Handles.DrawWireDisc(hit.point, hit.normal, defaultWidth * 0.5f);
                Handles.DrawLine(hit.point, hit.point + hit.normal * 1.0f);

                int controlID = GUIUtility.GetControlID(FocusType.Passive);
                HandleUtility.AddDefaultControl(controlID);

                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    SpawnStickerAtPosition(hit.point, hit.normal);
                    e.Use();
                }
            }

            sceneView.Repaint();
        }

        private void SpawnStickerAtPosition(Vector3 point, Vector3 normal)
        {
            GameObject obj;
            if (spawnMeshType == DecalMeshType.Solid3DPanel)
            {
                obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obj.name = "Sticker_SolidPanel";
            }
            else if (spawnMeshType == DecalMeshType.FlatQuad)
            {
                obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                obj.name = "Sticker_Quad";
            }
            else
            {
                obj = GameObject.CreatePrimitive(PrimitiveType.Plane);
                obj.name = "Sticker_Plane";
            }

            SetupMeshAndMaterial(obj);

            RoadSignDecal decal = obj.GetComponent<RoadSignDecal>();
            if (decal == null) decal = obj.AddComponent<RoadSignDecal>();

            ConfigureDecalProperties(decal);

            decal.AlignToSurface(point, normal, 0.002f);
            decal.ApplyProperties();

            Undo.RegisterCreatedObjectUndo(obj, "Stamp Surface Sticker");
            Selection.activeGameObject = obj;
        }

        private void SpawnSignQuadAtPivot()
        {
            Vector3 spawnPos = Vector3.zero;
            Vector3 spawnNormal = Vector3.up;

            if (SceneView.lastActiveSceneView != null)
            {
                spawnPos = SceneView.lastActiveSceneView.pivot;
            }

            SpawnStickerAtPosition(spawnPos, spawnNormal);
        }

        private void ApplyToSelectedBatch()
        {
            GameObject[] selectedObjs = Selection.gameObjects;
            if (selectedObjs == null || selectedObjs.Length == 0) return;

            foreach (GameObject sel in selectedObjs)
            {
                RoadSignDecal decal = sel.GetComponent<RoadSignDecal>();
                if (decal == null)
                {
                    decal = sel.AddComponent<RoadSignDecal>();
                }

                SetupMeshAndMaterial(sel);

                Undo.RecordObject(decal, "Apply Sticker Atlas Settings");
                ConfigureDecalProperties(decal);
                decal.ApplyProperties();
                EditorUtility.SetDirty(decal);
            }
        }

        private void SetupMeshAndMaterial(GameObject target)
        {
            MeshRenderer renderer = target.GetComponent<MeshRenderer>();
            if (renderer == null) return;

            Shader shader = Shader.Find("Custom/RoadSignAtlasURP");
            if (shader != null)
            {
                Material mat = new Material(shader);
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

        private void ConfigureDecalProperties(RoadSignDecal decal)
        {
            decal.AtlasTexture = atlasTexture;
            decal.NormalMap = normalMap;
            decal.UseColorKey = useColorKey;
            decal.ColorKey = colorKey;
            decal.ColorKeyTolerance = colorKeyTolerance;
            decal.ColorKeySoftness = colorKeySoftness;
            decal.UseGrid = useGrid;
            decal.GridCols = gridCols;
            decal.GridRows = gridRows;
            decal.CellIndex = cellIndex;
            decal.SignRect = signRect;
            decal.TargetWidth = defaultWidth;
        }
    }
}
