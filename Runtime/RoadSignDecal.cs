using System.Collections.Generic;
using UnityEngine;

namespace UniversalStickerAtlas
{
    public enum DecalMeshType
    {
        FlatQuad,
        FlatPlane,
        Solid3DPanel
    }

    [ExecuteAlways]
    [RequireComponent(typeof(MeshRenderer))]
    [AddComponentMenu("Universal Sticker Atlas/Sticker Decal")]
    public class RoadSignDecal : MonoBehaviour
    {
        [Header("Atlas Texture")]
        [Tooltip("The base diffuse/albedo texture atlas image containing sticker or road sign graphics.")]
        [SerializeField] private Texture2D atlasTexture;

        [Header("Normal Map Settings")]
        [Tooltip("Optional normal map atlas matching the base texture layout for realistic 3D surface detail.")]
        [SerializeField] private Texture2D normalMap;

        [Tooltip("Intensity multiplier for the normal map bumpiness depth.")]
        [Range(0f, 5f)]
        [SerializeField] private float normalScale = 1.0f;

        [Header("Color Key Background Removal")]
        [Tooltip("Enable chroma key background removal to make solid background colors (e.g. solid black or solid white) transparent.")]
        [SerializeField] private bool useColorKey = false;

        [Tooltip("The solid background color to key out and make transparent (e.g. Black (0,0,0) or White (1,1,1)).")]
        [SerializeField] private Color colorKey = Color.black;

        [Tooltip("Color distance tolerance threshold for background removal (higher values remove more color variations).")]
        [Range(0f, 1f)]
        [SerializeField] private float colorKeyTolerance = 0.1f;

        [Tooltip("Edge softness for smooth alpha transition when keying out solid backgrounds.")]
        [Range(0.001f, 0.5f)]
        [SerializeField] private float colorKeySoftness = 0.05f;

        [Header("Mode & Region")]
        [Tooltip("Enable Grid Mode to select sub-textures by cell column/row grid index instead of manual rect coordinates.")]
        [SerializeField] private bool useGrid = false;

        [Tooltip("Number of horizontal grid columns in the texture atlas.")]
        [SerializeField] private int gridCols = 4;

        [Tooltip("Number of vertical grid rows in the texture atlas.")]
        [SerializeField] private int gridRows = 4;

        [Tooltip("0-based index of the target grid cell to display.")]
        [SerializeField] private int cellIndex = 0;

        [Tooltip("Normalized 0..1 UV sub-rectangle (X-Offset, Y-Offset, Width, Height) for custom cropping.")]
        [SerializeField] private Rect signRect = new Rect(0f, 0f, 1f, 1f);

        [Header("Appearance & Snapping")]
        [Tooltip("Color tint multiplied with the texture atlas image.")]
        [SerializeField] private Color tintColor = Color.white;

        [Tooltip("Surface lift offset distance along normal to prevent Z-fighting flickering on flat meshes.")]
        [Range(-0.05f, 0.05f)]
        [SerializeField] private float zOffset = 0.002f;

        [Tooltip("Search radius range (in meters) for 360-degree surface raycast snapping.")]
        [Range(1f, 50f)]
        [SerializeField] private float raycastDistance = 15.0f;

        [Header("Scale & Aspect")]
        [Tooltip("Automatically scale the GameObject transform to match the aspect ratio of the selected sub-texture crop.")]
        [SerializeField] private bool autoScaleTransform = true;

        [Tooltip("Target physical world width (in meters) for the sticker quad/plane.")]
        [SerializeField] private float targetWidth = 2.5f;

        private MeshRenderer meshRenderer;
        private MaterialPropertyBlock propBlock;

        private List<RaycastHit> cachedHits = new List<RaycastHit>();
        private int currentHitIndex = 0;

        public Texture2D AtlasTexture
        {
            get => atlasTexture;
            set { atlasTexture = value; ApplyProperties(); }
        }

        public Texture2D NormalMap
        {
            get => normalMap;
            set { normalMap = value; ApplyProperties(); }
        }

        public float NormalScale
        {
            get => normalScale;
            set { normalScale = value; ApplyProperties(); }
        }

        public bool UseColorKey
        {
            get => useColorKey;
            set { useColorKey = value; ApplyProperties(); }
        }

        public Color ColorKey
        {
            get => colorKey;
            set { colorKey = value; ApplyProperties(); }
        }

        public float ColorKeyTolerance
        {
            get => colorKeyTolerance;
            set { colorKeyTolerance = value; ApplyProperties(); }
        }

