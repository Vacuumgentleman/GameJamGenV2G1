using GameJamG1.Dialogue;
using GameJamG1.Enemy;
using GameJamG1.Hallucination;
using GameJamG1.Interaction;
using GameJamG1.Items;
using GameJamG1.Objects;
using GameJamG1.Sequences;
using GameJamG1.Story;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace GameJamG1.EditorTools
{
    public static class PrefabTestRoomBuilder
    {
        private const string RootName = "PrefabTestRoom";
        private const string PrefabRoot = "Assets/GameJamG1/Prefabs";
        private const string DialogueFolder = "Assets/GameJamG1/ScriptableObjects/Dialogues/Test";
        private const float Column = 3f;
        private const float RowSpacing = 5f;

        private static Transform room;
        private static Vector3 origin;
        private static Vector3 forward;
        private static Vector3 right;
        private static Quaternion facing;

        [MenuItem("GameJamG1/Create Prefab Test Room")]
        private static void Build()
        {
            if (!HasSceneRequirements())
            {
                return;
            }

            PrefabLibraryBuilder.EnsureLibrary();

            var existing = GameObject.Find(RootName);
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing);
            }

            ComputeBasis();

            var root = new GameObject(RootName);
            root.transform.position = origin;
            Undo.RegisterCreatedObjectUndo(root, "Create Prefab Test Room");
            room = root.transform;

            CreateFloor();
            CreateSectionLabel(0, "INTERACTUABLES");
            BuildInteractables();
            CreateSectionLabel(1, "ITEMS");
            BuildItems();
            CreateSectionLabel(2, "BARRICADA");
            BuildBarricade();
            CreateSectionLabel(3, "CADENA DE HISTORIA");
            BuildStoryChain();
            CreateSectionLabel(4, "SISTEMAS");
            BuildSystems();
            EnsureMatchCounter();

            Selection.activeGameObject = root;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Prefab test room created.");
        }

        private static bool HasSceneRequirements()
        {
            if (Object.FindFirstObjectByType<PlayerInteractor>() != null &&
                Object.FindFirstObjectByType<DialogueManager>() != null &&
                Object.FindFirstObjectByType<StoryManager>() != null)
            {
                return true;
            }

            EditorUtility.DisplayDialog(
                "Sala de pruebas",
                "Abre una escena con el Player, el DialogueManager y el StoryManager (por ejemplo la escena principal) y vuelve a ejecutar.",
                "OK");
            return false;
        }

        private static void BuildInteractables()
        {
            var basic = Place(Interactables("Interactable_Basic"), Slot(0), 0f, 0.4f, "Interactable_Basic\nE muestra su dialogo");
            Configure(basic, "Examinar", Dialogue("Test_1_Basic", false, "Un objeto interactuable basico. Este es su dialogo."));

            var examinable = Place(Interactables("Examinable"), Slot(1), 0f, 0.25f, "Examinable\nSolo una vez");
            Configure(examinable, "Revisar", Dialogue("Test_2_OneShot", false, "Solo se puede usar una vez.", "Ya no responde ni se resalta."));

            Place(Interactables("Door"), Slot(2), 0f, 0f, "Door\nAbrir y cerrar");

            var drawer = Place(Interactables("Drawer"), Slot(3), 0f, 0f, "Drawer\nRevela una cerilla");
            var match = Place(PrefabPath("Gameplay/Items/MatchPickup"), Slot(3), 0f, 0.9f, null);
            EditorAssetUtility.SetObjects(drawer.GetComponentInChildren<Drawer>(), "contents", match);

            var zone = Place(Interactables("StoryTrigger_Zone"), Slot(4), 0f, 0.03f, "StoryTrigger_Zone\nPisa el cuadro");
            AddZoneMarker(zone.transform);
            EditorAssetUtility.SetObject(zone.GetComponent<StoryTrigger>(), "dialogueOnTrigger", Dialogue("Test_7_Zone", false, "Pise la zona. El trigger disparo este dialogo."));

            BuildPeephole(Slot(5), 0f);
        }

        private static void BuildPeephole(float x, float z)
        {
            var peephole = Place(Interactables("Peephole"), x, z, 1.6f, "Peephole\nFundido, camara y secuencia");
            var transform = peephole.transform;

            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Wall";
            wall.transform.SetParent(room, true);
            wall.transform.SetPositionAndRotation(transform.TransformPoint(0f, -0.35f, 0.1f), transform.rotation);
            wall.transform.localScale = new Vector3(3f, 2.5f, 0.2f);

            var enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "EnemyPlaceholder";
            Object.DestroyImmediate(enemy.GetComponent<Collider>());
            enemy.transform.SetParent(room, true);
            enemy.transform.SetPositionAndRotation(transform.TransformPoint(0f, -0.6f, 3.5f), transform.rotation);
            enemy.transform.localScale = new Vector3(0.8f, 1.2f, 0.8f);
            enemy.SetActive(false);

            var sequence = peephole.GetComponentInChildren<TimedEventSequence>();
            SetSteps(sequence, new[] { 0.8f, 1.5f }, new[] { "Aparece el enemigo", "Desaparece" });
            AddSetActive(sequence.Steps[0].OnReached, enemy, true);
            AddSetActive(sequence.Steps[1].OnReached, enemy, false);

            var peepholeInteractable = peephole.GetComponentInChildren<PeepholeSequence>();
            EditorAssetUtility.SetObject(peepholeInteractable, "endDialogue", Dialogue("Test_PeepholeEnd", false, "Que fue eso...", "Mejor no vuelvo a mirar."));
            SetFlags(peepholeInteractable, "flagsToSetOnInteract", "MirillaVista");
        }

        private static void BuildItems()
        {
            Place(Asset("Flashlight_World"), Slot(0), RowZ(1), 0.9f, "Flashlight_World\nF enciende, dura 120 s");
            Place(Asset("Candle_World"), Slot(1), RowZ(1), 0.9f, "Candle_World\nF gasta cerilla, correr la apaga");
            Place(Asset("Plank_World"), Slot(2), RowZ(1), 0.9f, "Plank_World\nSe coloca en la barricada");
            Place(Asset("Hammer_World"), Slot(3), RowZ(1), 0.9f, "Hammer_World\nObjeto de mano");
            Place(PrefabPath("Gameplay/Items/MatchPickup"), Slot(4), RowZ(1), 0.9f, "MatchPickup\n+3 cerillas");
            CreateLabel("Solo 1 objeto en la mano:\nrecoger otro cambia el actual", Point(Slot(5), RowZ(1), 1.6f));
        }

        private static void BuildBarricade()
        {
            var z = RowZ(2);
            Place(PrefabPath("Gameplay/Barricade/Chair_Breakable"), Slot(1), z, 0.25f, "Chair_Breakable\nSuelta 2 tablas");
            Place(PrefabPath("Gameplay/Barricade/Chair_Breakable"), Slot(2), z, 0.25f, null);
            Place(PrefabPath("Gameplay/Barricade/Door_Barricade"), Slot(4), z, 0f, "Door_Barricade\nRompe silla, agarra tabla, colocala");
            CreateLabel("Con la 1ra tabla la puerta ya no abre.\nCon 3 se pone PuertaBarricadaLista.", Point(Slot(0), z, 1.6f));
        }

        private static void BuildStoryChain()
        {
            var z = RowZ(3);

            var wake = Place(Interactables("Interactable_Basic"), Slot(0), z, 0.4f, "1. Despertar\nPone la flag Despierta");
            Configure(wake, "Despertar", Dialogue("Test_3_SetFlag", false, "Siento que algo cambio en la casa."));
            SetFlags(wake.GetComponent<InteractableBase>(), "flagsToSetOnInteract", "Despierta");

            var locked = Place(Interactables("Door"), Slot(1), z, 0f, "2. Puerta bloqueada\nExige la flag Despierta");
            var lockedInteractable = locked.GetComponent<InteractableBase>();
            SetFlags(lockedInteractable, "requiredFlags", "Despierta");
            EditorAssetUtility.SetObject(lockedInteractable, "lockedDialogue", Dialogue("Test_4_Locked", false, "Todavia no. Primero usa el objeto 1."));

            var watcher = Place(Interactables("StoryTrigger_Zone"), Slot(2), z, 0.03f, "3. Trigger por flag\nReacciona al cambiar Despierta");
            var trigger = watcher.GetComponent<StoryTrigger>();
            EditorAssetUtility.SetBool(trigger, "triggerOnZoneEnter", false);
            EditorAssetUtility.SetBool(trigger, "triggerOnFlagChanged", true);
            SetFlags(trigger, "requiredFlags", "Despierta");
            EditorAssetUtility.SetObject(trigger, "dialogueOnTrigger", Dialogue("Test_8_FlagTrigger", false, "El trigger detecto la flag Despierta."));

            var pills = Place(Interactables("Examinable"), Slot(3), z, 0.25f, "4. Eleccion A\nPastillas: FinalBueno");
            Configure(pills, "Tomar pastillas", Dialogue("Test_JarPills", false, "Hay pastillas dentro. Me las tomo."));
            SetFlags(pills.GetComponent<InteractableBase>(), "flagsToSetOnInteract", "FinalBueno");

            var hammer = Place(Interactables("Examinable"), Slot(4), z, 0.25f, "4. Eleccion B\nMartillo: FinalMalo");
            Configure(hammer, "Tomar martillo", Dialogue("Test_HammerChoice", false, "Elijo el martillo."));
            SetFlags(hammer.GetComponent<InteractableBase>(), "flagsToSetOnInteract", "FinalMalo");

            var choice = new GameObject("ExclusiveChoice");
            choice.transform.SetParent(room, false);
            choice.transform.position = Point(Slot(3) + Column / 2f, z, 0f);
            var group = choice.AddComponent<ExclusiveChoiceGroup>();
            EditorAssetUtility.SetObjects(group, "options", pills.GetComponent<InteractableBase>(), hammer.GetComponent<InteractableBase>());

            var jar = Place(Interactables("Examinable"), Slot(5), z, 0.25f, "5. Frasco vacio\nPone Frasco1Revisado");
            Configure(jar, "Revisar frasco", Dialogue("Test_JarEmpty", false, "El frasco esta vacio."));
            SetFlags(jar.GetComponent<InteractableBase>(), "flagsToSetOnInteract", "Frasco1Revisado");
        }

        private static void BuildSystems()
        {
            var z = RowZ(4);

            var minigame = Object.FindFirstObjectByType<BreathingMinigame>();
            if (minigame == null)
            {
                minigame = InstantiateSystem<BreathingMinigame>("UI/BreathingCanvas");
            }

            var manager = Object.FindFirstObjectByType<HallucinationManager>();
            if (manager == null)
            {
                manager = InstantiateSystem<HallucinationManager>("Gameplay/Systems/HallucinationManager");
            }

            if (Object.FindFirstObjectByType<ScreenFader>() == null)
            {
                InstantiateSystem<ScreenFader>("Gameplay/Systems/ScreenFader");
            }

            HallucinationSetupMenu.EnsureLevelLabel();

            CreateStation(Slot(0), z, "Respirar", "BreathingCanvas\nManten ESPACIO", minigame.Begin);
            CreateStation(Slot(1), z, "Subir nivel", "HallucinationManager\nNivel +1", manager.Increase);
            CreateStation(Slot(2), z, "Bajar nivel", "HallucinationManager\nNivel -1", manager.Decrease);
            BuildNoiseStation(Slot(3), z);
            BuildTimedStation(Slot(4), z);
            BuildFaderStation(Slot(5), z);
        }

        private static void BuildNoiseStation(float x, float z)
        {
            var station = CreateStation(x, z, "Hacer ruido", "NoiseEmitter\nMira la consola", null);
            var emitter = station.AddComponent<NoiseEmitter>();
            EditorAssetUtility.SetObject(emitter, "channel", SequencesSetupMenu.EnsureNoiseChannel());
            UnityEventTools.AddVoidPersistentListener(station.GetComponent<InteractableBase>().InteractEvent, emitter.Emit);
        }

        private static void BuildTimedStation(float x, float z)
        {
            var station = CreateStation(x, z, "Iniciar secuencia", "TimedEventSequence\n3 lamparas con retraso", null);

            var lamps = new GameObject[3];
            for (var i = 0; i < lamps.Length; i++)
            {
                var lamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                lamp.name = $"Lamp_{i + 1}";
                Object.DestroyImmediate(lamp.GetComponent<Collider>());
                lamp.transform.SetParent(room, true);
                lamp.transform.position = Point(x - 1f + i, z, 1.8f);
                lamp.transform.localScale = Vector3.one * 0.4f;
                lamp.SetActive(false);
                lamps[i] = lamp;
            }

            var sequenceGo = new GameObject("Sequence_Lamps");
            sequenceGo.transform.SetParent(room, false);
            var sequence = sequenceGo.AddComponent<TimedEventSequence>();
            SetSteps(sequence, new[] { 0.5f, 1f, 1f, 1.5f }, new[] { "Lampara 1", "Lampara 2", "Lampara 3", "Todo apagado" });

            for (var i = 0; i < lamps.Length; i++)
            {
                AddSetActive(sequence.Steps[i].OnReached, lamps[i], true);
                AddSetActive(sequence.Steps[3].OnReached, lamps[i], false);
            }

            UnityEventTools.AddVoidPersistentListener(station.GetComponent<InteractableBase>().InteractEvent, sequence.Play);
        }

        private static void BuildFaderStation(float x, float z)
        {
            var fader = Object.FindFirstObjectByType<ScreenFader>();
            var station = CreateStation(x, z, "Fundido a negro", "ScreenFader\nFadeOut 1 s", null);

            if (fader == null)
            {
                return;
            }

            var interactable = station.GetComponent<InteractableBase>();
            UnityAction<float> fadeOut = fader.FadeOut;
            UnityEventTools.AddFloatPersistentListener(interactable.InteractEvent, fadeOut, 1f);

            var sequenceGo = new GameObject("Sequence_FadeBack");
            sequenceGo.transform.SetParent(room, false);
            var sequence = sequenceGo.AddComponent<TimedEventSequence>();
            SetSteps(sequence, new[] { 1.5f }, new[] { "Volver" });
            UnityAction<float> fadeIn = fader.FadeIn;
            UnityEventTools.AddFloatPersistentListener(sequence.Steps[0].OnReached, fadeIn, 1f);
            UnityEventTools.AddVoidPersistentListener(interactable.InteractEvent, sequence.Play);
        }

        private static GameObject CreateStation(float x, float z, string prompt, string label, UnityAction action)
        {
            var station = Place(Interactables("Interactable_Basic"), x, z, 0.4f, label);
            station.name = "Station_" + prompt;
            var interactable = station.GetComponent<InteractableBase>();
            EditorAssetUtility.SetString(interactable, "promptText", prompt);

            if (action != null)
            {
                UnityEventTools.AddVoidPersistentListener(interactable.InteractEvent, action);
            }

            EditorUtility.SetDirty(interactable);
            return station;
        }

        private static void Configure(GameObject target, string prompt, DialogueSequence dialogue)
        {
            var interactable = target.GetComponent<InteractableBase>();
            EditorAssetUtility.SetString(interactable, "promptText", prompt);
            EditorAssetUtility.SetObject(interactable, "interactDialogue", dialogue);
        }

        private static void SetFlags(Object target, string field, string flagName)
        {
            var flag = EditorAssetUtility.FindAsset<StoryFlag>(flagName);
            if (flag == null)
            {
                Debug.LogWarning($"Flag {flagName} not found; {field} left empty.");
                return;
            }

            EditorAssetUtility.SetObjects(target, field, flag);
        }

        private static DialogueSequence Dialogue(string assetName, bool lockPlayer, params string[] lines)
        {
            EditorAssetUtility.EnsureFolder("Assets/GameJamG1/ScriptableObjects/Dialogues", "Test");
            return EditorAssetUtility.CreateDialogue(DialogueFolder, assetName, lockPlayer, lines);
        }

        private static void SetSteps(TimedEventSequence sequence, float[] delays, string[] labels)
        {
            var serialized = new SerializedObject(sequence);
            var steps = serialized.FindProperty("steps");
            steps.arraySize = delays.Length;

            for (var i = 0; i < delays.Length; i++)
            {
                steps.GetArrayElementAtIndex(i).FindPropertyRelative("delay").floatValue = delays[i];
                steps.GetArrayElementAtIndex(i).FindPropertyRelative("label").stringValue = labels[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddSetActive(UnityEvent target, GameObject go, bool value)
        {
            UnityAction<bool> action = go.SetActive;
            UnityEventTools.AddBoolPersistentListener(target, action, value);
        }

        private static void AddZoneMarker(Transform zone)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = "ZoneMarker";
            Object.DestroyImmediate(marker.GetComponent<Collider>());
            marker.transform.SetParent(zone, false);
            marker.transform.localPosition = Vector3.zero;
            marker.transform.localScale = Vector3.one;
        }

        private static T InstantiateSystem<T>(string relativePath) where T : Component
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabRoot}/{relativePath}.prefab");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(instance, "Create " + prefab.name);
            return instance.GetComponentInChildren<T>(true);
        }

        private static GameObject Place(string path, float x, float z, float height, string label)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, room);
            instance.transform.SetPositionAndRotation(Point(x, z, height), facing);

            if (label != null)
            {
                CreateLabel(label, Point(x, z, 2.6f));
            }

            return instance;
        }

        private static Vector3 Point(float x, float z, float height)
        {
            return origin + right * x + forward * z + Vector3.up * height;
        }

        private static float Slot(int index)
        {
            return -7.5f + index * Column;
        }

        private static float RowZ(int row)
        {
            return row * RowSpacing;
        }

        private static string PrefabPath(string relative)
        {
            return $"{PrefabRoot}/{relative}.prefab";
        }

        private static string Interactables(string prefabName)
        {
            return PrefabPath($"Gameplay/Interactables/{prefabName}");
        }

        private static string Asset(string prefabName)
        {
            return AssetDatabase.GetAssetPath(EditorAssetUtility.FindAsset<GameObject>(prefabName));
        }

        private static void CreateSectionLabel(int row, string text)
        {
            var go = new GameObject("Section_" + text);
            go.transform.SetParent(room, false);
            go.transform.SetPositionAndRotation(Point(-10.5f, RowZ(row), 1.5f), facing);

            var label = go.AddComponent<TextMeshPro>();
            label.text = text;
            label.fontSize = 3f;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.rectTransform.sizeDelta = new Vector2(3f, 2f);
        }

        private static void CreateLabel(string text, Vector3 position)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(room, false);
            go.transform.SetPositionAndRotation(position, facing);

            var label = go.AddComponent<TextMeshPro>();
            label.text = text;
            label.fontSize = 2.2f;
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.rectTransform.sizeDelta = new Vector2(2.8f, 1.2f);
        }

        private static void CreateFloor()
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(room, false);
            floor.transform.SetPositionAndRotation(origin + forward * 10.5f + Vector3.down * 0.05f, facing);
            floor.transform.localScale = new Vector3(26f, 0.1f, 32f);
        }

        private static void EnsureMatchCounter()
        {
            if (Object.FindFirstObjectByType<MatchCounterUI>() != null)
            {
                return;
            }

            var canvas = GameObject.Find("InteractionCanvas");
            if (canvas == null)
            {
                Debug.LogWarning("InteractionCanvas not found; the match counter was not created.");
                return;
            }

            var go = new GameObject("MatchCounter", typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-30f, -30f);
            rect.sizeDelta = new Vector2(300f, 50f);

            var text = go.AddComponent<TextMeshProUGUI>();
            text.fontSize = 32f;
            text.alignment = TextAlignmentOptions.Right;
            text.text = "Cerillas: 0";

            var counter = go.AddComponent<MatchCounterUI>();
            EditorAssetUtility.SetObject(counter, "label", text);
        }

        private static void ComputeBasis()
        {
            var player = GameObject.FindWithTag("Player");
            forward = Vector3.forward;
            origin = Vector3.zero;

            if (player != null)
            {
                forward = Vector3.ProjectOnPlane(player.transform.forward, Vector3.up);
                forward = forward.sqrMagnitude > 0.001f ? forward.normalized : Vector3.forward;
                origin = player.transform.position + forward * 4f;

                if (Physics.Raycast(origin + Vector3.up * 3f, Vector3.down, out var hit, 10f))
                {
                    origin.y = hit.point.y;
                }
                else
                {
                    origin.y = player.transform.position.y;
                }
            }

            right = Vector3.Cross(Vector3.up, forward);
            facing = Quaternion.LookRotation(forward);
        }
    }
}
