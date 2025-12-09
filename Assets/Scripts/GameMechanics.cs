using UnityEngine;
using TMPro;

public class GameMechanics : MonoBehaviour
{
    [Header("Timer Settings")]
    public TextMeshProUGUI timerText;
    private bool startTimeOnStart = true;

    private float elaspedTime = 0f;
    private bool isTimerRunning = false;
    private bool gameComplete = false;

    //start is called once before the first execution of Update after the MonoBehaviour is created
    //start is a Unity builtin function
    void Start()
    {
        //call startfunction method if the boolean is true
        if (startTimeOnStart)
        {
            startTimer();
        }
    }

    //update is called once per frame and is a Unity builtin function
    void Update()
    {   //check if isTimerRunning is true and if it is increment elaspedTime by deltaTime and call updateTimerDisplay function
        if (isTimerRunning)
        {
            elaspedTime += Time.deltaTime;
            updateTimerDisplay();
        }
    }

    void updateTimerDisplay()
    {
        //calculate minutes, seconds, milliseconds from elaspedTime
        int minutes = Mathf.FloorToInt(elaspedTime / 60f);
        int seconds = Mathf.FloorToInt(elaspedTime % 60f);
        int milliseconds = Mathf.FloorToInt((elaspedTime * 1000f) % 1000f);

        //update the timerText UI element with the formatted time string
        timerText.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);

        //just in case timerText is null to avoid null reference exception
        if (timerText != null)
        {
            //update the timerText UI element with the formatted time string
            timerText.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
        }
    }

    //a public function to start the timer
    public void startTimer()
    {
        //set isTimerRunning to true
        isTimerRunning = true;
    }

    //a public function to pause the timer
    public void pauseTimer()
    {
        //set isTimerRunning to false
        isTimerRunning = false;
    }

    //a public function to reset the timer
    public void resetTimer()
    {
        //set isTimerRunning to false and elaspedTime to 0 and update the timer display
        elaspedTime = 0f;
        updateTimerDisplay();
    }

    //a public function to stop the timer this is different from pause as it also resets the time
    public void stopTimer()
    {
        isTimerRunning = false;
        elaspedTime = 0f;
        updateTimerDisplay();

    }
    public void CompleteGame()
    {
        gameComplete = true;
        pauseTimer();   // or pausetTimer() if that’s what your method is called

        // Show leaderboard data panel to get player name
        LeaderboardDataHandler handler = FindObjectOfType<LeaderboardDataHandler>();
        if (handler != null)
        {
            handler.ShowLeaderboardDataPanel(elaspedTime);   // use your timer variable name
        }
        else
        {
            // Fallback: save time without name if handler not found
            SaveCompletionTime(elaspedTime);
        }
    }

    void SaveCompletionTime(float time)
    {
        // Get existing times
        string timesKey = "LevelCompletionTimes";
        string countKey = "LevelCompletionCount";

        int count = PlayerPrefs.GetInt(countKey, 0);

        // Save this time (and empty name as fallback)
        PlayerPrefs.SetFloat(timesKey + "_" + count, time);
        PlayerPrefs.SetString("LevelCompletionNames_" + count, "");
        count++;
        PlayerPrefs.SetInt(countKey, count);

        PlayerPrefs.Save();
    }

    public bool IsGameComplete()
    {
        return gameComplete;
    }
}