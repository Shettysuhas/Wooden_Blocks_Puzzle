using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameUiHandler : MonoBehaviour
{
    [SerializeField] private GameObject gameOverpanel;
    [SerializeField] private GameObject pausepanel;
    [SerializeField] private GameObject scoreOverpanel;
    [SerializeField] private TextMeshProUGUI gameOverScore;
    [SerializeField] private TextMeshProUGUI gameOverHighestScore;
    
    public void GameOver()
    {
        gameOverpanel.SetActive(true);
        scoreOverpanel.SetActive(false);
        gameOverScore.text = ScoreMnager.Instance.scores[ScoreMnager.Instance.currentLevel].ToString();
        gameOverHighestScore.text = ScoreMnager.Instance.highScores[ScoreMnager.Instance.currentLevel].ToString();
        
    }

    public void SceneChanger(int i)
    {
        SceneManager.LoadScene(i);
       
    }
    public void OnApplicationQuit()
    {
        Application.Quit();
    }
    public void Pause()
    {
        pausepanel.SetActive(true);
        scoreOverpanel.SetActive(false);
    }
    public void Resume()
    {
        pausepanel.SetActive(false);
        scoreOverpanel.SetActive(true);
    }
}
