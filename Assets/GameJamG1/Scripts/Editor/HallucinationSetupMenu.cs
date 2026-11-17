using System.IO;
using GameJamG1.Hallucination;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameJamG1.EditorTools
{
    public static class HallucinationSetupMenu
    {
        private const string Root = "Assets/GameJamG1";
        private const string ProfileFolder = Root + "/ScriptableObjects/HallucinationProfiles";

        internal static HallucinationManager EnsureHallucination()
        {
            var existing = Object.FindFirstObjectByType<HallucinationManager>();
            if (existing != null)
            {
                return existing;
            }

            EditorAssetUtility.EnsureFolder(Root + "/ScriptableObjects", "HallucinationProfiles");

            var levels = new Object[4];
            for (var level = 0; level < levels.Length; level++)
            {
                levels[level] = EnsureLevelProfile(level);
            }

            var go = new GameObject("HallucinationManager");
            var manager = go.AddComponent<HallucinationManager>();

            var volumeGo = new GameObject("HallucinationVolume");
            volumeGo.transform.SetParent(go.transform, false);
            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10f;
            volume.weight = 0f;

            var audioGo = new GameObject("AmbientAudio");
            audioGo.transform.SetParent(go.transform, false);
            var source = audioGo.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;

            EditorAssetUtility.SetObject(manager, "volume", volume);
            EditorAssetUtility.SetObject(manager, "ambientSource", source);
            EditorAssetUtility.SetObjects(manager, "profiles", levels);

            EnsureLevelLabel();

            Undo.RegisterCreatedObjectUndo(go, "Create Hallucination System");
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            return manager;
        }

        internal static BreathingMinigame EnsureBreathingUI()
        {
            var existing = Object.FindFirstObjectByType<BreathingMinigame>();
            if (existing != null)
            {
                return existing;
            }

            var noise = SequencesSetupMenu.EnsureNoiseChannel();
            var ring = EnsureRingSprite();
            var knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            var bar = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            var canvasGo = new GameObject("BreathingCanvas", typeof(Canvas), typeof(CanvasScaler));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var root = CreateRect("BreathingMinigame", canvasGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var panelRect = CreateRect("Panel", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500f, 600f));
            var group = panelRect.gameObject.AddComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;

            var zoneMax = CreateImage("ZoneMax", panelRect, ring, new Vector2(0f, 40f), new Vector2(360f, 360f), new Color(1f, 1f, 1f, 0.9f));
            var zoneMin = CreateImage("ZoneMin", panelRect, ring, new Vector2(0f, 40f), new Vector2(360f, 360f), new Color(1f, 1f, 1f, 0.9f));
            var circle = CreateImage("Circle", panelRect, knob, new Vector2(0f, 40f), new Vector2(360f, 360f), new Color(1f, 1f, 1f, 0.5f));

            var back = CreateImage("ProgressBack", panelRect, bar, new Vector2(0f, -190f), new Vector2(280f, 14f), new Color(0f, 0f, 0f, 0.6f));
            var fill = CreateImage("ProgressFill", back.transform, bar, Vector2.zero, new Vector2(280f, 14f), Color.white);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillAmount = 0f;

            var hintRect = CreateRect("Hint", panelRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -230f), new Vector2(480f, 40f));
            var hint = hintRect.gameObject.AddComponent<TextMeshProUGUI>();
            hint.fontSize = 24f;
            hint.alignment = TextAlignmentOptions.Center;
            hint.raycastTarget = false;

            var minigame = root.gameObject.AddComponent<BreathingMinigame>();
            EditorAssetUtility.SetObject(minigame, "panel", group);
            EditorAssetUtility.SetObject(minigame, "circle", circle.rectTransform);
            EditorAssetUtility.SetObject(minigame, "circleImage", circle);
            EditorAssetUtility.SetObject(minigame, "zoneMinRing", zoneMin.rectTransform);
            EditorAssetUtility.SetObject(minigame, "zoneMaxRing", zoneMax.rectTransform);
            EditorAssetUtility.SetObject(minigame, "progressFill", fill);
            EditorAssetUtility.SetObject(minigame, "hintLabel", hint);
            EditorAssetUtility.SetObject(minigame, "noiseChannel", noise);

            panelRect.gameObject.SetActive(false);

            Undo.RegisterCreatedObjectUndo(canvasGo, "Create Breathing UI");
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            return minigame;
        }

        private static HallucinationLevelProfile EnsureLevelProfile(int level)
        {
            var path = $"{ProfileFolder}/Hallucination_Level{level}.asset";
            var isNew = AssetDatabase.LoadAssetAtPath<HallucinationLevelProfile>(path) == null;
            var profile = EditorAssetUtility.LoadOrCreate<HallucinationLevelProfile>(path);

            if (!isNew)
            {
                return profile;
            }

            EditorAssetUtility.SetString(profile, "description", level == 0 ? "Sin efecto." : $"Alucinacion nivel {level}. Ajustar el volume profile.");

            if (level > 0)
            {
                EditorAssetUtility.SetObject(profile, "volumeProfile", CreateVolumeProfile(level));
            }

            ConfigureLevelProfile(profile, level);

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }

        private static VolumeProfile CreateVolumeProfile(int level)
        {
            var path = $"{ProfileFolder}/Hallucination_Level{level}_Volume.asset";
            var existing = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (existing != null)
            {
                return existing;
            }

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);
            ConfigureVolumeProfile(profile, level);
            return profile;
        }

        // Alucinacion = distorsion y ruido por encima del volumen base de la escena, nunca oscuridad.
        // Solo se activa el override de los parametros listados: Add<T>(true) activaria tambien
        // postExposure/contrast y anularia el +exposure del volumen base, dejando la escena negra.
        // Grano y aberracion deben superar los del volumen base (0.51 y 0.2) para notarse.
        internal static void ConfigureVolumeProfile(VolumeProfile profile, int level)
        {
            var t = Mathf.Clamp01(level / 3f);

            foreach (var component in profile.components.ToArray())
            {
                profile.components.Remove(component);
                Object.DestroyImmediate(component, true);
            }

            var grain = profile.Add<FilmGrain>();
            grain.type.Override(FilmGrainLookup.Medium4);
            grain.intensity.Override(Mathf.Lerp(0.6f, 0.95f, t));
            grain.response.Override(0.6f);
            AssetDatabase.AddObjectToAsset(grain, profile);

            var aberration = profile.Add<ChromaticAberration>();
            aberration.intensity.Override(Mathf.Lerp(0.3f, 0.8f, t));
            AssetDatabase.AddObjectToAsset(aberration, profile);

            var lens = profile.Add<LensDistortion>();
            lens.intensity.Override(Mathf.Lerp(-0.1f, -0.3f, t));
            lens.scale.Override(Mathf.Lerp(1.02f, 1.08f, t));
            AssetDatabase.AddObjectToAsset(lens, profile);

            var colors = profile.Add<ColorAdjustments>();
            colors.saturation.Override(Mathf.Lerp(-25f, -40f, t));
            AssetDatabase.AddObjectToAsset(colors, profile);

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
        }

        internal static void ConfigureLevelProfile(HallucinationLevelProfile profile, int level)
        {
            var so = new SerializedObject(profile);
            so.FindProperty("distortionWobble").floatValue = level <= 0 ? 0f : Mathf.Lerp(0.05f, 0.12f, Mathf.Clamp01(level / 3f));
            so.FindProperty("distortionWobbleSpeed").floatValue = 0.4f + 0.15f * level;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static void EnsureLevelLabel()
        {
            var canvas = GameObject.Find("InteractionCanvas");
            if (canvas == null || Object.FindFirstObjectByType<HallucinationLevelLabel>() != null)
            {
                return;
            }

            var rect = CreateRect("HallucinationLabel", canvas.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(360f, 50f));
            rect.pivot = new Vector2(0f, 1f);

            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.fontSize = 32f;
            text.alignment = TextAlignmentOptions.Left;
            text.text = "Alucinacion: 0";
            text.raycastTarget = false;

            var label = rect.gameObject.AddComponent<HallucinationLevelLabel>();
            EditorAssetUtility.SetObject(label, "label", text);
        }

        private static Sprite EnsureRingSprite()
        {
            const string path = Root + "/Art/UI/Ring.png";
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null)
            {
                return existing;
            }

            EditorAssetUtility.EnsureFolder(Root + "/Art", "UI");

            const int size = 256;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var center = (size - 1) * 0.5f;

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / (size * 0.5f);
                    var outer = Mathf.Clamp01((1f - distance) * size * 0.5f);
                    var inner = Mathf.Clamp01((distance - 0.95f) * size * 0.5f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Min(outer, inner)));
                }
            }

            File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), path), texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;

            if (anchorMin == Vector2.zero && anchorMax == Vector2.one)
            {
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
            else
            {
                rect.anchoredPosition = position;
                rect.sizeDelta = size;
            }

            return rect;
        }

        private static Image CreateImage(string name, Transform parent, Sprite sprite, Vector2 position, Vector2 size, Color color)
        {
            var rect = CreateRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }
    }
}
