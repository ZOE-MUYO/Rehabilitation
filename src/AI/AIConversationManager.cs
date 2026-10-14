using UnityEngine;

public class AIConversationManager : MonoBehaviour
{
    public static AIConversationManager Instance;

    //==================================================
    // AI类型
    //==================================================
    public enum AIType
    {
        None,
        Staff,
        Salesman
    }

    //==================================================
    // 医院要求的物品
    //==================================================
    public enum RequiredItem
    {
        None,
        Seal,
        Document
    }

    private AIType currentType = AIType.None;
    private RequiredItem requiredItem = RequiredItem.None;

    private float conversationStartTime = 0f;
    private bool conversationRunning = false;

    private bool salesReject = false;
    private bool correctItem = false;

    private void Awake()
    {
        Instance = this;
    }

    //==================================================
    // 对话开始
    //==================================================
    public void StartConversation(AIType type)
    {
        currentType = type;

        conversationRunning = true;
        conversationStartTime = Time.time;

        switch (type)
        {
            case AIType.Staff:

                SQLiteManager.Instance.UpdateAIType("Staff");
                SQLiteManager.Instance.UpdateSalesReject("-");
                SQLiteManager.Instance.UpdateCorrectItem("NO");

                break;

            case AIType.Salesman:

                SQLiteManager.Instance.UpdateAIType("Salesman");
                SQLiteManager.Instance.UpdateSalesReject("NO");
                SQLiteManager.Instance.UpdateCorrectItem("-");

                break;
        }

        salesReject = false;
        correctItem = false;

        Debug.Log("Conversation Start");
    }

    //==================================================
    // 对话结束
    //==================================================
    public void EndConversation()
    {
        if (!conversationRunning)
            return;

        conversationRunning = false;

        float time = Time.time - conversationStartTime;

        SQLiteManager.Instance.UpdateConversationTime(time);

        Debug.Log("Conversation End : " + time);
    }

    //==================================================
    // 设置医院要求物品
    //==================================================
    public void SetRequiredItem(RequiredItem item)
    {
        requiredItem = item;

        Debug.Log("Required Item : " + item);
    }

    //==================================================
    // Drawer检测物品
    //==================================================
    public void CheckItem(string objectName)
    {
        if (currentType != AIType.Staff)
            return;


        bool correct = false;


        objectName = objectName.ToLower();


        switch (requiredItem)
        {

            // =========================
            // 印章
            // =========================
            case RequiredItem.Seal:

                if (objectName.Contains("seal"))
                {
                    correct = true;
                }

                break;



            // =========================
            // 确认书
            // =========================
            case RequiredItem.Document:

                if (objectName.Contains("sm_newspaper3"))
                {
                    correct = true;
                }

                break;
        }



        if (correct)
        {
            correctItem = true;

            SQLiteManager.Instance.UpdateCorrectItem("YES");

            Debug.Log("Correct Item : YES");
        }
        else
        {
            correctItem = false;

            SQLiteManager.Instance.UpdateCorrectItem("NO");

            Debug.Log("Correct Item : NO");
        }
    }

    //==================================================
    // 每一句Whisper识别完成
    //==================================================
    public void ReceiveUserText(string text)
    {
        if (!conversationRunning)
            return;

        float time = Time.time - conversationStartTime;

        SQLiteManager.Instance.UpdateConversationTime(time);

        if (currentType == AIType.Salesman)
        {
            if (!salesReject && IsRejectSentence(text))
            {
                salesReject = true;

                SQLiteManager.Instance.UpdateSalesReject("YES");

                Debug.Log("Sales Reject YES");
            }
        }
    }

    //==================================================
    // 手动设置CorrectItem
    //==================================================
    public void SetCorrectItem(bool value)
    {
        if (currentType != AIType.Staff)
            return;

        correctItem = value;

        SQLiteManager.Instance.UpdateCorrectItem(value ? "YES" : "NO");
    }

    //==================================================
    // 判断拒绝销售
    //==================================================
    private bool IsRejectSentence(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        string[] keywords =
        {
            "いりません",
            "要りません",
            "必要ありません",
            "結構です",
            "遠慮します",
            "買いません",
            "興味ありません",
            "大丈夫です",
            "また今度",
            "今回はいいです",
            "すみません",
            "不要",
            "いらない",
            "忙しい",
            "時間がありません"
        };

        foreach (string key in keywords)
        {
            if (text.Contains(key))
            {
                return true;
            }
        }

        return false;
    }

    //==================================================
    // 实时更新会话时间
    //==================================================
    private void Update()
    {
        if (!conversationRunning)
            return;

        float time = Time.time - conversationStartTime;

        SQLiteManager.Instance.UpdateConversationTime(time);
    }
}