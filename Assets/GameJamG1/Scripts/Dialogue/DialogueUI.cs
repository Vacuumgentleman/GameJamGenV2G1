using System.Collections;
using TMPro;
using UnityEngine;

namespace GameJamG1.Dialogue
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DialogueUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private AudioSource blipSource;

        [Header("Typing")]
        [SerializeField, Min(1f)] private float charsPerSecond = 30f;
        [SerializeField, Min(0f)] private float minDuration = 1.5f;
        [SerializeField, Min(0f)] private float holdTime = 1f;

        [Header("Blip")]
        [SerializeField] private AudioClip blipClip;
        [SerializeField, Min(1)] private int blipEveryNChars = 2;
        [SerializeField] private Vector2 blipPitchRange = new(0.9f, 1.1f);
        [SerializeField, Range(0f, 1f)] private float blipVolume = 0.5f;

        [Header("Fade")]
        [SerializeField, Min(0f)] private float fadeDuration = 0.25f;

        private void Awake()
        {
            HideImmediate();
        }

        public IEnumerator ShowPanel()
        {
            yield return Fade(1f);
        }

        public IEnumerator HidePanel()
        {
            yield return Fade(0f);
            dialogueText.text = string.Empty;
        }

        public void HideImmediate()
        {
            canvasGroup.alpha = 0f;
            dialogueText.text = string.Empty;
        }

        public IEnumerator PlayLine(DialogueLine line)
        {
            dialogueText.text = line.Text;
            dialogueText.maxVisibleCharacters = 0;
            dialogueText.ForceMeshUpdate();

            var total = dialogueText.textInfo.characterCount;
            var revealTime = total / charsPerSecond;
            var duration = line.DurationOverride > 0f ? line.DurationOverride : Mathf.Max(minDuration, revealTime + holdTime);
            var effectiveReveal = Mathf.Min(revealTime, duration);
            var clip = line.Sound != null ? line.Sound : blipClip;

            var elapsed = 0f;
            var shown = 0;
            var sinceBlip = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var target = effectiveReveal > 0f
                    ? Mathf.Min(total, Mathf.FloorToInt(elapsed / effectiveReveal * total))
                    : total;

                if (target != shown)
                {
                    sinceBlip += target - shown;
                    shown = target;
                    dialogueText.maxVisibleCharacters = shown;

                    if (sinceBlip >= blipEveryNChars)
                    {
                        sinceBlip = 0;
                        PlayBlip(clip);
                    }
                }

                yield return null;
            }

            dialogueText.maxVisibleCharacters = total;
        }

        private void PlayBlip(AudioClip clip)
        {
            if (blipSource == null || clip == null)
            {
                return;
            }

            blipSource.pitch = Random.Range(blipPitchRange.x, blipPitchRange.y);
            blipSource.PlayOneShot(clip, blipVolume);
        }

        private IEnumerator Fade(float target)
        {
            var start = canvasGroup.alpha;
            var elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(start, target, elapsed / fadeDuration);
                yield return null;
            }

            canvasGroup.alpha = target;
        }

        private void OnValidate()
        {
            if (dialogueText == null)
            {
                Debug.LogWarning($"{name}: DialogueUI has no text assigned.", this);
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (blipClip == null)
            {
                Debug.LogWarning($"{name}: DialogueUI has no blip clip, lines will be silent.", this);
            }
        }
    }
}
