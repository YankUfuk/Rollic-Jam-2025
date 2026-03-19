using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class UIPopupHandler : MonoBehaviour
{
    [Header("Popups")]
    [SerializeField] private GameObject levelCompletedPopup;
    [SerializeField] private GameObject levelFailedPopup;
    private void OnDisable()
    {
        if (EventManager.Instance == null) return;

        EventManager.Instance.Unregister<WinEvent>(OnLevelCompleted);
        EventManager.Instance.Unregister<TimesUpEvent>(OnLevelFailed);
    }

    private void Start()
    {
        EventManager.Instance.Register<WinEvent>(OnLevelCompleted);
        EventManager.Instance.Register<TimesUpEvent>(OnLevelFailed);

        // Disable all popups at start
        levelCompletedPopup.SetActive(false);
        levelFailedPopup.SetActive(false);
    }

    // -------------------- EVENT CALLBACKS --------------------

    private void OnLevelCompleted(WinEvent evt)
    {
        Debug.Log("UI: Level Completed Popup displayed.");
        levelCompletedPopup.SetActive(true);
    }

    private void OnLevelFailed(TimesUpEvent evt)
    {
        Debug.Log("UI: Level Failed Popup displayed.");
        levelFailedPopup.SetActive(true);
    }

    // -------------------- BUTTON FUNCTIONS --------------------

    public void RetryLevel()
    {
        int index = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(index);
    }

    public void LoadNextLevel()
    {
        int next = SceneManager.GetActiveScene().buildIndex + 1;

        // TODO: Add your game logic if last level
        if (next >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log("No next level found.");
            return;
        }

        SceneManager.LoadScene(next);
    }

    public void LoadPreviousLevel()
    {
        int prev = SceneManager.GetActiveScene().buildIndex - 1;

        if (prev < 0)
        {
            Debug.Log("No previous level found.");
            return;
        }

        SceneManager.LoadScene(prev);
    }
}