        public float ColorKeySoftness
        {
            get => colorKeySoftness;
            set { colorKeySoftness = value; ApplyProperties(); }
        }

        public bool UseGrid
        {
            get => useGrid;
            set { useGrid = value; ApplyProperties(); }
        }

        public int GridCols
        {
            get => gridCols;
            set { gridCols = Mathf.Max(1, value); ApplyProperties(); }
        }

        public int GridRows
        {
            get => gridRows;
            set { gridRows = Mathf.Max(1, value); ApplyProperties(); }
        }

        public int CellIndex
        {
            get => cellIndex;
            set { cellIndex = Mathf.Clamp(value, 0, Mathf.Max(1, gridCols * gridRows - 1)); ApplyProperties(); }
        }

        public Rect SignRect
        {
            get => signRect;
            set { signRect = value; ApplyProperties(); }
        }

        public Color TintColor
        {
            get => tintColor;
            set { tintColor = value; ApplyProperties(); }
        }

        public float ZOffset
        {
            get => zOffset;
            set { zOffset = value; ApplyProperties(); }
        }

        public float RaycastDistance
        {
            get => raycastDistance;
            set { raycastDistance = Mathf.Max(0.5f, value); }
        }

        public float TargetWidth
        {
            get => targetWidth;
            set { targetWidth = value; ApplyProperties(); }
        }

        public int DetectedHitsCount => cachedHits.Count;
        public int CurrentHitIndex => currentHitIndex;

        private void OnEnable()
        {
            ApplyProperties();
        }

        private void OnValidate()
        {
            ApplyProperties();
        }

        public void ApplyProperties()
        {
            if (meshRenderer == null)
                meshRenderer = GetComponent<MeshRenderer>();

            if (meshRenderer == null)
                return;

            if (propBlock == null)
                propBlock = new MaterialPropertyBlock();

            meshRenderer.GetPropertyBlock(propBlock);

            if (atlasTexture != null)
            {
                propBlock.SetTexture("_BaseMap", atlasTexture);
            }

            if (normalMap != null)
            {
                propBlock.SetTexture("_BumpMap", normalMap);
                propBlock.SetFloat("_BumpScale", normalScale);
                propBlock.SetFloat("_EnableNormalMap", 1.0f);
            }
            else
            {
                propBlock.SetFloat("_EnableNormalMap", 0.0f);
            }

            propBlock.SetFloat("_UseColorKey", useColorKey ? 1.0f : 0.0f);
            propBlock.SetColor("_ColorKey", colorKey);
            propBlock.SetFloat("_ColorKeyTolerance", colorKeyTolerance);
            propBlock.SetFloat("_ColorKeySoftness", colorKeySoftness);

            propBlock.SetColor("_BaseColor", tintColor);
            propBlock.SetFloat("_UseGrid", useGrid ? 1.0f : 0.0f);
            propBlock.SetInt("_GridCols", Mathf.Max(1, gridCols));
            propBlock.SetInt("_GridRows", Mathf.Max(1, gridRows));
            propBlock.SetInt("_CellIndex", cellIndex);
            propBlock.SetVector("_SignRect", new Vector4(signRect.x, signRect.y, signRect.width, signRect.height));
            propBlock.SetFloat("_ZOffset", zOffset);

            meshRenderer.SetPropertyBlock(propBlock);

            if (meshRenderer.sharedMaterial != null)
            {
                if (normalMap != null)
                    meshRenderer.sharedMaterial.EnableKeyword("_NORMALMAP");
            }

            if (autoScaleTransform)
            {
                AdjustTransformAspect();
            }
        }

        public void AdjustTransformAspect()
        {
            float cropW = useGrid ? (1.0f / Mathf.Max(1, gridCols)) : Mathf.Max(0.001f, signRect.width);
            float cropH = useGrid ? (1.0f / Mathf.Max(1, gridRows)) : Mathf.Max(0.001f, signRect.height);

            float aspect = cropW / cropH;

            MeshFilter filter = GetComponent<MeshFilter>();
            bool isPlaneMesh = (filter != null && filter.sharedMesh != null && filter.sharedMesh.name.Contains("Plane"));
            float meshScaleMultiplier = isPlaneMesh ? 0.1f : 1.0f;

            float currentWidth = targetWidth * meshScaleMultiplier;
            float currentHeight = (targetWidth / aspect) * meshScaleMultiplier;

            Vector3 scale = transform.localScale;
            scale.x = currentWidth;
            scale.z = currentHeight;
            transform.localScale = scale;
        }

