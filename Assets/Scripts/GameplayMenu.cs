using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GameplayMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject menuPanel;
    [Header("Buttons")]
    public Button restartButton;
    public Button homeButton;

    private void Start()
    {
        //call the setup button listeners function
        setupButtonListeners();
        //hide menu panel at the start of the gameplay scene
        if (menuPanel != null)
            menuPanel.SetActive(false);

    }
    //this function listens for restart and home buttons
    public void setupButtonListeners()
    {
        //restart button listener and home button listener
        if (restartButton != null)
            restartButton.onClick.AddListener(onRestartButtonClicked);
        if (homeButton != null)
            homeButton.onClick.AddListener(onHomeButtonClicked);
    }
    //this function restart the current scene
    public void onRestartButtonClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    //home button clicked to load menu scene
    public void onHomeButtonClicked()
    {
       SceneManager.LoadScene("Menu");
    }

    //this function show menu panel when game ends
    public void showMenuPanel()
    {
        if (menuPanel != null)
            menuPanel.SetActive(true);
    }

    //avoid memory leaks using unity's builtin OnDestroy function
    void OnDestroy()
    {
        if (restartButton != null)
            restartButton.onClick.RemoveListener(onRestartButtonClicked);
        if (homeButton != null)
            homeButton.onClick.RemoveListener(onHomeButtonClicked);
    }
}
