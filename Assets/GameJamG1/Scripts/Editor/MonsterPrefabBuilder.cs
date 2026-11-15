using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameJamG1.Enemy;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AI;

namespace GameJamG1.EditorTools
{
    public static class MonsterPrefabBuilder
    {
        private const string FbxPath = "Assets/GameJamG1/Prefabs/Enemies/MonsterPSX.fbx";
        private const string PrefabFolder = "Assets/GameJamG1/Prefabs/Enemies";
        private const string ControllerFolder = "Assets/GameJamG1/Animation/Monster";

        private const string IdleClip = "Idle_Watchful";
        private const string WalkClip = "Walk_Nervous";
        private const string RunClip = "Run_Frantic";
        private const string AttackClip = "Attack_Lunge";
        private const string ScreamClip = "Scream.lol";

        private const float WalkThreshold = 1.5f;
        private const float RunThreshold = 4f;

        private static readonly string[] LoopingClips = { IdleClip, WalkClip, RunClip };

        [MenuItem("GameJamG1/Enemies/Build Monster Prefabs")]
        public static void Build()
        {
            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);

            if (fbx == null)
            {
                Debug.LogError($"MonsterPrefabBuilder: model not found at {FbxPath}");
                return;
            }

            ConfigureLoops();

            var clips = LoadClips();
            var missing = new[] { IdleClip, WalkClip, RunClip, AttackClip, ScreamClip }
                .Where(name => !clips.ContainsKey(name))
                .ToList();

            if (missing.Count > 0)
            {
                Debug.LogError($"MonsterPrefabBuilder: missing clips in FBX: {string.Join(", ", missing)}");
                return;
            }

            EnsureFolder(ControllerFolder);

            var staticController = BuildStaticController(clips);
            var chaserController = BuildChaserController(clips);

            BuildStaticPrefab(fbx, staticController);
            BuildChaserPrefab(fbx, chaserController);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("MonsterPrefabBuilder: Monster_Static and Monster_Chaser built.");
        }

        private static void ConfigureLoops()
        {
            var importer = (ModelImporter)AssetImporter.GetAtPath(FbxPath);
            var clips = importer.clipAnimations.Length > 0 ? importer.clipAnimations : importer.defaultClipAnimations;
            var changed = false;

            foreach (var clip in clips)
            {
                var shouldLoop = LoopingClips.Any(name => MatchesClip(clip.name, name));

                if (clip.loopTime != shouldLoop)
                {
                    clip.loopTime = shouldLoop;
                    changed = true;
                }
            }

            if (!changed)
            {
                return;
            }

            importer.clipAnimations = clips;
            importer.SaveAndReimport();
        }

        private static Dictionary<string, AnimationClip> LoadClips()
        {
            var result = new Dictionary<string, AnimationClip>();
            var names = new[] { IdleClip, WalkClip, RunClip, AttackClip, ScreamClip };

            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(FbxPath))
            {
                if (asset is not AnimationClip clip || clip.name.StartsWith("__preview__"))
                {
                    continue;
                }

                foreach (var name in names.Where(name => MatchesClip(clip.name, name)))
                {
                    result[name] = clip;
                }
            }

