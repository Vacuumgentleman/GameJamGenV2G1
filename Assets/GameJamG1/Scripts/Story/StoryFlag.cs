using UnityEngine;

namespace GameJamG1.Story
{
    [CreateAssetMenu(fileName = "NewStoryFlag", menuName = "GameJamG1/Story/Flag")]
    public class StoryFlag : ScriptableObject
    {
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;

        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public string Description => description;
    }
}
