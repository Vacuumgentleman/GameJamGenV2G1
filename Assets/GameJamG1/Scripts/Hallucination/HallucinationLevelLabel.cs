using TMPro;
using UnityEngine;

namespace GameJamG1.Hallucination
{
    public class HallucinationLevelLabel : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [Tooltip("{0} is the current level.")]
        [SerializeField] private string format = "Alucinación: {0}";

        private HallucinationManager manager;

        private void Start()
        {
            manager = HallucinationManager.Instance;

            if (manager == null)
            {
                return;
            }

            manager.LevelChanged += Refresh;
            Refresh(manager.Level);
        }

        private void OnDestroy()
        {
            if (manager != null)
            {
                manager.LevelChanged -= Refresh;
            }
        }

        private void Refresh(int level)
        {
            label.text = string.Format(format, level);
        }

        private void OnValidate()
        {
            if (label == null)
            {
                Debug.LogWarning($"{name}: HallucinationLevelLabel has no label assigned.", this);
            }
        }
    }
}
