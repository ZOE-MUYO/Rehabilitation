using UnityEngine;
using TMPro;

public class UserInputManager : MonoBehaviour
{
    public TMP_InputField inputField;

    public void OnClickStart()
    {
        string name = inputField.text;

        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("名前が空");
            return;
        }

        ExperimentSession.Instance.StartSession(name);

        SQLiteManager.Instance.CreateSession(
            name,
            ExperimentSession.Instance.startTime
        );

        Debug.Log("Experiment Started: " + name);
    }
}