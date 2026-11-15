using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace GameJamG1.EditorTools
{
    internal static class BarricadeGhostMaterial
    {
        private const string Path = "Assets/GameJamG1/Art/Materials/Barricade_Ghost.mat";

        public static Material EnsureGhostMaterial()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(Path);
            if (existing != null)
            {
                return existing;
            }

            var material = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { name = "Barricade_Ghost" };
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
            material.SetColor("_BaseColor", new Color(0.4f, 0.9f, 1f, 0.35f));

            AssetDatabase.CreateAsset(material, Path);
            AssetDatabase.SaveAssets();
            return material;
        }
    }
}
