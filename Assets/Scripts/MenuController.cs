using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject menuPanel;
    public GameObject leaderboardPanel;
    public GameObject settingsPanel;

    [Header("Menu Buttons")]
    public Button playButton;
    public Button leaderboardButton;
    public Button settingsButton;
    public Button exitButton;

    [Header("Back Buttons")]
    public Button settingsBackButton;
    public Button leaderboardBackButton;
    //start is called before the first frame update
    void Start()
    {
        //setup button listeners
        setupButtonListeners();
        // Show menu panel on default
        showMenuPanel();
    }
    //function to setup button listeners
    void setupButtonListeners()
    {
        //check if all these buttons are not null before adding listeners
        if (leaderboardButton != null)
            leaderboardButton.onClick.AddListener(onLeaderboardClicked);
        if(playButton != null)
            playButton.onClick.AddListener(onPlayButtonClicked);
        if(settingsButton != null)
            settingsButton.onClick.AddListener(onSettingButtonClicked);
        if(exitButton != null)
            exitButton.onClick.AddListener(onExitButtonClicked);

        //check if back buttons is not null before adding listener
        if (settingsBackButton != null)
            settingsBackButton.onClick.AddListener(onBackButtonClicked);
        if(leaderboardBackButton != null)
            leaderboardBackButton.onClick.AddListener(onBackButtonClicked);
    }

    //function for if the user click leaderboard back button
    public void onLeaderboardClicked()
    {
        menuPanel.SetActive(false);
        leaderboardPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    public void onPlayButtonClicked()
    {
        SceneManager.LoadScene("Gameplay");
    }

    //function for if use clicked settings button
    public void onSettingButtonClicked()
    {
        menuPanel.SetActive(false);
        leaderboardPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }
    //function is for exit button clicked
    public void onExitButtonClicked()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
    //function for back button clicked
    public void onBackButtonClicked()
    {
        showMenuPanel();
    }

    //function to show menu panel and hide others
    private void showMenuPanel()
    {
        menuPanel.SetActive(true);
        leaderboardPanel.SetActive(false);
        settingsPanel.SetActive(false); 
    }
    //avoid memory leaks using unity's builtin OnDestroy function
    void OnDestroy()
    {
        //remove listeners to avoid memory leaks
        if(leaderboardButton != null)
            leaderboardButton.onClick.RemoveListener(onLeaderboardClicked);
        if(playButton != null)
            playButton.onClick.RemoveListener(onPlayButtonClicked);
        if(settingsButton != null)
            settingsButton.onClick.RemoveListener(onSettingButtonClicked);
        if(exitButton != null)
            exitButton.onClick.RemoveListener(onExitButtonClicked);
        if(settingsBackButton != null)
            settingsBackButton.onClick.RemoveListener(onBackButtonClicked);
        if(leaderboardBackButton != null)
            leaderboardBackButton.onClick.RemoveListener(onBackButtonClicked);
    }
}
