using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FoodGrabListener : MonoBehaviour
{
    private XRGrabInteractable grab;


    void Start()
    {
        grab = GetComponent<XRGrabInteractable>();

        if (grab != null)
        {
            grab.selectEntered.AddListener(OnGrab);
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        Debug.Log("Grab: " + gameObject.name);


        if (gameObject.name.Contains("Food"))
        {
            FridgeTaskManager.Instance.OnFoodGrab(gameObject.name);
        }
    }
}