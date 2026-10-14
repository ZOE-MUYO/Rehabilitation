using UnityEngine;
using System.Collections.Generic;

public class FridgeTaskManager : MonoBehaviour
{
    public static FridgeTaskManager Instance;

    // =========================
    // 时间数据
    // =========================
    private float firstGrabTime = -1f;
    private float lastFoodPlaceTime = -1f;

    // 防重复记录
    private HashSet<string> countedFoods = new HashSet<string>();

    // 错误计数
    private int fridgeErrorCount = 0;

    void Awake()
    {
        Instance = this;
    }

    // =========================
    // 第一次抓取 Food
    // =========================
    public void OnFoodGrab(string foodName)
    {
        if (!foodName.Contains("Food"))
            return;

        if (firstGrabTime < 0f)
        {
            firstGrabTime = Time.time;
            Debug.Log("First Grab Time: " + firstGrabTime);
        }
    }

    // =========================
    // 放到案板
    // =========================
    public void OnFoodPlaced(string foodName)
    {
        Debug.Log("Board: " + foodName);

        // ❌ 排除碎片
        if (foodName.Contains("SlicedPart"))
            return;

        // ❌ 只处理 Food
        if (!foodName.Contains("Food"))
            return;

        // ❌ 防止同一个物体重复记录
        if (countedFoods.Contains(foodName))
            return;

        countedFoods.Add(foodName);

        // =========================
        // 更新最后时间
        // =========================
        lastFoodPlaceTime = Time.time;

        // =========================
        // 计算 FridgeTime
        // =========================
        if (firstGrabTime >= 0f)
        {
            float fridgeTime = lastFoodPlaceTime - firstGrabTime;

            Debug.Log("Saved FridgeTime -> " + fridgeTime);

            if (SQLiteManager.Instance != null)
            {
                SQLiteManager.Instance.UpdateFridgeTime(fridgeTime);
            }
        }
        else
        {
            Debug.LogWarning("FridgeTime 计算失败：FirstGrabTime 未初始化");
        }

        // =========================
        // 错误判断
        // =========================
        bool isCorrect =
            foodName.Contains("Potato") ||
            foodName.Contains("Carrot");

        if (!isCorrect)
        {
            fridgeErrorCount++;

            if (SQLiteManager.Instance != null)
            {
                SQLiteManager.Instance.UpdateFridgeErrorCount(fridgeErrorCount);
            }

            Debug.Log("Error Count = " + fridgeErrorCount);
        }
    }

    // =========================
    // 重置（每轮实验必须调用）
    // =========================
    public void ResetFridgeTask()
    {
        firstGrabTime = -1f;
        lastFoodPlaceTime = -1f;
        countedFoods.Clear();
        fridgeErrorCount = 0;

        Debug.Log("Fridge Reset");
    }
}