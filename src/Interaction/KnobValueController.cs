using UnityEngine;
using UnityEngine.XR.Content.Interaction;
using UnityEngine.XR.Interaction.Toolkit;

public class KnobValueController : MonoBehaviour
{
    public XRKnob knob; // XR Knob组件
    public new  ParticleSystem particleSystem; // 粒子系统引用

    // 必须是public，并且参数是float类型
    public void CheckKnobValue(float value)
    {
        if (value > 0.1f)
        {
            if (!particleSystem.isPlaying)
                particleSystem.Play();
        }
        else
        {
            if (particleSystem.isPlaying)
                particleSystem.Stop();
        }
    }
}
