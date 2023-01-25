using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public GameObject PausePanel;
    public GameObject GameOverPanel;
    public GameObject GameOverReason;

    public enum GameState
    {
        Running,
        Paused,
        GameOver
    }

    public GameState State { get; private set; }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && State != GameState.GameOver)
        {
            if (State == GameState.Running)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }
    }

    public void GameOver(string gameOverReason)
    {  
        GameOverPanel.SetActive(true);
        GameOverReason.GetComponent<TextMeshProUGUI>().text = gameOverReason;
        State = GameState.GameOver;
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        PausePanel.SetActive(true);
        State = GameState.Paused;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        PausePanel.SetActive(false);
        State = GameState.Running;
    }
}
