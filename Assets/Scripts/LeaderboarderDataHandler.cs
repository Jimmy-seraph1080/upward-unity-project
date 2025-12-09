using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardDataHandler : MonoBehaviour
{
    [Header("Panels")]
    //reference to the panel where the player enters their name for the leaderboard
    public GameObject leaderboardDataPanel;
    //reference to the end screen panel shown after submitting data
    public GameObject endScreenPanel;

    [Header("Input")]
    //input field where the player types their name
    public TMP_InputField nameInputField;

    [Header("Buttons")]
    //button pressed when the player is done entering their name
    public Button doneButton;

    [Header("Game Mechanics Reference")]
    //reference to the GameMechanics script (if needed for other features)
    public GameMechanics gameMechanics;

    //stores the current run's completion time
    private float currentTime = 0f;
    //stores the index of the current time entry in PlayerPrefs
    private int currentTimeIndex = -1;

    void Start()
    {
        //if the doneButton is not null, add a click listener
        if (doneButton != null)
        {
            //attach OnDoneButtonClicked to the doneButton's onClick event
            doneButton.onClick.AddListener(OnDoneButtonClicked);
        }

        //if the leaderboardDataPanel is not null then hide leaderboardDataPanel at the start of the game
        if (leaderboardDataPanel != null)
        {
            //disable leaderboardDataPanel
            leaderboardDataPanel.SetActive(false);
        }

        //if the endSceenPanel is not null then hide endScreenPanel at the start of the game
        if (endScreenPanel != null)
        {
            //disable endScreenPanel
            endScreenPanel.SetActive(false);
        }
    }

    //called when the game is completed to show the leaderboard data panel
    public void ShowLeaderboardDataPanel(float completionTime)
    {
        //store the passed completion time in currentTime
        currentTime = completionTime;

        //get the current number of saved level completion entries
        int count = PlayerPrefs.GetInt("Level Completion Count", 0);
        //save this run's completion time using the current count as index
        PlayerPrefs.SetFloat("Level Completion Times" + count, currentTime);
        //save an empty name as a placeholder to be updated later
        PlayerPrefs.SetString("Level Completion Names" + count, "");
        //store the index used for this entry in currentTimeIndex
        currentTimeIndex = count;
        //increment the total count of level completion entries
        count++;
        //update LevelCompletionCount in PlayerPrefs with the incremented value
        PlayerPrefs.SetInt("Level Completion Count", count);
        //commit all PlayerPrefs changes to disk
        PlayerPrefs.Save();

        //if the leaderboardDataPanel is not null then, show the leaderboardDataPanel so player can enter their name
        if (leaderboardDataPanel != null)
        {
            //enable leaderboardDataPanel
            leaderboardDataPanel.SetActive(true);
        }

        //if the nameInputField is not null then prepare the name input field for user entry
        if (nameInputField != null)
        {
            //clear any previous text in the input field
            nameInputField.text = "";
            //select the input field so it becomes focused
            nameInputField.Select();
            //activate the input field so the keyboard/cursor is ready
            nameInputField.ActivateInputField();
        }
    }

    public void OnDoneButtonClicked()
    {
        //empty playerName string
        string playerName = "";
        //if nameInputField exists, get the text the player typed
        if (nameInputField != null)
        {
            //assign the entered text to playerName
            playerName = nameInputField.text;
        }
        //if playerName is empty or null, use Player as default
        if (string.IsNullOrEmpty(playerName))
        {
            //set default name when no input is provided
            playerName = "Player";
        }

        //if currentTimeIndex greater than 0, update the stored name for this entry
        if (currentTimeIndex >= 0)
        {
            //set the string for player's name for the current run index in PlayerPrefs
            PlayerPrefs.SetString("LevelCompletionNames" + currentTimeIndex, playerName);
            //write and save PlayerPrefs changes
            PlayerPrefs.Save();
        }

        //if FirebaseManager instance is not null, upload the entry to the online leaderboard
        if (FirebaseManager.Instance != null)
        {
            //call SaveLeaderboardEntry on FirebaseManager to upload name and time
            FirebaseManager.Instance.saveLeaderboardEntry(playerName, currentTime, success =>
            {
                //if upload was not successful, log a warning in the console
                if (!success)
                    Debug.LogWarning("Failed to upload leaderboard entry to Firebase.");
            });
        }
        else
        {
            //log warning if FirebaseManager is missing to indicate no online upload
            Debug.LogWarning("FirebaseManager not found, skipping online upload");
        }

        //if the leaderboardDataPanel is not null then, hide the leaderboardDataPanel after finishing name input
        if (leaderboardDataPanel != null)
        {
            //disable leaderboardDataPanel
            leaderboardDataPanel.SetActive(false);
        }

        //if the endScreenPanel is not null then, show the endScreenPanel to display the end-of-run screen
        if (endScreenPanel != null)
        {
            //enable endScreenPanel
            endScreenPanel.SetActive(true);
        }
    }

    void OnDestroy()
    {
        //if doneButton is null, remove the click listener when object is destroyed
        if (doneButton != null)
        {
            //remove OnDoneButtonClicked from doneButton's onClick event
            doneButton.onClick.RemoveListener(OnDoneButtonClicked);
        }
    }
}