using UnityEngine;

public class RecipeCheckManager : MonoBehaviour
{
    public static RecipeCheckManager Instance;

    public Camera playerCamera;

    public float maxDistance = 0.8f;

    public float viewAngle = 30f;

    public float readTime = 1f;

    private bool isReading = false;

    private float timer = 0;

    private int count = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (playerCamera == null)
            return;

        float distance =
            Vector3.Distance(playerCamera.transform.position,
                             transform.position);

        Vector3 dir =
            (transform.position - playerCamera.transform.position).normalized;

        float angle =
            Vector3.Angle(playerCamera.transform.forward, dir);

        bool looking =
            distance < maxDistance &&
            angle < viewAngle;

        if (looking)
        {
            timer += Time.deltaTime;

            if (!isReading && timer >= readTime)
            {
                isReading = true;

                count++;

                Debug.Log("Recipe Read = " + count);

                SQLiteManager.Instance.UpdateRecipeCheckCount(count);
            }
        }
        else
        {
            timer = 0;
            isReading = false;
        }
    }
}