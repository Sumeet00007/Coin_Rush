using Fusion;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private float gameDuration = 60f;
    private float returnToMenuDelay = 3f;
    private string mainMenuSceneName = "MainMenu";

    [Header("UI")]
    [SerializeField] private TMP_Text player1ScoreText;
    [SerializeField] private TMP_Text player2ScoreText;
    [SerializeField] private TMP_Text timerText;
    private bool uiInitialized;

    [Header("Win UI")]
    [SerializeField] private GameObject player1WinIMG;
    [SerializeField] private GameObject player2WinIMG;
    [SerializeField] private TMP_Text player1WinScoreText;
    [SerializeField] private TMP_Text player2WinScoreText;

    [Networked] public int Player1Score { get; private set; }
    [Networked] public int Player2Score { get; private set; }
    [Networked] private PlayerRef Player1Ref { get; set; }
    [Networked] private PlayerRef Player2Ref { get; set; }
    [Networked] private TickTimer GameTimer { get; set; }
    [Networked] private NetworkBool GameFinished { get; set; }
    [Networked] private int Winner { get; set; }

    private bool winUIShown;
    private bool returnToMenuStarted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        HideWinUI();
    }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            InitializeGame();
        }
        UpdateUI();
    }

    private void InitializeGame()
    {
        Player1Score = 0;
        Player2Score = 0;

        Winner = 0;
        GameFinished = false;

        winUIShown = false;
        returnToMenuStarted = false;

        int playerIndex = 0;

        foreach (PlayerRef player in Runner.ActivePlayers)
        {
            if (playerIndex == 0)
            {
                Player1Ref = player;
            }
            else if (playerIndex == 1)
            {
                Player2Ref = player;
            }

            playerIndex++;

            if (playerIndex >= 2)
                break;
        }

        if (playerIndex < 2)
        {
            //Debug.LogWarning("ScoreManager: Waiting for two players.");
            return;
        }

        GameTimer = TickTimer.CreateFromSeconds(Runner,gameDuration);

        //Debug.Log
        //(
        //    $"ScoreManager initialized. " +
        //    $"Player1={Player1Ref}, " +
        //    $"Player2={Player2Ref}, " +
        //    $"Duration={gameDuration}s"
        //);
    }


    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;
        if (!GameFinished && GameTimer.Expired(Runner))
        {
            FinishGame();
        }
    }

    public override void Render()
    {
        UpdateUI();
        if (GameFinished && !winUIShown)
        {
            ShowWinnerUI();
        }
    }

    public void AddCoin(PlayerRef collectingPlayer)
    {
        if (!HasStateAuthority)  return;
        if (GameFinished) return;

        if (GameTimer.Expired(Runner))
        {
            FinishGame();
            return;
        }

        if (collectingPlayer == Player1Ref)
        {
            Player1Score++;
        }

        else if (collectingPlayer == Player2Ref)
        {
            Player2Score++;
        }

        else
        {
            //Debug.LogWarning($"ScoreManager: Unknown PlayerRef {collectingPlayer}");
            return;
        }

        //Debug.Log(
        //    $"Coin collected by {collectingPlayer}. " +
        //    $"P1={Player1Score}, P2={Player2Score}"
        //);

        UpdateUI();
    }


    private void FinishGame()
    {
        if (!HasStateAuthority) return;

        if (GameFinished) return;

        if (Player1Score > Player2Score)
        {
            Winner = 1;
        }
        else if (Player2Score > Player1Score)
        {
            Winner = 2;
        }
        else
        {
            Winner = 0;
        }

        GameFinished = true;

        //Debug.Log(
        //    $"Game Finished. " +
        //    $"Player1={Player1Score}, " +
        //    $"Player2={Player2Score}"
        //);

        UpdateUI();
    }

    private void ShowWinnerUI()
    {
        if (winUIShown) return;
        winUIShown = true;
        HideWinUI();

        if (Winner == 1)
        {
            if (player1WinIMG != null)
            {
                player1WinIMG.SetActive(true);
            }

            if (player1WinScoreText != null)
            {
                player1WinScoreText.text = Player1Score.ToString();
            }

        }

        else if (Winner == 2)
        {
            if (player2WinIMG != null)
            {
                player2WinIMG.SetActive(true);
            }

            if (player2WinScoreText != null)
            {
                player2WinScoreText.text = Player2Score.ToString();
            }
        }

        else
        {
            //Debug.Log(
            //    $"GAME DRAW! " +
            //    $"Both players collected {Player1Score} coins."
            //);
        }

        if (!returnToMenuStarted)
        {
            returnToMenuStarted = true;
            Invoke(nameof(ReturnToMainMenu), returnToMenuDelay);
        }
    }

    private void HideWinUI()
    {
        if (player1WinIMG != null)
        {
            player1WinIMG.SetActive(false);
        }

        if (player2WinIMG != null)
        {
            player2WinIMG.SetActive(false);
        }
    }

    private void ReturnToMainMenu()
    {
       
        if (!returnToMenuStarted) return;
        SceneManager.LoadScene(mainMenuSceneName);
        // Then reload MainMenu to make sure it starts completely fresh
        SceneManager.sceneLoaded += ReloadMainMenu;
    }

    private void ReloadMainMenu(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != mainMenuSceneName)
            return;

        // Remove the callback so it doesn't remain registered
        SceneManager.sceneLoaded -= ReloadMainMenu;

        // Reload MainMenu again
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private float GetRemainingTime()
    {
        if (GameFinished)  return 0f;
        if (!GameTimer.IsRunning) return 0f;
        float remaining = GameTimer.RemainingTime(Runner) ?? 0f;
        return Mathf.Max(0f, remaining);
    }


    private void UpdateUI()
    {
        if (player1ScoreText != null)
        {
            player1ScoreText.text = Player1Score.ToString();
        }

        if (player2ScoreText != null)
        {
            player2ScoreText.text = Player2Score.ToString();
        }

        if (timerText != null)
        {
            float remaining = GetRemainingTime();
            int totalSeconds = Mathf.CeilToInt(remaining);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    public int GetPlayer1Score()
    {
        return Player1Score;
    }

    public int GetPlayer2Score()
    {
        return Player2Score;
    }

    public bool IsGameFinished()
    {
        return GameFinished;
    }

    public float GetTimeRemaining()
    {
        return GetRemainingTime();
    }

}