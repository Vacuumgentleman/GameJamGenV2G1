using System;
using GameJamG1.Barricade;
using GameJamG1.Dialogue;
using GameJamG1.Hallucination;
using GameJamG1.Interaction;
using GameJamG1.Items;
using GameJamG1.Objects;
using GameJamG1.Sequences;
using GameJamG1.Story;
using UnityEditor;
using UnityEngine;

namespace GameJamG1.EditorTools
{
    public static class PrefabLibraryBuilder
    {
        private const string PrefabRoot = "Assets/GameJamG1/Prefabs";
        private const string GameplayFolder = PrefabRoot + "/Gameplay";
        private const string InteractablesFolder = GameplayFolder + "/Interactables";
        private const string ItemsFolder = GameplayFolder + "/Items";
        private const string BarricadeFolder = GameplayFolder + "/Barricade";
        private const string SystemsFolder = GameplayFolder + "/Systems";
        private const string UIFolder = PrefabRoot + "/UI";

        private static Material outline;

        internal static void EnsureLibrary()
        {
            Run();
        }

        private static void Run()
        {
            outline = EditorAssetUtility.FindAsset<Material>("Outline_InvertedHull");

            EditorAssetUtility.EnsureFolder(GameplayFolder, "Interactables");
            EditorAssetUtility.EnsureFolder(GameplayFolder, "Items");
            EditorAssetUtility.EnsureFolder(GameplayFolder, "Barricade");
            EditorAssetUtility.EnsureFolder(GameplayFolder, "Systems");

            Make(InteractablesFolder, "Interactable_Basic", BuildBasic);
            Make(InteractablesFolder, "Examinable", BuildExaminable);
            Make(InteractablesFolder, "Door", BuildDoor);
            Make(InteractablesFolder, "Drawer", BuildDrawer);
            Make(InteractablesFolder, "StoryTrigger_Zone", BuildZoneTrigger);
            Make(InteractablesFolder, "Peephole", BuildPeephole);

            Make(ItemsFolder, "MatchPickup", BuildMatchPickup);

            Make(BarricadeFolder, "Chair_Breakable", BuildBreakableChair);
            Make(BarricadeFolder, "Door_Barricade", BuildBarricadeDoor);

            Make(SystemsFolder, "ScreenFader", BuildScreenFader);
            Make(SystemsFolder, "HallucinationManager", BuildHallucinationManager);
            Make(UIFolder, "BreathingCanvas", BuildBreathingCanvas);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Prefab library created in " + GameplayFolder);
        }

        private static void Make(string folder, string prefabName, Func<GameObject> build)
        {
            var path = $"{folder}/{prefabName}.prefab";

            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                return;
            }

            var instance = build();
            if (instance == null)
            {
                return;
            }

            instance.name = prefabName;
            PrefabUtility.SaveAsPrefabAsset(instance, path);
            UnityEngine.Object.DestroyImmediate(instance);
        }

