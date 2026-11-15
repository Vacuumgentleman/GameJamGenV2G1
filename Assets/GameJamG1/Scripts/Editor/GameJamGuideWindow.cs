using System.Collections.Generic;
using System.Linq;
using GameJamG1.Dialogue;
using GameJamG1.Interaction;
using GameJamG1.Story;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GameJamG1.EditorTools
{
    public sealed class GameJamGuideWindow : EditorWindow
    {
        private const string ScriptableRoot = "Assets/GameJamG1/ScriptableObjects";
        private const string FlagsFolder = ScriptableRoot + "/Flags";
        private const string DialoguesFolder = ScriptableRoot + "/Dialogues";
        private const int MaxDialogueRows = 60;

        private static readonly string[] TabNames = { "Inicio", "Prefabs", "Historia", "Sistemas", "Recetas" };

        private sealed class FlagUsage
        {
            public readonly List<Component> Setters = new();
            public readonly List<Component> Requirers = new();
        }

        private int tab;
        private Vector2 scroll;
        private readonly Dictionary<string, bool> folds = new();
        private readonly Dictionary<string, GameObject> prefabCache = new();
        private readonly Dictionary<StoryFlag, FlagUsage> usage = new();
        private List<StoryFlag> flags = new();
        private List<DialogueSequence> dialogues = new();
        private bool usageDirty = true;

        private string flagFilter = string.Empty;
        private string dialogueFilter = string.Empty;
        private string newFlagName = string.Empty;
        private string newFlagDescription = string.Empty;
        private string newDialogueName = string.Empty;
        private string newDialogueText = string.Empty;
        private bool newDialogueLock;

        private GUIStyle wrapStyle;

        [MenuItem("GameJamG1/Guia")]
        private static void Open()
        {
            var window = GetWindow<GameJamGuideWindow>("GameJamG1 Guia");
            window.minSize = new Vector2(460f, 400f);
            window.RefreshAssets();
        }

        private void OnEnable()
        {
            RefreshAssets();
        }

        private void OnProjectChange()
        {
            RefreshAssets();
            Repaint();
        }

        private void OnHierarchyChange()
        {
            usageDirty = true;
            Repaint();
        }

        private void OnInspectorUpdate()
        {
            if (tab == 2 && Application.isPlaying)
            {
                Repaint();
            }
        }

        private void OnGUI()
        {
            wrapStyle ??= new GUIStyle(EditorStyles.label) { wordWrap = true, richText = true };

            tab = GUILayout.Toolbar(tab, TabNames, GUILayout.Height(26f));
            EditorGUILayout.Space(4f);

            scroll = EditorGUILayout.BeginScrollView(scroll);

            switch (tab)
            {
                case 0:
                    DrawOverview();
                    break;
                case 1:
                    DrawPrefabs();
                    break;
                case 2:
                    DrawStory();
                    break;
                case 3:
                    DrawTopics(GuideContent.Systems);
                    break;
                default:
                    DrawTopics(GuideContent.Recipes);
                    break;
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawOverview()
        {
            Wrap(GuideContent.Overview);
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Herramientas", EditorStyles.boldLabel);

            if (GUILayout.Button("Crear libreria de prefabs (los que falten)"))
            {
                PrefabLibraryBuilder.EnsureLibrary();
                RefreshAssets();
            }

            if (GUILayout.Button("Crear sala de pruebas con todos los ejemplos"))
            {
                EditorApplication.ExecuteMenuItem("GameJamG1/Create Prefab Test Room");
            }

            if (GUILayout.Button("Ir a la carpeta de prefabs"))
            {
                var folder = AssetDatabase.LoadAssetAtPath<Object>("Assets/GameJamG1/Prefabs/Gameplay");
                Selection.activeObject = folder;
                EditorGUIUtility.PingObject(folder);
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.HelpBox("Los sistemas globales (ScreenFader, HallucinationManager, BreathingCanvas) van una sola vez por escena. DialogueManager y StoryManager ya estan en la escena principal.", MessageType.Info);
        }

        private void DrawPrefabs()
        {
            Wrap("Elige un prefab para ver que hace y como configurarlo. 'Agregar a la escena' lo coloca en el centro de la vista Scene.");

            foreach (var info in GuideContent.Prefabs)
            {
                prefabCache.TryGetValue(info.Name, out var prefab);

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(info.Name, EditorStyles.boldLabel);
                    Wrap(info.Summary);
                    Wrap("<b>Configurar:</b> " + info.Setup);

                    if (prefab == null)
                    {
                        EditorGUILayout.HelpBox("Este prefab no existe todavia. Pulsa 'Crear libreria de prefabs' en Inicio.", MessageType.Warning);
                        continue;
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Seleccionar"))
                        {
                            Selection.activeObject = prefab;
                            EditorGUIUtility.PingObject(prefab);
                        }

                        if (GUILayout.Button("Agregar a la escena"))
                        {
                            AddToScene(prefab);
                        }
                    }
                }
            }
        }

        private void DrawStory()
        {
            DrawFold("Como funciona la historia", () => Wrap(GuideContent.StoryConcept), true);
            DrawFold("Crear flag", DrawCreateFlag, false);
            DrawFold("Crear dialogo", DrawCreateDialogue, false);
            DrawFold("Flags: estado y uso en la escena", DrawFlags, true);
            DrawFold("Dialogos: probar", DrawDialogues, false);
        }

        private void DrawCreateFlag()
        {
            newFlagName = EditorGUILayout.TextField("Nombre", newFlagName);
            EditorGUILayout.LabelField("Descripcion");
            newFlagDescription = EditorGUILayout.TextArea(newFlagDescription, GUILayout.MinHeight(40f));

            using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(newFlagName)))
            {
                if (GUILayout.Button("Crear flag"))
                {
                    CreateFlag();
                }
            }
        }

        private void DrawCreateDialogue()
        {
            newDialogueName = EditorGUILayout.TextField("Nombre", newDialogueName);
            EditorGUILayout.LabelField("Lineas (una por renglon)");
            newDialogueText = EditorGUILayout.TextArea(newDialogueText, GUILayout.MinHeight(60f));
            newDialogueLock = EditorGUILayout.Toggle("Bloquear al jugador", newDialogueLock);

            using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(newDialogueName) || string.IsNullOrWhiteSpace(newDialogueText)))
            {
                if (GUILayout.Button("Crear dialogo"))
                {
                    CreateDialogue();
                }
            }
        }

        private void DrawFlags()
        {
            var manager = StoryManager.Instance;
            var playing = Application.isPlaying && manager != null;

            if (!playing)
            {
                EditorGUILayout.HelpBox("Entra a Play para activar y desactivar flags. Abajo ves quien pone y quien exige cada una en la escena abierta.", MessageType.Info);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                flagFilter = EditorGUILayout.TextField("Buscar", flagFilter);

                if (GUILayout.Button("Actualizar", GUILayout.Width(80f)))
                {
                    RefreshAssets();
                    usageDirty = true;
                }
            }

            if (usageDirty)
            {
                ScanScene();
            }

            foreach (var flag in flags)
            {
                if (!string.IsNullOrEmpty(flagFilter) && flag.name.IndexOf(flagFilter, System.StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (new EditorGUI.DisabledScope(!playing))
                        {
                            var active = playing && manager.Has(flag);
                            var next = EditorGUILayout.ToggleLeft(flag.name, active, EditorStyles.boldLabel);

                            if (next != active)
                            {
                                if (next)
                                {
                                    manager.Set(flag);
                                }
                                else
                                {
                                    manager.Clear(flag);
                                }
                            }
                        }

                        if (GUILayout.Button("Asset", GUILayout.Width(52f)))
                        {
                            Selection.activeObject = flag;
                            EditorGUIUtility.PingObject(flag);
                        }
                    }

                    if (!string.IsNullOrEmpty(flag.Description))
                    {
                        Wrap(flag.Description);
                    }

                    usage.TryGetValue(flag, out var flagUsage);
                    DrawUsers("La pone", flagUsage?.Setters);
                    DrawUsers("La exige", flagUsage?.Requirers);
                }
            }
        }

        private void DrawUsers(string title, List<Component> users)
        {
            if (users == null || users.Count == 0)
            {
                EditorGUILayout.LabelField(title + ": nadie en esta escena", EditorStyles.miniLabel);
                return;
            }

            EditorGUILayout.LabelField(title + ":", EditorStyles.miniBoldLabel);

            foreach (var user in users)
            {
                if (user == null)
                {
                    continue;
                }

                if (GUILayout.Button("  " + user.gameObject.name, EditorStyles.linkLabel))
                {
                    Selection.activeObject = user.gameObject;
                    EditorGUIUtility.PingObject(user.gameObject);
                }
            }
        }

        private void DrawDialogues()
        {
            var manager = DialogueManager.Instance;
            var playing = Application.isPlaying && manager != null;

            dialogueFilter = EditorGUILayout.TextField("Buscar", dialogueFilter);

            var shown = 0;
            foreach (var dialogue in dialogues)
            {
                if (!string.IsNullOrEmpty(dialogueFilter) && dialogue.name.IndexOf(dialogueFilter, System.StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                if (++shown > MaxDialogueRows)
                {
                    EditorGUILayout.LabelField("Hay mas resultados: usa el buscador.", EditorStyles.miniLabel);
                    break;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"{dialogue.name}  ({dialogue.Lines.Count} lineas)");

                    using (new EditorGUI.DisabledScope(!playing))
                    {
                        if (GUILayout.Button("Reproducir", GUILayout.Width(80f)))
                        {
                            manager.Play(dialogue);
                        }
                    }

                    if (GUILayout.Button("Asset", GUILayout.Width(52f)))
                    {
                        Selection.activeObject = dialogue;
                        EditorGUIUtility.PingObject(dialogue);
                    }
                }
            }
        }

        private void DrawTopics(GuideTopic[] topics)
        {
            foreach (var topic in topics)
            {
                var body = topic.Body;
                DrawFold(topic.Title, () => Wrap(body), false);
            }
        }

        private void DrawFold(string title, System.Action content, bool defaultOpen)
        {
            if (!folds.TryGetValue(title, out var open))
            {
                open = defaultOpen;
            }

            open = EditorGUILayout.Foldout(open, title, true);
            folds[title] = open;

            if (!open)
            {
                return;
            }

            EditorGUI.indentLevel++;
            content();
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(4f);
        }

        private void Wrap(string text)
        {
            EditorGUILayout.LabelField(text, wrapStyle);
        }

        private void RefreshAssets()
        {
            prefabCache.Clear();
            foreach (var info in GuideContent.Prefabs)
            {
                prefabCache[info.Name] = EditorAssetUtility.FindAsset<GameObject>(info.Name);
            }

            flags = AssetDatabase.FindAssets("t:StoryFlag")
                .Select(guid => AssetDatabase.LoadAssetAtPath<StoryFlag>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(flag => flag != null)
                .OrderBy(flag => flag.name)
                .ToList();

            dialogues = AssetDatabase.FindAssets("t:DialogueSequence")
                .Select(guid => AssetDatabase.LoadAssetAtPath<DialogueSequence>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(dialogue => dialogue != null)
                .OrderBy(dialogue => dialogue.name)
                .ToList();
        }

        private void ScanScene()
        {
            usageDirty = false;
            usage.Clear();

            foreach (var interactable in FindObjectsByType<InteractableBase>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Collect(interactable, "flagsToSetOnInteract", true);
                Collect(interactable, "completionFlags", true);
                Collect(interactable, "requiredFlags", false);
            }

            foreach (var trigger in FindObjectsByType<StoryTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Collect(trigger, "requiredFlags", false);
            }
        }

        private void Collect(Component component, string property, bool asSetter)
        {
            var list = new SerializedObject(component).FindProperty(property);
            if (list == null || !list.isArray)
            {
                return;
            }

            for (var i = 0; i < list.arraySize; i++)
            {
                if (list.GetArrayElementAtIndex(i).objectReferenceValue is not StoryFlag flag)
                {
                    continue;
                }

                if (!usage.TryGetValue(flag, out var flagUsage))
                {
                    flagUsage = new FlagUsage();
                    usage[flag] = flagUsage;
                }

                (asSetter ? flagUsage.Setters : flagUsage.Requirers).Add(component);
            }
        }

        private static void AddToScene(GameObject prefab)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            var view = SceneView.lastActiveSceneView;
            instance.transform.position = view != null ? view.pivot : Vector3.zero;
            Undo.RegisterCreatedObjectUndo(instance, "Add " + prefab.name);
            Selection.activeGameObject = instance;
            EditorSceneManager.MarkSceneDirty(instance.scene);
        }

        private void CreateFlag()
        {
            var assetName = Sanitize(newFlagName);
            if (string.IsNullOrEmpty(assetName))
            {
                return;
            }

            var path = $"{FlagsFolder}/{assetName}.asset";
            if (AssetDatabase.LoadAssetAtPath<StoryFlag>(path) != null)
            {
                EditorUtility.DisplayDialog("Crear flag", $"Ya existe una flag llamada {assetName}.", "OK");
                return;
            }

            EditorAssetUtility.EnsureFolder(ScriptableRoot, "Flags");
            var flag = CreateInstance<StoryFlag>();
            AssetDatabase.CreateAsset(flag, path);
            EditorAssetUtility.SetString(flag, "displayName", newFlagName.Trim());
            EditorAssetUtility.SetString(flag, "description", newFlagDescription);
            EditorUtility.SetDirty(flag);
            AssetDatabase.SaveAssets();

            Selection.activeObject = flag;
            EditorGUIUtility.PingObject(flag);
            newFlagName = string.Empty;
            newFlagDescription = string.Empty;
            RefreshAssets();
        }

        private void CreateDialogue()
        {
            var assetName = Sanitize(newDialogueName);
            if (string.IsNullOrEmpty(assetName))
            {
                return;
            }

            var path = $"{DialoguesFolder}/{assetName}.asset";
            if (AssetDatabase.LoadAssetAtPath<DialogueSequence>(path) != null &&
                !EditorUtility.DisplayDialog("Crear dialogo", $"Ya existe {assetName}. Reemplazar sus lineas?", "Reemplazar", "Cancelar"))
            {
                return;
            }

            var lines = newDialogueText
                .Split('\n')
                .Select(line => line.Trim())
                .Where(line => line.Length > 0)
                .ToArray();

            EditorAssetUtility.EnsureFolder(ScriptableRoot, "Dialogues");
            var dialogue = EditorAssetUtility.CreateDialogue(DialoguesFolder, assetName, newDialogueLock, lines);
            AssetDatabase.SaveAssets();

            Selection.activeObject = dialogue;
            EditorGUIUtility.PingObject(dialogue);
            newDialogueName = string.Empty;
            newDialogueText = string.Empty;
            RefreshAssets();
        }

        private static string Sanitize(string value)
        {
            var chars = value.Trim().Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray();
            return new string(chars);
        }
    }
}
