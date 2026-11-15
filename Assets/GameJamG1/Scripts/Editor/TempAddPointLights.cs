using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class TempAddPointLights
{
    private const string ScenePath = "Assets/GameJamG1/Scenes/Prototypes/Prototype.unity";
    private const string SelfPath = "Assets/GameJamG1/Scripts/Editor/TempAddPointLights.cs";
    private const string RootName = "Point Lights";
    private static readonly Regex FixtureName = new Regex(@"^(Foco|Lampara\d*)(\.\d+)?$");

    [MenuItem("Tools/Temp/Add Point Lights")]
    public static void Run()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var previous = GameObject.Find(RootName);
        if (previous != null)
        {
            Object.DestroyImmediate(previous);
        }

        var root = new GameObject(RootName);
        var fixtures = Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .Where(t => FixtureName.IsMatch(t.name))
            .OrderBy(t => t.name)
            .ToList();

        var created = 0;
        foreach (var fixture in fixtures)
        {
            var renderers = fixture.GetComponentsInChildren<Renderer>(false);
            if (renderers.Length == 0)
            {
                continue;
            }

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            var position = bounds.size.y > 1f
                ? new Vector3(bounds.center.x, bounds.max.y - 0.25f, bounds.center.z)
                : bounds.center;

            var lightObject = new GameObject("PL_" + fixture.name);
            lightObject.transform.SetParent(root.transform, true);
            lightObject.transform.position = position;

            var pointLight = lightObject.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.color = new Color(0.75f, 0.85f, 1f);
            pointLight.intensity = 1f;
            pointLight.range = 5f;
            pointLight.shadows = LightShadows.None;
            lightObject.AddComponent<UniversalAdditionalLightData>();
            created++;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("TempAddPointLights: " + created + " luces creadas de " + fixtures.Count + " fixtures");
        AssetDatabase.DeleteAsset(SelfPath);
    }
}
