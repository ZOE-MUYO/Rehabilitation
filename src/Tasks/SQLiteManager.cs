using UnityEngine;
using SQLite;

public class SQLiteManager : MonoBehaviour
{
    public static SQLiteManager Instance;

    private SQLiteConnection db;
    private string dbPath;

    void Awake()
    {
        Instance = this;

        dbPath = System.IO.Path.Combine(Application.persistentDataPath, "experiment.db");

        db = new SQLiteConnection(dbPath);

        db.Execute(
        @"CREATE TABLE IF NOT EXISTS ExperimentDataDB (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            userName TEXT,
            startTime TEXT,
            fridgeTime REAL,
            fridgeErrorCount INTEGER,
            cutTime REAL,
            cutErrorCount INTEGER,
            potTime REAL,
            potErrorCount INTEGER,
            gusStop TEXT,
            recipeCheckCount INTEGER,
            finalComplete TEXT,
            doorbellNotice TEXT,
            doorbellResponseTime REAL,
            aiType TEXT,
            salesReject TEXT,
            correctItem TEXT,
            conversationTime REAL
        )");

        Debug.Log("DB Ready");
    }

    // Session
    public void CreateSession(string userName, string startTime)
    {
                db.Execute(
        @"INSERT INTO ExperimentDataDB
        (
            userName,
            startTime,
            fridgeTime,
            fridgeErrorCount,
            cutTime,
            cutErrorCount,
            potTime,
            potErrorCount,
            gusStop,
            recipeCheckCount,
            finalComplete,
            doorbellNotice,
            doorbellResponseTime,
            aiType,
            salesReject,
            correctItem,
            conversationTime
        )
        VALUES
        (
            ?, ?, 
            0, 0, 
            0, 0, 
            0, 0, 
            0,
            0,
            'NO',
            'NO',
            0,
            '-',
            '-',
            '-',
            0
        )",
        userName,
        startTime
        );

        Debug.Log("Session created");
    }

    // Fridge
    public void UpdateFridgeTime(float value)
    {
        db.Execute(
        @"UPDATE ExperimentDataDB
          SET fridgeTime=?
          WHERE id=(SELECT MAX(id) FROM ExperimentDataDB)",
        value);
    }

    public void UpdateFridgeErrorCount(int value)
    {
        db.Execute(
        @"UPDATE ExperimentDataDB
          SET fridgeErrorCount=?
          WHERE id=(SELECT MAX(id) FROM ExperimentDataDB)",
        value);
    }

    // Cut
    public void UpdateCutTime(float value)
    {
        db.Execute(
        @"UPDATE ExperimentDataDB
          SET cutTime=?
          WHERE id=(SELECT MAX(id) FROM ExperimentDataDB)",
        value);
    }

    public void UpdateCutErrorCount(int value)
    {
        db.Execute(
        @"UPDATE ExperimentDataDB
          SET cutErrorCount=?
          WHERE id=(SELECT MAX(id) FROM ExperimentDataDB)",
        value);
    }

    // Pot
    public void UpdatePotTime(float value)
    {
        db.Execute(
        @"UPDATE ExperimentDataDB
          SET potTime=?
          WHERE id=(SELECT MAX(id) FROM ExperimentDataDB)",
        value);

        Debug.Log("PotTime = " + value);
    }

    public void UpdatePotErrorCount(int value)
    {
        db.Execute(
        @"UPDATE ExperimentDataDB
          SET potErrorCount=?
          WHERE id=(SELECT MAX(id) FROM ExperimentDataDB)",
        value);

        Debug.Log("PotErrorCount = " + value);
    }

    //gus
    public void UpdateGusStop(string value)
{
    db.Execute(
        @"UPDATE ExperimentDataDB
          SET gusStop = ?
          WHERE id = (SELECT MAX(id) FROM ExperimentDataDB)",
        value
    );
}

    //recipe

    public void UpdateRecipeCheckCount(int count)
    {
        db.Execute(
            @"UPDATE ExperimentDataDB
          SET recipeCheckCount = ?
          WHERE id = (SELECT MAX(id) FROM ExperimentDataDB)",
            count);

        Debug.Log("RecipeCheckCount -> " + count);
    }

    public void UpdateFinalComplete(string value)
    {
        db.Execute(
            @"UPDATE ExperimentDataDB
          SET finalComplete = ?
          WHERE id = (SELECT MAX(id) FROM ExperimentDataDB)",
            value);

        Debug.Log("FinalComplete -> " + value);
    }

    //Doorbell

    public void UpdateDoorbellNotice(string value)
    {
        db.Execute(
            @"UPDATE ExperimentDataDB
          SET doorbellNotice = ?
          WHERE id = (SELECT MAX(id) FROM ExperimentDataDB)",
            value);

        Debug.Log("DoorbellNotice -> " + value);
    }

    public void UpdateDoorbellResponseTime(float time)
    {
        db.Execute(
            @"UPDATE ExperimentDataDB
          SET doorbellResponseTime = ?
          WHERE id = (SELECT MAX(id) FROM ExperimentDataDB)",
            time);

        Debug.Log("DoorbellResponseTime -> " + time);
    }

    //AI
    public void UpdateAIType(string value)
    {
        db.Execute(
            @"UPDATE ExperimentDataDB
        SET aiType=?
        WHERE id=(SELECT MAX(id) FROM ExperimentDataDB)",
            value);
    }

    public void UpdateSalesReject(string value)
    {
        db.Execute(
            @"UPDATE ExperimentDataDB
        SET salesReject=?
        WHERE id=(SELECT MAX(id) FROM ExperimentDataDB)",
            value);
    }

    public void UpdateCorrectItem(string value)
    {
        db.Execute(
            @"UPDATE ExperimentDataDB
        SET correctItem=?
        WHERE id=(SELECT MAX(id) FROM ExperimentDataDB)",
            value);
    }

    public void UpdateConversationTime(float value)
    {
        db.Execute(
            @"UPDATE ExperimentDataDB
        SET conversationTime=?
        WHERE id=(SELECT MAX(id) FROM ExperimentDataDB)",
            value);
    }
}
