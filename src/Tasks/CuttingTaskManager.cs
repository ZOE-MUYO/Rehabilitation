using UnityEngine;

public class CuttingTaskManager : MonoBehaviour
{
    public static CuttingTaskManager Instance;

    private float firstCutTime = -1f;
    private float lastCutTime = -1f;

    void Awake()
    {
        Instance = this;
    }

    public void ResetCut()
    {
        firstCutTime = -1f;
        lastCutTime = -1f;
    }

    public void OnFoodCut(string foodName)
    {
        if (!foodName.Contains("SlicedPart"))
            return;

        float now = Time.time;

        if (firstCutTime < 0f)
        {
            firstCutTime = now;
            Debug.Log("First Cut Time: " + firstCutTime);
        }

        lastCutTime = now;

        float cutTime = lastCutTime - firstCutTime;

        Debug.Log("Cut Time Saved -> " + cutTime);

        SQLiteManager.Instance.UpdateCutTime(cutTime);
    }

    private int cutErrorCount = 0;

    public void AddCutError()
    {
        cutErrorCount++;

        Debug.Log("Cut Error Count -> " + cutErrorCount);

        SQLiteManager.Instance.UpdateCutErrorCount(cutErrorCount);
    }
}