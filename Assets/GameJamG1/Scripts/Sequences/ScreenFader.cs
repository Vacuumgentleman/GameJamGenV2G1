using System.Collections;
using UnityEngine;

namespace GameJamG1.Sequences
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ScreenFader : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;

        public static ScreenFader Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Instance = null;
        }

        private void Awake()
        {
            Instance = this;
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public IEnumerator FadeTo(float target, float duration)
        {
            var start = group.alpha;
            var elapsed = 0f;

            while (elapsed < 1f)
            {
                elapsed += Time.unscaledDeltaTime / Mathf.Max(duration, 0.01f);
                group.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed));
                yield return null;
            }

            group.alpha = target;
        }

        public void FadeOut(float duration)
        {
            StartCoroutine(FadeTo(1f, duration));
        }

        public void FadeIn(float duration)
        {
            StartCoroutine(FadeTo(0f, duration));
        }

        private void OnValidate()
        {
            if (group == null)
            {
                group = GetComponent<CanvasGroup>();
            }
        }
    }
}
