using TMPro;
using UnityEngine;

namespace GameJamG1.Items
{
    public class MatchCounterUI : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Optional. Found automatically in the scene when empty.")]
        [SerializeField] private MatchBox matchBox;
        [SerializeField] private TMP_Text label;

        [Header("Format")]
        [Tooltip("{0} is the number of matches.")]
        [SerializeField] private string format = "Cerillas: {0}";

        private void Start()
        {
            if (matchBox == null)
            {
                matchBox = FindFirstObjectByType<MatchBox>();
            }

            if (matchBox == null)
            {
                return;
            }

            matchBox.CountChanged += Refresh;
            Refresh(matchBox.Count);
        }

        private void OnDestroy()
        {
            if (matchBox != null)
            {
                matchBox.CountChanged -= Refresh;
            }
        }

        private void Refresh(int count)
        {
            label.text = string.Format(format, count);
        }

        private void OnValidate()
        {
            if (label == null)
            {
                Debug.LogWarning($"{name}: MatchCounterUI has no label assigned.", this);
            }
        }
    }
}
