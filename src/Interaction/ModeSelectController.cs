using UnityEngine;
using UnityEngine.SceneManagement;

public class ModeSelectController : MonoBehaviour
{
    // 按下“练习模式”
    public void OnPracticeButton()
    {
        SceneManager.LoadScene("PracticeScene");
    }

    // 按下“正式开始”
    public void OnStartButton()
    {
        SceneManager.LoadScene("Rehabilitation");
    }
}
