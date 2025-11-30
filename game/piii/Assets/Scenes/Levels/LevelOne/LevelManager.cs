using UnityEngine;

public class LevelController : MonoBehaviour
{
    [Header("Setup")]
    public Transform itemsContainer;
    public GameObject winMenu;

    private DraggableItem[] allItems;

    void Start()
    {
        if (winMenu != null) winMenu.SetActive(false);

        if (itemsContainer != null)
        {
            allItems = itemsContainer.GetComponentsInChildren<DraggableItem>();
        }
        else
        {
            Debug.LogError("LevelController: Please assign the 'Items' container in the Inspector!");
        }
    }

    public void CheckLevelCompletion()
    {
        if (allItems == null || allItems.Length == 0) return;

        foreach (DraggableItem item in allItems)
        {
            if (!item.IsPlaced)
            {
                return;
            }
        }

        LevelCompleted();
    }

    void LevelCompleted()
    {
        Debug.Log("LEVEL COMPLETED!");

        if (winMenu != null) winMenu.SetActive(true);

        // Optional: Play sound or save progress here
        // AudioManager.Instance.PlaySFX("WinSound");
    }
}