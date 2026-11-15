using System.Collections;
using UnityEngine;

namespace GameJamG1.Enemy
{
    [RequireComponent(typeof(MonsterAnimationDriver))]
    public class MonsterIdleScreamer : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float minInterval = 6f;
        [SerializeField, Min(0.1f)] private float maxInterval = 14f;

        private MonsterAnimationDriver driver;

        private void Awake()
        {
            driver = GetComponent<MonsterAnimationDriver>();
        }

        private void OnEnable()
        {
            StartCoroutine(ScreamLoop());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private void OnValidate()
        {
            maxInterval = Mathf.Max(maxInterval, minInterval);
        }

        private IEnumerator ScreamLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
                driver.TriggerScream();
            }
        }
    }
}
