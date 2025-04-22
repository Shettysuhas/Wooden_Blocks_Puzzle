using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UiHandler : MonoBehaviour
{
    [SerializeField]private Image difficultyImage;
    [SerializeField] private Slider difficultySlider;
    [SerializeField] private Sprite easySprite;
    [SerializeField] private Sprite mediumSprite;
    [SerializeField] private Sprite hardSprite;
    [SerializeField] private  TextMeshProUGUI difficultyLevel;
    
    public static int _difficultyLevel;
    private void Update()
    {
        SetDifficultyLevel();
    }
    public void SetDifficultyLevel()
    {
        if (difficultySlider.value == 0)
        {
            difficultyLevel.text = "Easy";
            _difficultyLevel = 0;
            difficultyImage.sprite = easySprite;
            
        }
        else if (difficultySlider.value == 1)
        {
            difficultyLevel.text = "Medium";
            _difficultyLevel = 1;
            difficultyImage.sprite = mediumSprite;
        }
        else
        {
            difficultyLevel.text = "Hard";
            _difficultyLevel = 2;
            difficultyImage.sprite = hardSprite;
        }
    }
    public void PlayButtonPressed()
    {
        SceneManager.LoadScene(1);
    }

    public void OnApplicationQuit()
    {
        Application.Quit();
    }
}
