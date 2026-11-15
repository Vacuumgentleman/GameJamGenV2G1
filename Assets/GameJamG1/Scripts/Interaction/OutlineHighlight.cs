using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameJamG1.Interaction
{
    public class OutlineHighlight : MonoBehaviour
    {
        private static readonly int ColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int WidthId = Shader.PropertyToID("_OutlineWidth");

        [Header("Style")]
        [Tooltip("Material using the GameJamG1/OutlineInvertedHull shader. Used as a template.")]
        [SerializeField] private Material outlineMaterial;
        [SerializeField] private Color color = Color.white;
        [Tooltip("Outline thickness in world units.")]
        [SerializeField, Min(0f)] private float thickness = 0.02f;

        [Header("Targets")]
        [Tooltip("Renderers that get the outline. Leave empty to use every MeshRenderer under this object.")]
        [SerializeField] private Renderer[] renderers;

        private readonly List<(Renderer renderer, MeshFilter filter)> targets = new();
        private Material runtimeMaterial;
        private bool highlighted;

        public void SetHighlight(bool value)
        {
            highlighted = value;
        }

        private void Awake()
        {
            BuildTargets();
            CreateMaterial();
        }

        private void LateUpdate()
        {
            if (!highlighted || runtimeMaterial == null)
            {
                return;
            }

            foreach (var (renderer, filter) in targets)
            {
                if (renderer == null || filter == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                {
                    continue;
                }

                DrawOutline(filter);
            }
        }

        private void OnDestroy()
        {
            if (runtimeMaterial != null)
            {
                Destroy(runtimeMaterial);
            }
        }

        private void DrawOutline(MeshFilter filter)
        {
            var mesh = filter.sharedMesh;
            if (mesh == null)
            {
                return;
            }

            var matrix = filter.transform.localToWorldMatrix;
            var layer = filter.gameObject.layer;

            for (var submesh = 0; submesh < mesh.subMeshCount; submesh++)
            {
                Graphics.DrawMesh(mesh, matrix, runtimeMaterial, layer, null, submesh, null, ShadowCastingMode.Off, false);
            }
        }

        private void BuildTargets()
        {
            targets.Clear();

            var source = renderers != null && renderers.Length > 0
                ? renderers
                : GetComponentsInChildren<MeshRenderer>(true);

            foreach (var candidate in source)
            {
                if (candidate != null && candidate.TryGetComponent(out MeshFilter filter))
                {
                    targets.Add((candidate, filter));
                }
            }
        }

        private void CreateMaterial()
        {
            if (outlineMaterial == null)
            {
                Debug.LogError($"{name}: OutlineHighlight has no outline material assigned.", this);
                enabled = false;
                return;
            }

            runtimeMaterial = new Material(outlineMaterial) { hideFlags = HideFlags.HideAndDontSave };
            ApplyStyle();
        }

        private void ApplyStyle()
        {
            if (runtimeMaterial == null)
            {
                return;
            }

            runtimeMaterial.SetColor(ColorId, color);
            runtimeMaterial.SetFloat(WidthId, thickness);
        }

        private void OnValidate()
        {
            ApplyStyle();

            if (outlineMaterial == null)
            {
                Debug.LogWarning($"{name}: OutlineHighlight has no outline material assigned.", this);
            }
        }

#if UNITY_EDITOR
        private void Reset()
        {
            foreach (var guid in AssetDatabase.FindAssets("Outline_InvertedHull t:Material"))
            {
                outlineMaterial = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                break;
            }
        }
#endif
    }
}
