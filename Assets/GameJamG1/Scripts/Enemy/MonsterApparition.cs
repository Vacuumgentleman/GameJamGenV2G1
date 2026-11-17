using System.Collections;
using UnityEngine;

namespace GameJamG1.Enemy
{
    // Mientras el monstruo esta dormido (antes de la persecucion), si el jugador llega a verlo
    // parpadea y se desvanece: no se queda quieto en la esquina. Vuelve a ser visible cuando se activa.
    [RequireComponent(typeof(EnemyController))]
    public class MonsterApparition : MonoBehaviour
    {
        [SerializeField, Min(0.5f)] private float noticeDistance = 14f;
        [Tooltip("Fraction of the screen around the centre where the monster counts as seen (0.5 = whole screen).")]
        [SerializeField, Range(0.1f, 0.5f)] private float viewMargin = 0.4f;
        [Tooltip("Seconds the player must look at it before it reacts.")]
        [SerializeField, Min(0f)] private float lookTime = 0.35f;
        [SerializeField, Min(0.1f)] private float flickerDuration = 0.8f;
        [SerializeField] private LayerMask obstacleMask = ~0;
        [SerializeField, Min(0f)] private float targetHeight = 1.2f;

        private EnemyController enemy;
        private Renderer[] renderers;
        private Collider[] colliders;
        private Camera viewer;
        private float seenFor;
        private bool hidden;
        private Coroutine flicker;

        private void Awake()
        {
            enemy = GetComponent<EnemyController>();
            renderers = GetComponentsInChildren<Renderer>(true);
            colliders = GetComponentsInChildren<Collider>(true);
        }

        private void OnEnable()
        {
            enemy.StateChanged += HandleState;
        }

        private void OnDisable()
        {
            enemy.StateChanged -= HandleState;
        }

        private void HandleState(EnemyState state)
        {
            if (flicker != null)
            {
                StopCoroutine(flicker);
                flicker = null;
            }

            // Activo: persigue y se ve. Dormido de nuevo (tras morir el jugador): vuelve a su esquina, visible.
            hidden = false;
            seenFor = 0f;
            SetVisible(true);
            SetSolid(true);
        }

        private void Update()
        {
            if (enemy.State != EnemyState.Inactive || hidden || flicker != null)
            {
                return;
            }

            if (viewer == null)
            {
                viewer = Camera.main;
                if (viewer == null)
                {
                    return;
                }
            }

            seenFor = IsSeen() ? seenFor + Time.deltaTime : 0f;

            if (seenFor >= lookTime)
            {
                flicker = StartCoroutine(FlickerAway());
            }
        }

        private bool IsSeen()
        {
            if (!viewer.isActiveAndEnabled)
            {
                return false;
            }

            var target = transform.position + Vector3.up * targetHeight;
            var toTarget = target - viewer.transform.position;
            if (toTarget.sqrMagnitude > noticeDistance * noticeDistance)
            {
                return false;
            }

            var vp = viewer.WorldToViewportPoint(target);
            if (vp.z <= 0f || Mathf.Abs(vp.x - 0.5f) > viewMargin || Mathf.Abs(vp.y - 0.5f) > viewMargin)
            {
                return false;
            }

            if (Physics.Linecast(viewer.transform.position, target, out var hit, obstacleMask, QueryTriggerInteraction.Ignore))
            {
                return hit.transform.IsChildOf(transform);
            }

            return true;
        }

        private IEnumerator FlickerAway()
        {
            var end = Time.time + flickerDuration;
            var visible = true;

            while (Time.time < end)
            {
                visible = !visible;
                SetVisible(visible);
                // Cada vez mas tiempo apagado: se va desvaneciendo.
                var progress = 1f - (end - Time.time) / flickerDuration;
                yield return new WaitForSeconds(visible ? Random.Range(0.03f, 0.08f) * (1f - progress * 0.6f) : Random.Range(0.04f, 0.12f) * (1f + progress * 2f));
            }

            SetVisible(false);
            SetSolid(false);
            hidden = true;
            flicker = null;
        }

        private void SetVisible(bool visible)
        {
            foreach (var r in renderers)
            {
                if (r != null)
                {
                    r.enabled = visible;
                }
            }
        }

        // Desaparecido del todo: tampoco se puede chocar con el ni tapa la vista.
        private void SetSolid(bool solid)
        {
            foreach (var c in colliders)
            {
                if (c != null)
                {
                    c.enabled = solid;
                }
            }
        }
    }
}
