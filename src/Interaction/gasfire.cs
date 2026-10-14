using UnityEngine;

public class GasFireTouchDetect : MonoBehaviour
{
    public GameObject lightObj;
    public ParticleSystem flamesEffect;
    public Color defaultColor = Color.gray;
    public Color activeColor = Color.red;
    public float cooldownTime = 1.0f;

    public GameObject[] handObjects;  // ✅ 支持多个手（左右手都能拖进来）

    private Renderer lightRenderer;
    private bool isActive = false;
    private float lastToggleTime = -Mathf.Infinity;

    void Start()
    {
        if (lightObj != null)
        {
            lightRenderer = lightObj.GetComponent<Renderer>();
            lightRenderer.material = new Material(lightRenderer.material);
            lightRenderer.material.color = defaultColor;
        }

        if (flamesEffect != null)
        {
            flamesEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    void Update()
    {
        if (Time.time - lastToggleTime < cooldownTime) return;

        foreach (GameObject hand in handObjects)
        {
            if (hand != null && GetComponent<Collider>() != null && hand.GetComponent<Collider>() != null)
            {
                Bounds myBounds = GetComponent<Collider>().bounds;
                Bounds handBounds = hand.GetComponent<Collider>().bounds;

                if (myBounds.Intersects(handBounds))
                {
                    Toggle();
                    lastToggleTime = Time.time;
                    break; // 一只手触发就够了
                }
            }
        }
    }

    void Toggle()
    {
        isActive = !isActive;

        if (lightRenderer != null)
        {
            lightRenderer.material.color = isActive ? activeColor : defaultColor;
        }

        if (flamesEffect != null)
        {
            if (isActive)
                flamesEffect.Play();
            else
                flamesEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        Debug.Log("触碰成功！当前状态：" + (isActive ? "点火" : "熄火"));
    }
}