            return result;
        }

        private static bool MatchesClip(string clipName, string shortName)
        {
            return clipName == shortName || clipName.EndsWith("|" + shortName);
        }

        private static AnimatorController BuildStaticController(IReadOnlyDictionary<string, AnimationClip> clips)
        {
            var controller = CreateController("MonsterStatic");
            controller.AddParameter(MonsterAnimatorParameters.Scream, AnimatorControllerParameterType.Trigger);

            var machine = controller.layers[0].stateMachine;
            var idle = machine.AddState("Idle");
            idle.motion = clips[IdleClip];
            machine.defaultState = idle;

            AddTriggerState(machine, "Scream", clips[ScreamClip], MonsterAnimatorParameters.Scream, idle);
            return controller;
        }

        private static AnimatorController BuildChaserController(IReadOnlyDictionary<string, AnimationClip> clips)
        {
            var controller = CreateController("MonsterChaser");
            controller.AddParameter(MonsterAnimatorParameters.Speed, AnimatorControllerParameterType.Float);
            controller.AddParameter(MonsterAnimatorParameters.Attack, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(MonsterAnimatorParameters.Scream, AnimatorControllerParameterType.Trigger);

            var machine = controller.layers[0].stateMachine;
            var locomotion = controller.CreateBlendTreeInController("Locomotion", out var tree);
            tree.blendType = BlendTreeType.Simple1D;
            tree.blendParameter = MonsterAnimatorParameters.Speed;
            tree.useAutomaticThresholds = false;
            tree.AddChild(clips[IdleClip], 0f);
            tree.AddChild(clips[WalkClip], WalkThreshold);
            tree.AddChild(clips[RunClip], RunThreshold);
            machine.defaultState = locomotion;

            AddTriggerState(machine, "Attack", clips[AttackClip], MonsterAnimatorParameters.Attack, locomotion);
            AddTriggerState(machine, "Scream", clips[ScreamClip], MonsterAnimatorParameters.Scream, locomotion);
            return controller;
        }

        private static AnimatorController CreateController(string name)
        {
            var path = $"{ControllerFolder}/{name}.controller";
            AssetDatabase.DeleteAsset(path);
            return AnimatorController.CreateAnimatorControllerAtPath(path);
        }

        private static void AddTriggerState(
            AnimatorStateMachine machine,
            string stateName,
            AnimationClip clip,
            string trigger,
            AnimatorState returnState)
        {
            var state = machine.AddState(stateName);
            state.motion = clip;

            var enter = machine.AddAnyStateTransition(state);
            enter.AddCondition(AnimatorConditionMode.If, 0f, trigger);
            enter.hasExitTime = false;
            enter.duration = 0.1f;
            enter.canTransitionToSelf = false;

            var exit = state.AddTransition(returnState);
            exit.hasExitTime = true;
            exit.exitTime = 0.95f;
            exit.duration = 0.15f;
        }

        private static void BuildStaticPrefab(GameObject fbx, RuntimeAnimatorController controller)
        {
            var root = new GameObject("Monster_Static");
            var animator = AttachModel(root, fbx, controller, out var bounds);
            AddCollider(root, bounds);

            var driver = root.AddComponent<MonsterAnimationDriver>();
            AssignReference(driver, "animator", animator);
            root.AddComponent<MonsterIdleScreamer>();

            SavePrefab(root, "Monster_Static");
        }

        private static void BuildChaserPrefab(GameObject fbx, RuntimeAnimatorController controller)
        {
            var root = new GameObject("Monster_Chaser");
            var animator = AttachModel(root, fbx, controller, out var bounds);
            var collider = AddCollider(root, bounds);

            var agent = root.AddComponent<NavMeshAgent>();
            agent.radius = collider.radius;
            agent.height = collider.height;
            agent.angularSpeed = 360f;
            agent.acceleration = 12f;

            var driver = root.AddComponent<MonsterAnimationDriver>();
            AssignReference(driver, "animator", animator);

            var chaser = root.AddComponent<MonsterChaser>();
            AssignReference(chaser, "animationDriver", driver);

            var serialized = new SerializedObject(chaser);
            serialized.FindProperty("startState").enumValueIndex = (int)EnemyState.Patrol;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, "Monster_Chaser");
        }

        private static Animator AttachModel(
            GameObject root,
            GameObject fbx,
            RuntimeAnimatorController controller,
            out Bounds bounds)
        {
            var model = (GameObject)PrefabUtility.InstantiatePrefab(fbx, root.transform);
            model.name = "Model";

            if (!model.TryGetComponent<Animator>(out var animator))
            {
                animator = model.AddComponent<Animator>();
            }

            if (animator.avatar == null)
            {
                animator.avatar = AssetDatabase.LoadAllAssetsAtPath(FbxPath).OfType<Avatar>().FirstOrDefault();
            }

            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;

            bounds = CalculateBounds(model);
            return animator;
        }

        private static Bounds CalculateBounds(GameObject model)
        {
            var renderers = model.GetComponentsInChildren<Renderer>();

            if (renderers.Length == 0)
            {
                return new Bounds(Vector3.up, new Vector3(1f, 2f, 1f));
            }

            var bounds = renderers[0].bounds;

            foreach (var renderer in renderers.Skip(1))
            {
                bounds.Encapsulate(renderer.bounds);
            }

            return bounds;
        }

        private static CapsuleCollider AddCollider(GameObject root, Bounds bounds)
        {
            var collider = root.AddComponent<CapsuleCollider>();
            collider.radius = Mathf.Clamp(Mathf.Max(bounds.size.x, bounds.size.z) * 0.5f, 0.2f, 0.6f);
            collider.height = Mathf.Max(bounds.size.y, collider.radius * 2f);
            collider.center = new Vector3(0f, collider.height * 0.5f, 0f);
            return collider;
        }

        private static void AssignReference(Object target, string propertyName, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(propertyName).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SavePrefab(GameObject root, string name)
        {
            PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabFolder}/{name}.prefab");
            Object.DestroyImmediate(root);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
