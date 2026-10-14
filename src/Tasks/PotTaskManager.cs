using UnityEngine;

public class PotTaskManager : MonoBehaviour
{
    public static PotTaskManager Instance;

    private float firstPutTime = -1f;
    private float lastPutTime = -1f;

    private bool potatoAdded = false;
    private bool carrotAdded = false;
    private bool waterAdded = false;

    private int potErrorCount = 0;

    private void Awake()
    {
        Instance = this;

        Debug.Log("PotTaskManager Started");
    }

    void RecordTime()
    {
        float now = Time.time;

        if (firstPutTime < 0)
        {
            firstPutTime = now;
        }

        lastPutTime = now;

        float potTime = lastPutTime - firstPutTime;

        if (SQLiteManager.Instance != null)
        {
            SQLiteManager.Instance.UpdatePotTime(potTime);
        }

        Debug.Log("PotTime = " + potTime);
    }

    public void AddPotato()
    {
        if (potatoAdded) return;

        potatoAdded = true;

        Debug.Log("AddPotato Called");

        RecordTime();
    }

    public void AddCarrot()
    {
        if (carrotAdded) return;

        carrotAdded = true;

        Debug.Log("AddCarrot Called");

        RecordTime();
    }

    public void AddWater()
    {
        if (waterAdded) return;

        waterAdded = true;

        Debug.Log("AddWater Called");

        // 水必须最后放
        if (!(potatoAdded && carrotAdded))
        {
            potErrorCount++;

            if (SQLiteManager.Instance != null)
            {
                SQLiteManager.Instance.UpdatePotErrorCount(potErrorCount);
            }

            Debug.Log("Pot Order Error");
        }

        RecordTime();
    }
}