using UnityEngine;

public class DrawerItemDetector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (AIConversationManager.Instance == null)
            return;


        string itemName = other.gameObject.name;


        Debug.Log("Drawer Detect Item : " + itemName);


        AIConversationManager.Instance.CheckItem(itemName);
    }
}