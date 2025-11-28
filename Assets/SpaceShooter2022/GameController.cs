using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameController : MonoBehaviour
{
    [SerializeField] private Image timerImage;
    [SerializeField] private float gameTime;
    private float sliderCurrentFillAmount = 1f;
    
    [Header("Score Components")]
    [SerializeField] private TextMeshProUGUI scoreText;
    
    [Header("Game Over Components")]
    [SerializeField] private GameObject gameOverScreen;

    private int playerScore;

    public enum GameState
    {
        Waiting,
        Playing,
        GameOver
    }

    public static GameState currentGameStatus;

    private void Awake()
    {
        currentGameStatus = GameState.Waiting;
    }
    
    private void Update()
    {
        if (currentGameStatus == GameState.Playing)
              AdjustTimer();
        
    }

    private void AdjustTimer()
    {
        timerImage.fillAmount = sliderCurrentFillAmount - (Time.deltaTime / gameTime);
        
        sliderCurrentFillAmount = timerImage.fillAmount;
        
        if(sliderCurrentFillAmount <=0f)
        {
            GameOver();
        }
    }

    public void UpdatePlayerScore(int asteroidHitPoints)
    {
        if(currentGameStatus != GameState.Playing)
            return;
        
        playerScore += asteroidHitPoints;
        scoreText.text = playerScore.ToString();
    }
    
    public void StartGame()
    {
        currentGameStatus = GameState.Playing;
    }
    
    public void GameOver()
    {
        currentGameStatus=GameState.GameOver;
        
        //show game over screen
        gameOverScreen.SetActive(true);
    }
    
    public void ResetGame()
    {
        currentGameStatus = GameState.Waiting;
        //put time to 1
        sliderCurrentFillAmount = 1f;
        timerImage.fillAmount = 1f;
        
        //reset the score
        playerScore = 0;
        scoreText.text = "0";
    }
    
}
