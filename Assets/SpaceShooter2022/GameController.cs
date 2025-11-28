using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
public class GameController : MonoBehaviour
{
    [SerializeField] private Image timerImage;
    [SerializeField] private float gameTime;
    private float sliderCurrentFillAmount = 1f;
    
    [Header("Score Components")]
    [SerializeField] private TextMeshProUGUI scoreText;
    
    [Header("Game Over Components")]
    [SerializeField] private GameObject gameOverScreen;

    [Header("High Score Components")]
    [SerializeField] private TextMeshProUGUI highscoreText;
    private int highScore;

    [Header("Gameplay audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] gameplayAudio;


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
        if(PlayerPrefs.HasKey("HighScore"))
        {
            highscoreText.text=PlayerPrefs.GetInt("HighScore").ToString();
        }
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
         PlayGameAudio(gameplayAudio[1], true);
    }
    
    public void GameOver()
    {
        currentGameStatus=GameState.GameOver;
        
        //show game over screen
        gameOverScreen.SetActive(true);

        if(playerScore>PlayerPrefs.GetInt("HighScore"))
        {
            PlayerPrefs.SetInt("HighScore",playerScore);
            highscoreText.text=playerScore.ToString();
        }

         PlayGameAudio(gameplayAudio[2], false);
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

        PlayGameAudio(gameplayAudio[0], true);
    }

    private void PlayGameAudio(AudioClip clipToPlay,bool shouldLoop)
    {
        audioSource.clip=clipToPlay;
        audioSource.loop=shouldLoop;
        audioSource.Play();
    }
    
}
