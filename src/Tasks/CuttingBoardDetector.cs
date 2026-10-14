using UnityEngine;

public class CuttingBoardDetector : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        string foodName = other.gameObject.name;

        Debug.Log("Board: " + foodName);

        // ❌ 排除非食物
        if (!foodName.Contains("Food"))
            return;

        // 👉 ⭐关键：调用 FridgeTaskManager
        if (FridgeTaskManager.Instance != null)
        {
            FridgeTaskManager.Instance.OnFoodPlaced(foodName);
        }
        else
        {
            Debug.LogError("FridgeTaskManager is NULL");
        }
    }
}