using System.Collections;
using UnityEngine;

public class DoorbellController : MonoBehaviour
{
    public static DoorbellController Instance;

    [Header("Doorbell Settings")]
    public AudioSource doorbellAudio;
    public float minDelay = 10f;
    public float maxDelay = 40f;

    public ParticleSystem[] fireEffects;
    public GameObject potatoChunk;
    public GameObject carrotChunk;

    public static bool DoorbellFinished = false;

    private bool isTriggered = false;

    // =========================
    // Doorbell Response System
    // =========================
    public bool BellIsPlaying { get; private set; } = false;
    public bool WaitingForResponse => waitingForResponse;

    private bool waitingForResponse = false;
    private float bellStartTime = -1f;

    [Header("Microwave Random Bell")]
    public bool MicrowaveTimerRunning = false;
    private bool microwaveBellTriggered = false;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(AutoCheckRoutine());
        StartCoroutine(MicrowaveBellRoutine());
    }

    IEnumerator AutoCheckRoutine()
    {
        yield return new WaitForSeconds(1f);

        while (!isTriggered)
        {
            TryTriggerBell();
            yield return new WaitForSeconds(2f);
        }
    }

    private void TryTriggerBell()
    {
        if (isTriggered) return;

        bool anyFirePlaying = false;

        foreach (ParticleSystem fire in fireEffects)
        {
            if (fire != null && fire.isPlaying)
            {
                anyFirePlaying = true;
                break;
            }
        }

        if (!anyFirePlaying) return;

        bool potatoVisible = potatoChunk != null && potatoChunk.activeInHierarchy;
        bool carrotVisible = carrotChunk != null && carrotChunk.activeInHierarchy;

        if (!potatoVisible || !carrotVisible) return;

        isTriggered = true;

        float delay = Random.Range(minDelay, maxDelay);

        Debug.Log($"满足条件，{delay:F1} 秒后播放门铃");

        Invoke(nameof(PlayDoorbell), delay);
    }

    void PlayDoorbell()
    {
        if (doorbellAudio == null)
            return;

        BellIsPlaying = true;
        waitingForResponse = true;

        bellStartTime = Time.time;

        // 默认 NO
        if (SQLiteManager.Instance != null)
        {
            SQLiteManager.Instance.UpdateDoorbellNotice("NO");
        }

        doorbellAudio.Play();

        Debug.Log("🔔 Doorbell Started");

        StartCoroutine(WaitForDoorbellEnd());
    }

    public void DoorbellAnswered()
    {
        if (!waitingForResponse)
            return;

        waitingForResponse = false;

        float responseTime = Time.time - bellStartTime;

        Debug.Log($"Doorbell Response = {responseTime:F2}s");

        if (SQLiteManager.Instance != null)
        {
            SQLiteManager.Instance.UpdateDoorbellNotice("YES");
            SQLiteManager.Instance.UpdateDoorbellResponseTime(responseTime);
        }
    }

    IEnumerator WaitForDoorbellEnd()
    {
        if (doorbellAudio != null)
        {
            yield return new WaitWhile(() => doorbellAudio.isPlaying);
        }

        BellIsPlaying = false;

        DoorbellFinished = true;

        Debug.Log("门铃播放结束（但仍可响应）");
    }

    IEnumerator MicrowaveBellRoutine()
    {
        while (true)
        {
            if (MicrowaveTimerRunning && !microwaveBellTriggered)
            {
                microwaveBellTriggered = true;

                if (doorbellAudio != null)
                {
                    float randomDelay = Random.Range(minDelay, maxDelay);

                    Debug.Log($"微波炉倒计时中，{randomDelay:F1} 秒后播放门铃");

                    yield return new WaitForSeconds(randomDelay);

                    BellIsPlaying = true;
                    waitingForResponse = true;

                    bellStartTime = Time.time;

                    if (SQLiteManager.Instance != null)
                    {
                        SQLiteManager.Instance.UpdateDoorbellNotice("NO");
                    }

                    doorbellAudio.Play();

                    StartCoroutine(WaitForDoorbellEnd());
                }
                else
                {
                    Debug.LogWarning("doorbellAudio 未设置！");
                    microwaveBellTriggered = false;
                }
            }

            if (!MicrowaveTimerRunning && microwaveBellTriggered)
            {
                microwaveBellTriggered = false;
            }

            yield return null;
        }
    }
}