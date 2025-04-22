using System;
using TMPro;
using UnityEngine;

public class ScoreMnager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;

    public static ScoreMnager Instance;
    public int[] scores = new int[3];
    public int[] highScores = new int[3];
    public int currentLevel;
    private void Start()
    {
        Instance = this;
        currentLevel = UiHandler._difficultyLevel;
        LoadScores();
        UpdateScoreUI();
       
    }
    public void AddScore(int scoreToAdd)
    {
        scores[currentLevel] += scoreToAdd;
        if (scores[currentLevel] > highScores[currentLevel])
        {
            highScores[currentLevel] = scores[currentLevel];
            PlayerPrefs.SetInt($"HighScore_{currentLevel}", highScores[currentLevel]);
            PlayerPrefs.Save();
        }
        UpdateScoreUI();
    }
    
    private void UpdateScoreUI()
    {
        scoreText.text = scores[currentLevel].ToString();
        highScoreText.text = highScores[currentLevel].ToString();
    }
    private void LoadScores()
    {
        for (int i = 0; i < 3; i++)
        {
            highScores[i] = PlayerPrefs.GetInt($"HighScore_{i}", 0);
        }
    }
}
