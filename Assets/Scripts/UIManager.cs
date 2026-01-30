using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Score UI")]
    [SerializeField] private TextMeshProUGUI[] scoreEntries;

    [Header("Countdown")]
    [SerializeField] private GameObject countdownRoot;
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Game Timer")]
    [SerializeField] private TextMeshProUGUI gameTimerText;

    [Header("Start / Restart Popup")]
    [SerializeField] private GameObject startPopup;
    
    [Header("In game UI parent")]
    [SerializeField] private GameObject gameUIParent;

    [Header("Highscore")]
    [SerializeField] private TextMeshProUGUI[] highscoreEntries;
    [SerializeField] private GameObject highscoreUIParent;
    
    public Action OnTimerFinished;
    public Action<int> OnPressStartButton;
    public Action OnCountdownFinished;
    
    private float remainingTime;
    private Coroutine timerRoutine;
    
    private void ShowStartPopup()
    {
        startPopup.SetActive(true);
    }

    public void StartGameWithPlayers(int playerCount)
    {
        startPopup.SetActive(false);
        OnPressStartButton?.Invoke(playerCount);
    }

    public void ShowCountdown()
    {
        StartCoroutine(StartSequence());
    }

    private IEnumerator StartSequence()
    {
        yield return StartCoroutine(Countdown());
        OnCountdownFinished?.Invoke();
    }
    
    private IEnumerator Countdown()
    {
        countdownRoot.SetActive(true);

        for (var i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        countdownText.text = "GO!";
        yield return new WaitForSeconds(0.5f);

        countdownRoot.SetActive(false);
    }
    
    public void StartTimer(float gameDuration)
    {
        gameUIParent.SetActive(true);
        remainingTime = gameDuration;

        if (timerRoutine != null)
            StopCoroutine(timerRoutine);

        timerRoutine = StartCoroutine(GameTimer());
    }

    private IEnumerator GameTimer()
    {
        while (remainingTime > 0f)
        {
            remainingTime -= Time.deltaTime;
            UpdateTimerUI(remainingTime);
            yield return null;
        }
        UpdateTimerUI(0);
        OnTimerFinished?.Invoke();
    }

    private void UpdateTimerUI(float time)
    {
        var seconds = Mathf.CeilToInt(time);
        gameTimerText.text = seconds.ToString();
    }

    public void SetPlayerColor(CarColor playerCarColor, int playerNumber)
    {
        scoreEntries[playerNumber].color = GetColorFromCarColor(playerCarColor);
    }

    private Color GetColorFromCarColor(CarColor playerCarColor)
    {
        switch (playerCarColor)
        {
            case CarColor.Blue: return Color.blue;
            case CarColor.Orange: return new Color(1f, 0.5f, 0);
            case CarColor.Yellow: return new Color(1f, 1f, 0);
            case CarColor.Purple: return new Color(1f, 0, 1f);
            default: return Color.white;
        }
    }

    public void SetScoreUI(int numberOfPlayers)
    {
        for (var i = 0; i < scoreEntries.Length; i++)
        {
            scoreEntries[i].enabled = i < numberOfPlayers;
        }
    }

    public void UpdateScore(CarColor carColor, int playerScore)
    {
        foreach (var scoreEntry in scoreEntries)
        {
            if (scoreEntry.color == GetColorFromCarColor(carColor))
            {
                scoreEntry.text = scoreEntry.text.Substring(0,4) + playerScore;
                return;
            }
        }
    }

    public void ShowHighscoreUI()
    {
        var scores = scoreEntries.OrderByDescending(entry => int.Parse(entry.text.Substring(4))).ToArray();

        for (var i = 0; i < highscoreEntries.Length; i++)
        {
            highscoreEntries[i].text = scores[i].text;
            highscoreEntries[i].color = scores[i].color;
            highscoreEntries[i].enabled = scores[i].enabled;
        }
        
        highscoreUIParent.SetActive(true);
    }

    public void HighscoreContinuePressed()
    {
        highscoreUIParent.SetActive(false);
        ShowStartPopup();
    }

    public void ResetScores()
    {
        for (var i = 0; i < scoreEntries.Length; i++)
        {
            scoreEntries[i].text = "P" + (i+1) + ": 0000";
        }
    }
}
