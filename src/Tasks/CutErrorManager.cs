using UnityEngine;

public class CutErrorManager : MonoBehaviour
{
    public static CutErrorManager Instance;

    private int cutErrorCount = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void AddError()
    {
        cutErrorCount++;

        Debug.Log("Cut Error Count = " + cutErrorCount);

        if (SQLiteManager.Instance != null)
        {
            SQLiteManager.Instance.UpdateCutErrorCount(cutErrorCount);
        }
    }
}