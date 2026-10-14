using UnityEngine;

public class GusStopMonitor : MonoBehaviour
{
    public static GusStopMonitor Instance;

    [Header("Fire References")]
    public ParticleSystem fireBlue1;
    public ParticleSystem fireBlue2;

    [Header("Threshold")]
    public float stopThreshold = 5f;

    private float stopStartTime = -1f;
    private bool gusActive = false;
    private bool gusStopRecorded = false;

    private void Awake()
    {
        Instance = this;
    }

    public void SetGusActive(bool active)
    {
        gusActive = active;

        if (!active)
        {
            stopStartTime = -1f;
            gusStopRecorded = false;
        }
    }

    void Update()
    {
        if (!gusActive)
            return;

        bool isFirePlaying =
            (fireBlue1 != null && fireBlue1.isPlaying) ||
            (fireBlue2 != null && fireBlue2.isPlaying);

        // 火正在播放，重置计时
        if (isFirePlaying)
        {
            stopStartTime = -1f;
            return;
        }

        // 火全部停止，开始计时
        if (stopStartTime < 0f)
        {
            stopStartTime = Time.time;
        }

        // 停止超过5秒
        if (!gusStopRecorded && Time.time - stopStartTime >= stopThreshold)
        {
            gusStopRecorded = true;

            Debug.Log("GUS STOP = YES");

            if (SQLiteManager.Instance != null)
            {
                SQLiteManager.Instance.UpdateGusStop("YES");
            }
        }
    }
}