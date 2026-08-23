using UnityEngine;

namespace BoneHaven
{
    public interface IInteractable
    {
        string GetPromptText();
        void Interact(GameObject interactor);
    }
}