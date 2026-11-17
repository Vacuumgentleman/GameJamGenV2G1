using System.Collections;
using System.Collections.Generic;
using GameJamG1.Dialogue;
using GameJamG1.Enemy;
using GameJamG1.Player;
using GameJamG1.Sequences;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace GameJamG1.Gameplay
{
    public class GameFlowController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Optional. Found by tag when empty.")]
        [SerializeField] private Transform player;
        [SerializeField] private string playerTag = "Player";

        [Header("Death")]
        [SerializeField, Min(0f)] private float fadeDuration = 0.6f;
        [Tooltip("Seconds the screen stays black before the player respawns.")]
        [SerializeField, Min(0f)] private float deathHold = 1f;
        [Tooltip("Seconds enemies stay still after the player respawns.")]
        [SerializeField, Min(0f)] private float enemyRespawnDelay = 4f;

        [Header("Ending")]
        [SerializeField] private string menuSceneName = "Main Menu";
        [SerializeField, Min(0f)] private float endHold = 1.5f;
        [Tooltip("Seconds of the fast fade to black before the ending text.")]
        [SerializeField, Min(0f)] private float endingFadeDuration = 0.35f;
        [Tooltip("Typewriter keys played as each letter of the ending text appears.")]
        [SerializeField] private AudioClip[] endingTypeClips;
        [Tooltip("Played when a line of the ending text is complete (typewriter bell).")]
        [SerializeField] private AudioClip endingLineEndClip;

        [Header("Events")]
        [SerializeField] private UnityEvent onPlayerDied = new();
        [SerializeField] private UnityEvent onRespawned = new();

        private readonly List<MonsterChaser> enemies = new();
        private readonly List<Vector3> enemyPositions = new();
        private readonly List<Quaternion> enemyRotations = new();

        private readonly CheckpointSnapshot snapshot = new();
        private Vector3 checkpointPosition;
        private Quaternion checkpointRotation;
        private bool dying;
        private bool ending;
        private bool showingEnding;

        public static GameFlowController Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Instance = null;
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (player == null)
            {
                var found = GameObject.FindGameObjectWithTag(playerTag);
                player = found != null ? found.transform : null;
            }

            SaveCheckpoint();

            foreach (var enemy in FindObjectsByType<MonsterChaser>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                enemies.Add(enemy);
                enemyPositions.Add(enemy.transform.position);
                enemyRotations.Add(enemy.transform.rotation);
                enemy.AttackHit += KillPlayer;
            }
        }

        private void OnDestroy()
        {
            foreach (var enemy in enemies)
            {
                if (enemy != null)
                {
                    enemy.AttackHit -= KillPlayer;
                }
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SaveCheckpoint()
        {
            if (player == null)
            {
                return;
            }

            checkpointPosition = player.position;
            checkpointRotation = player.rotation;
            snapshot.Capture(player);
        }

        public void KillPlayer()
        {
            if (dying || ending || player == null)
            {
                return;
            }

            StartCoroutine(DeathRoutine());
        }

        public void BeginEnding()
        {
            ending = true;

            foreach (var enemy in enemies)
            {
                if (enemy != null && enemy.isActiveAndEnabled)
                {
                    enemy.SetActive(false);
                }
            }
        }

        public void EndGame()
        {
            if (showingEnding)
            {
                return;
            }

            BeginEnding();
            StartCoroutine(EndRoutine());
        }

        // Final: la pantalla se va a negro rapido y, sobre el negro, se cuenta lo que paso.
        public void PlayEnding(DialogueSequence text)
        {
            if (showingEnding)
            {
                return;
            }

            showingEnding = true;
            BeginEnding();
            StartCoroutine(EndingTextRoutine(text));
        }

        private IEnumerator EndingTextRoutine(DialogueSequence text)
        {
            if (player != null)
            {
                var playerLock = player.GetComponentInChildren<PlayerControlLock>();
                if (playerLock != null)
                {
                    playerLock.Lock(this);
                }
            }

            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.Stop();
            }

            yield return EndingScreen.Show(text, endingFadeDuration, endingTypeClips, endingLineEndClip);
            LoadMenu();
        }

        private IEnumerator DeathRoutine()
        {
            dying = true;

            var playerLock = player.GetComponentInChildren<PlayerControlLock>();
            if (playerLock != null)
            {
                playerLock.Lock(this);
            }

            onPlayerDied.Invoke();

            var retry = ChaseRetry.FindActive();

            yield return Fade(1f);

            var wasActive = new List<bool>();
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                var usable = enemy != null && enemy.isActiveAndEnabled;
                wasActive.Add(usable && enemy.State != EnemyState.Inactive);

                if (usable)
                {
                    enemy.SetActive(false);
                    enemy.transform.SetPositionAndRotation(enemyPositions[i], enemyRotations[i]);
                }
            }

            if (retry != null)
            {
                // La escena de la persecucion se repite: los enemigos que la empiezan no se reactivan solos.
                for (var i = 0; i < enemies.Count; i++)
                {
                    foreach (var sleeper in retry.EnemiesToSleep)
                    {
                        if (sleeper == enemies[i])
                        {
                            wasActive[i] = false;
                        }
                    }
                }

                retry.Rewind();
                var point = retry.RespawnPoint;
                TeleportPlayer(point != null ? point.position : checkpointPosition, point != null ? point.rotation : checkpointRotation);
            }
            else
            {
                TeleportPlayer(checkpointPosition, checkpointRotation);
            }

            snapshot.Restore(player);

            yield return new WaitForSeconds(deathHold);
            yield return Fade(0f);

            if (playerLock != null)
            {
                playerLock.Unlock(this);
            }

            dying = false;
            onRespawned.Invoke();

            if (retry != null)
            {
                retry.PlayRetryDialogue();
            }

            yield return new WaitForSeconds(enemyRespawnDelay);

            for (var i = 0; i < enemies.Count; i++)
            {
                if (!ending && wasActive[i] && enemies[i] != null)
                {
                    enemies[i].SetActive(true);
                }
            }
        }

        private IEnumerator EndRoutine()
        {
            yield return new WaitForSeconds(endHold);
            yield return Fade(1f);
            LoadMenu();
        }

        private void LoadMenu()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene(menuSceneName);
        }

        private void TeleportPlayer(Vector3 position, Quaternion rotation)
        {
            var controller = player.GetComponent<CharacterController>();

            if (controller != null)
            {
                controller.enabled = false;
            }

            // Solo el giro horizontal: la camara mira al frente al reaparecer.
            player.SetPositionAndRotation(position, Quaternion.Euler(0f, rotation.eulerAngles.y, 0f));

            if (controller != null)
            {
                controller.enabled = true;
            }
        }

        private IEnumerator Fade(float target)
        {
            var fader = ScreenFader.Instance;

            if (fader != null)
            {
                yield return fader.FadeTo(target, fadeDuration);
            }
        }
    }
}
