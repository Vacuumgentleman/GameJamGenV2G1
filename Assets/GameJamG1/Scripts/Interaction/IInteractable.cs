namespace GameJamG1.Interaction
{
    public interface IInteractable
    {
        bool IsLocked { get; }

        string GetPrompt();

        bool CanInteract();

        void Interact(PlayerInteractor interactor);

        void InteractLocked(PlayerInteractor interactor);

        void SetHighlight(bool highlighted);
    }
}
