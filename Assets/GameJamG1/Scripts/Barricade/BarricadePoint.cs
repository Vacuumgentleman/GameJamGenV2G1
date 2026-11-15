using System;
using System.Collections.Generic;
using GameJamG1.Dialogue;
using GameJamG1.Interaction;
using GameJamG1.Items;
using GameJamG1.Objects;
using GameJamG1.Story;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace GameJamG1.Barricade
{
    public class BarricadePoint : InteractableBase
    {
        [Header("Planks")]
        [Tooltip("Carry item accepted by this barricade.")]
        [SerializeField] private ItemData plankItem;
        [SerializeField, Min(1)] private int requiredPlanks = 3;
        [Tooltip("Plank meshes enabled one by one, in order.")]
        [SerializeField] private List<GameObject> planks = new();

        [Header("Blocked Object")]
        [Tooltip("Door blocked from the first plank placed.")]
        [SerializeField] private Door door;
        [SerializeField] private List<StoryFlag> completionFlags = new();

        [Header("Placement Hint")]
        [Tooltip("Translucent material for the silhouettes of the planks still to be placed. Empty disables them.")]
        [SerializeField] private Material ghostMaterial;
        [Tooltip("Tint of the preview. The next plank pulses, brighter while carrying a plank.")]
        [SerializeField] private Color ghostColor = new(0.4f, 0.9f, 1f, 0.4f);
        [SerializeField, Min(0.1f)] private float pulseSpeed = 3f;

        [Header("Feedback")]
        [SerializeField] private AudioClip nailSound;
        [SerializeField] private DialogueSequence needPlankDialogue;

        [Header("Durability")]
        [SerializeField, Min(1f)] private float maxHealth = 100f;

        [Header("Events")]
        [SerializeField] private UnityEvent onCompleted;
        [SerializeField] private UnityEvent onBreached;

        private readonly List<GameObject> ghosts = new();
        private readonly List<Renderer[]> ghostRenderers = new();
        private MaterialPropertyBlock block;
        private ItemHolder holder;

        public int CurrentPlanks { get; private set; }

        public float Health { get; private set; }

        public bool IsComplete => CurrentPlanks >= requiredPlanks;

        public override string GetPrompt()
        {
            return $"{base.GetPrompt()} ({CurrentPlanks}/{requiredPlanks})";
        }

        public override bool CanInteract()
        {
            return base.CanInteract() && !IsComplete && (door == null || !door.IsOpen);
        }

        public void TakeDamage(float amount)
        {
            if (!IsComplete || amount <= 0f)
            {
                return;
            }

            Health = Mathf.Max(0f, Health - amount);

            if (Health <= 0f)
            {
                Breach();
            }
        }

        protected override void Awake()
        {
            base.Awake();
            CreateGhosts();
            RefreshVisuals();
        }

        private void Update()
        {
            if (IsComplete || ghosts.Count == 0)
            {
                return;
            }

            var carrying = IsCarryingPlank();
            var pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            var nextAlpha = Mathf.Lerp(ghostColor.a * 0.6f, carrying ? 1f : ghostColor.a, pulse);

            for (var i = 0; i < ghosts.Count; i++)
            {
                if (ghosts[i] != null && ghosts[i].activeSelf)
                {
                    Paint(i, i == CurrentPlanks ? nextAlpha : ghostColor.a * 0.5f);
                }
            }
        }

        protected override bool OnInteract(PlayerInteractor interactor)
        {
            if (!interactor.TryGetComponent(out ItemHolder holder) || holder.CurrentCarry != plankItem)
            {
                PlayDialogue(needPlankDialogue);
                return false;
            }

            holder.Clear(ItemSlot.Carry);
            CurrentPlanks++;
            RefreshVisuals();
            UpdateDoorBlock();

            if (nailSound != null)
            {
                AudioSource.PlayClipAtPoint(nailSound, transform.position);
            }

            if (IsComplete)
            {
                Complete();
            }

            return true;
        }

        private void Complete()
        {
            Health = maxHealth;

            if (StoryManager.Instance != null)
            {
                StoryManager.Instance.SetMany(completionFlags);
            }

            onCompleted.Invoke();
        }

        private void Breach()
        {
            CurrentPlanks = 0;
            RefreshVisuals();
            UpdateDoorBlock();

            onBreached.Invoke();
        }

        private void CreateGhosts()
        {
            if (ghostMaterial == null)
            {
                return;
            }

            block = new MaterialPropertyBlock();

            foreach (var plank in planks)
            {
                var ghost = plank != null ? CreateGhost(plank) : null;
                ghosts.Add(ghost);
                ghostRenderers.Add(ghost != null ? ghost.GetComponentsInChildren<Renderer>(true) : Array.Empty<Renderer>());
            }
        }

        private GameObject CreateGhost(GameObject plank)
        {
            var ghost = Instantiate(plank, plank.transform.parent);
            ghost.name = $"{plank.name}_Ghost";

            foreach (var ghostCollider in ghost.GetComponentsInChildren<Collider>(true))
            {
                Destroy(ghostCollider);
            }

            foreach (var ghostRenderer in ghost.GetComponentsInChildren<Renderer>(true))
            {
                var materials = new Material[ghostRenderer.sharedMaterials.Length];
                Array.Fill(materials, ghostMaterial);
                ghostRenderer.sharedMaterials = materials;
                ghostRenderer.shadowCastingMode = ShadowCastingMode.Off;
            }

            return ghost;
        }

        private void Paint(int index, float alpha)
        {
            var color = new Color(ghostColor.r, ghostColor.g, ghostColor.b, alpha);
            block.SetColor("_BaseColor", color);
            block.SetColor("_Color", color);

            foreach (var ghostRenderer in ghostRenderers[index])
            {
                ghostRenderer.SetPropertyBlock(block);
            }
        }

        private bool IsCarryingPlank()
        {
            if (holder == null)
            {
                holder = FindFirstObjectByType<ItemHolder>();
            }

            return holder != null && holder.CurrentCarry == plankItem;
        }

        private void UpdateDoorBlock()
        {
            if (door != null)
            {
                door.SetBarricaded(CurrentPlanks > 0);
            }
        }

        private void RefreshVisuals()
        {
            for (var i = 0; i < planks.Count; i++)
            {
                if (planks[i] != null)
                {
                    planks[i].SetActive(i < CurrentPlanks);
                }

                if (i < ghosts.Count && ghosts[i] != null)
                {
                    ghosts[i].SetActive(i >= CurrentPlanks);
                }
            }
        }

        private static void PlayDialogue(DialogueSequence sequence)
        {
            if (sequence != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.Play(sequence, false);
            }
        }

#if UNITY_EDITOR
        private void Reset()
        {
            foreach (var guid in UnityEditor.AssetDatabase.FindAssets("Barricade_Ghost t:Material"))
            {
                ghostMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
                break;
            }
        }
#endif

        protected override void OnValidate()
        {
            base.OnValidate();

            if (plankItem == null)
            {
                Debug.LogWarning($"{name}: BarricadePoint has no plank item assigned.", this);
            }

            if (ghostMaterial == null)
            {
                Debug.LogWarning($"{name}: no ghost material assigned, the plank preview is disabled.", this);
            }

            if (planks.Count != requiredPlanks)
            {
                Debug.LogWarning($"{name}: plank meshes ({planks.Count}) should match required planks ({requiredPlanks}).", this);
            }
        }
    }
}
