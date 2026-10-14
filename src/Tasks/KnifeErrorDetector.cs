using UnityEngine;

public class KnifeErrorDetector : MonoBehaviour
{
    [Header("Cooldown")]
    [SerializeField]
    private float cooldown = 5f;

    private float lastHandErrorTime = -999f;
    private float lastSelfDangerTime = -999f;

    [Header("Knife Points")]
    public Transform startSlicePoint;
    public Transform endSlicePoint;

    [Header("Player")]
    public Transform playerCamera;

    [Header("Self-hit Settings")]
    [SerializeField]
    private float selfHitThreshold = 0.7f;

    [SerializeField]
    private float maxDistance = 1.2f;

    private void OnTriggerEnter(Collider other)
    {
        string objName = other.name;

        bool isHand =
            objName.Contains("LeftHand Controller") ||
            objName.Contains("RightHand Controller");

        // -----------------------------
        // ① 手误触检测（独立 cooldown）
        // -----------------------------
        if (isHand)
        {
            if (Time.time - lastHandErrorTime >= cooldown)
            {
                lastHandErrorTime = Time.time;

                Debug.Log("❌ Hand Error");

                if (CutErrorManager.Instance != null)
                {
                    CutErrorManager.Instance.AddError(); // 或 AddHandError()
                }
            }
        }

        // -----------------------------
        // ② 自身危险方向检测（独立 cooldown）
        // -----------------------------
        if (startSlicePoint != null && endSlicePoint != null && playerCamera != null)
        {
            Vector3 knifeDir =
                (endSlicePoint.position - startSlicePoint.position).normalized;

            Vector3 viewDir = playerCamera.forward.normalized;

            float dot = Vector3.Dot(knifeDir, viewDir);

            float distance =
                Vector3.Distance(endSlicePoint.position, playerCamera.position);

            bool isSelfDanger =
                dot > selfHitThreshold && distance < maxDistance;

            if (isSelfDanger)
            {
                if (Time.time - lastSelfDangerTime >= cooldown)
                {
                    lastSelfDangerTime = Time.time;

                    Debug.Log("❌ Self Danger Error");

                    if (CutErrorManager.Instance != null)
                    {
                        CutErrorManager.Instance.AddError();
                    }
                }
            }
        }
    }
}