        /// <summary>
        /// Align object position and orientation to match any surface normal (ground, vertical walls, billboards).
        /// </summary>
        public void AlignToSurface(Vector3 position, Vector3 surfaceNormal, float offsetDistance = 0.002f)
        {
            transform.position = position + surfaceNormal * offsetDistance;

            if (surfaceNormal != Vector3.zero)
            {
                MeshFilter filter = GetComponent<MeshFilter>();
                bool isPlaneMesh = (filter != null && filter.sharedMesh != null && filter.sharedMesh.name.Contains("Plane"));

                if (isPlaneMesh)
                {
                    transform.rotation = Quaternion.FromToRotation(Vector3.up, surfaceNormal);
                }
                else
                {
                    transform.rotation = Quaternion.FromToRotation(Vector3.back, surfaceNormal);
                }
            }
        }

        /// <summary>
        /// 360-degree Spherical Raycast Snapping to find the absolute nearest surface face.
        /// </summary>
        public bool SnapToNearestSurface()
        {
            cachedHits.Clear();
            currentHitIndex = 0;

            Vector3 origin = transform.position;

            EnsureNearbyColliders(origin, raycastDistance);

            List<Vector3> rayDirections = new List<Vector3>();
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        if (x == 0 && y == 0 && z == 0) continue;

                        Vector3 dirWorld = new Vector3(x, y, z).normalized;
                        Vector3 dirLocal = transform.TransformDirection(dirWorld).normalized;

                        if (!rayDirections.Contains(dirWorld)) rayDirections.Add(dirWorld);
                        if (!rayDirections.Contains(dirLocal)) rayDirections.Add(dirLocal);
                    }
                }
            }

            foreach (Vector3 dir in rayDirections)
            {
                RaycastHit[] rayHits = Physics.RaycastAll(origin, dir, raycastDistance, ~0, QueryTriggerInteraction.Ignore);
                foreach (RaycastHit hit in rayHits)
                {
                    if (hit.transform != transform && !hit.transform.IsChildOf(transform))
                    {
                        AddHitIfDistinct(hit);
                    }
                }
            }

            cachedHits.Sort((a, b) => Vector3.Distance(origin, a.point).CompareTo(Vector3.Distance(origin, b.point)));

            if (cachedHits.Count > 0)
            {
                ApplyHitIndex(0);
                return true;
            }

            return false;
        }

        private void AddHitIfDistinct(RaycastHit hit)
        {
            bool isDuplicate = false;
            foreach (RaycastHit existing in cachedHits)
            {
                if (Vector3.Distance(existing.point, hit.point) < 0.05f)
                {
                    isDuplicate = true;
                    break;
                }
            }
            if (!isDuplicate)
            {
                cachedHits.Add(hit);
            }
        }

        private void EnsureNearbyColliders(Vector3 center, float radius)
        {
            MeshRenderer[] renderers = FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
            foreach (MeshRenderer r in renderers)
            {
                if (r.transform == transform || r.transform.IsChildOf(transform)) continue;

                if (Vector3.Distance(r.bounds.center, center) <= radius + r.bounds.extents.magnitude)
                {
                    if (r.GetComponent<Collider>() == null)
                    {
                        MeshFilter mf = r.GetComponent<MeshFilter>();
                        if (mf != null && mf.sharedMesh != null)
                        {
                            MeshCollider mc = r.gameObject.AddComponent<MeshCollider>();
                            mc.sharedMesh = mf.sharedMesh;
                        }
                        else
                        {
                            r.gameObject.AddComponent<BoxCollider>();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Cycle to the next detected surface face if multiple faces are in range.
        /// </summary>
        public void CycleNextSurfaceFace()
        {
            if (cachedHits.Count == 0)
            {
                SnapToNearestSurface();
                return;
            }

            currentHitIndex = (currentHitIndex + 1) % cachedHits.Count;
            ApplyHitIndex(currentHitIndex);
        }

        /// <summary>
        /// Flip the sticker rotation 180 degrees (opposite side face).
        /// </summary>
        public void FlipSurfaceNormal()
        {
            transform.Rotate(0, 180, 0, Space.Self);
        }

        private void ApplyHitIndex(int index)
        {
            if (index >= 0 && index < cachedHits.Count)
            {
                RaycastHit hit = cachedHits[index];
                AlignToSurface(hit.point, hit.normal, zOffset);
            }
        }
    }
}
