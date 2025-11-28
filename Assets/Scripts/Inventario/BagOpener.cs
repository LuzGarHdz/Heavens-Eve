using UnityEngine;

public class BagOpener : MonoBehaviour
{
    public void OnBagClicked()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.ToggleInventory();
    }
}