        private static GameObject BuildBasic()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localScale = Vector3.one * 0.8f;
            var interactable = cube.AddComponent<InteractableBase>();
            EditorAssetUtility.SetString(interactable, "promptText", "Examinar");
            AddOutline(cube);
            return cube;
        }

        private static GameObject BuildExaminable()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localScale = Vector3.one * 0.5f;
            var examinable = cube.AddComponent<ExaminableObject>();
            EditorAssetUtility.SetString(examinable, "promptText", "Examinar");
            EditorAssetUtility.SetBool(examinable, "oneShot", true);
            AddOutline(cube);
            return cube;
        }

        private static GameObject BuildDoor()
        {
            return CreateDoorHinge("Door");
        }

        private static GameObject BuildDrawer()
        {
            var root = new GameObject("Drawer");

            var cabinet = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabinet.name = "Cabinet";
            cabinet.transform.SetParent(root.transform, false);
            cabinet.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            cabinet.transform.localScale = new Vector3(1f, 0.8f, 0.7f);

            var drawerGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            drawerGo.name = "DrawerFront";
            drawerGo.transform.SetParent(root.transform, false);
            drawerGo.transform.localPosition = new Vector3(0f, 0.55f, -0.4f);
            drawerGo.transform.localScale = new Vector3(0.8f, 0.2f, 0.1f);

            var drawer = drawerGo.AddComponent<Drawer>();
            EditorAssetUtility.SetVector3(drawer, "openOffset", new Vector3(0f, 0f, -0.4f));
            AddOutline(drawerGo);
            return root;
        }

        private static GameObject BuildZoneTrigger()
        {
            var zone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            zone.transform.localScale = new Vector3(1.6f, 0.05f, 1.6f);
            zone.GetComponent<BoxCollider>().isTrigger = true;
            UnityEngine.Object.DestroyImmediate(zone.GetComponent<MeshRenderer>());
            UnityEngine.Object.DestroyImmediate(zone.GetComponent<MeshFilter>());

            var trigger = zone.AddComponent<StoryTrigger>();
            EditorAssetUtility.SetBool(trigger, "triggerOnZoneEnter", true);
            return zone;
        }

        private static GameObject BuildPeephole()
        {
            var root = new GameObject("Peephole");

            var plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = "Plate";
            plate.transform.SetParent(root.transform, false);
            plate.transform.localScale = new Vector3(0.3f, 0.3f, 0.05f);

            var cameraGo = new GameObject("PeepholeCamera");
            cameraGo.transform.SetParent(root.transform, false);
            cameraGo.transform.localPosition = new Vector3(0f, 0f, 0.3f);
            var peepholeCamera = cameraGo.AddComponent<Camera>();
            peepholeCamera.fieldOfView = 90f;
            peepholeCamera.enabled = false;

            var sequenceGo = new GameObject("Sequence");
            sequenceGo.transform.SetParent(root.transform, false);
            var sequence = sequenceGo.AddComponent<TimedEventSequence>();

            var peephole = plate.AddComponent<PeepholeSequence>();
            EditorAssetUtility.SetString(peephole, "promptText", "Mirar por la mirilla");
            EditorAssetUtility.SetBool(peephole, "oneShot", true);
            EditorAssetUtility.SetObject(peephole, "peepholeCamera", peepholeCamera);
            EditorAssetUtility.SetObject(peephole, "sequence", sequence);
            AddOutline(plate);
            return root;
        }

        private static GameObject BuildMatchPickup()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localScale = new Vector3(0.2f, 0.1f, 0.1f);
            var pickup = cube.AddComponent<MatchPickup>();
            EditorAssetUtility.SetString(pickup, "promptText", "Tomar cerillas");
            EditorAssetUtility.SetBool(pickup, "oneShot", true);
            EditorAssetUtility.SetBool(pickup, "hideWhenConsumed", true);
            EditorAssetUtility.SetInt(pickup, "amount", 3);
            AddOutline(cube);
            return cube;
        }

        private static GameObject BuildBreakableChair()
        {
            var plankPrefab = EditorAssetUtility.FindAsset<GameObject>("Plank_World");
            var chair = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chair.transform.localScale = Vector3.one * 0.5f;

            var furniture = chair.AddComponent<BreakableFurniture>();
            EditorAssetUtility.SetString(furniture, "promptText", "Romper silla");
            EditorAssetUtility.SetBool(furniture, "oneShot", true);
            EditorAssetUtility.SetBool(furniture, "hideWhenConsumed", true);

            if (plankPrefab != null)
            {
                EditorAssetUtility.SetObject(furniture, "plankPrefab", plankPrefab.GetComponent<EquippableItem>());
            }

            AddOutline(chair);
            return chair;
        }

        private static GameObject BuildBarricadeDoor()
        {
            var root = new GameObject("Door_Barricade");
            var doorRoot = CreateDoorHinge("Door");
            doorRoot.transform.SetParent(root.transform, false);
            var door = doorRoot.GetComponent<Door>();

            var blocked = EditorAssetUtility.FindAsset<DialogueSequence>("Test_DoorBlocked");
            if (blocked != null)
            {
                EditorAssetUtility.SetObject(door, "barricadedDialogue", blocked);
            }

            var point = new GameObject("BarricadePoint");
            point.transform.SetParent(root.transform, false);
            point.transform.localPosition = new Vector3(0.5f, 1f, -0.09f);
            point.AddComponent<BoxCollider>().size = new Vector3(1f, 2f, 0.3f);

            var meshes = new UnityEngine.Object[3];
            for (var i = 0; i < meshes.Length; i++)
            {
                var plank = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plank.name = $"BarricadePlank_{i + 1}";
                UnityEngine.Object.DestroyImmediate(plank.GetComponent<Collider>());
                plank.transform.SetParent(point.transform, false);
                plank.transform.localPosition = new Vector3(0f, -0.6f + i * 0.6f, 0f);
                plank.transform.localScale = new Vector3(0.95f, 0.14f, 0.05f);
                meshes[i] = plank;
            }

            var barricade = point.AddComponent<BarricadePoint>();
            EditorAssetUtility.SetObject(barricade, "plankItem", EditorAssetUtility.FindAsset<ItemData>("Plank"));
            EditorAssetUtility.SetObjects(barricade, "planks", meshes);
            EditorAssetUtility.SetObject(barricade, "door", door);
            EditorAssetUtility.SetInt(barricade, "requiredPlanks", meshes.Length);
            EditorAssetUtility.SetObject(barricade, "ghostMaterial", BarricadeGhostMaterial.EnsureGhostMaterial());

            var doneFlag = EditorAssetUtility.FindAsset<StoryFlag>("PuertaBarricadaLista");
            if (doneFlag != null)
            {
                EditorAssetUtility.SetObjects(barricade, "completionFlags", doneFlag);
            }

            var needPlank = EditorAssetUtility.FindAsset<DialogueSequence>("Test_NeedPlank");
            if (needPlank != null)
            {
                EditorAssetUtility.SetObject(barricade, "needPlankDialogue", needPlank);
            }

            return root;
        }

        private static GameObject BuildScreenFader()
        {
            var existing = UnityEngine.Object.FindFirstObjectByType<ScreenFader>();
            var fader = SequencesSetupMenu.EnsureScreenFader();
            return ExtractFromScene(fader.gameObject, existing != null);
        }

        private static GameObject BuildHallucinationManager()
        {
            var existing = UnityEngine.Object.FindFirstObjectByType<HallucinationManager>();
            var hadLabel = UnityEngine.Object.FindFirstObjectByType<HallucinationLevelLabel>() != null;
            var manager = HallucinationSetupMenu.EnsureHallucination();
            var result = ExtractFromScene(manager.gameObject, existing != null);

            if (!hadLabel)
            {
                var label = UnityEngine.Object.FindFirstObjectByType<HallucinationLevelLabel>();
                if (label != null)
                {
                    UnityEngine.Object.DestroyImmediate(label.gameObject);
                }
            }

            return result;
        }

        private static GameObject BuildBreathingCanvas()
        {
            var existing = UnityEngine.Object.FindFirstObjectByType<BreathingMinigame>();
            var minigame = HallucinationSetupMenu.EnsureBreathingUI();
            var canvas = minigame.GetComponentInParent<Canvas>().gameObject;
            return ExtractFromScene(canvas, existing != null);
        }

        private static GameObject ExtractFromScene(GameObject source, bool keepSource)
        {
            if (keepSource)
            {
                var copy = UnityEngine.Object.Instantiate(source);
                copy.name = source.name;
                return copy;
            }

            return source;
        }

        private static GameObject CreateDoorHinge(string hingeName)
        {
            var hinge = new GameObject(hingeName);

            var panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = "Panel";
            panel.transform.SetParent(hinge.transform, false);
            panel.transform.localPosition = new Vector3(0.5f, 1f, 0f);
            panel.transform.localScale = new Vector3(1f, 2f, 0.1f);

            var door = hinge.AddComponent<Door>();
            AddOutline(hinge);
            return hinge;
        }

        private static void AddOutline(GameObject target)
        {
            var highlight = target.AddComponent<OutlineHighlight>();
            EditorAssetUtility.SetObject(highlight, "outlineMaterial", outline);
        }
    }
}
