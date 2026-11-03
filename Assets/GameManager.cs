using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Handles the start screen, the end screen, and lives.
// AlienFleet creates this automatically, so nothing has to be set up in the editor.
public class GameManager : MonoBehaviour
{
    public enum State { Title, Playing, Won, Lost }

    public static GameManager Instance { get; private set; }

    public int startingLives = 3;
    public float deathScreenDelay = 1.2f;   // time for the ship's explosion before GAME OVER

    public State CurrentState { get; private set; } = State.Title;
    public bool IsPlaying => CurrentState == State.Playing;
    public int Lives { get; private set; }

    static bool skipTitle = false;   // set when restarting so we go straight into the game
    bool ending = false;             // the game is over and the end screen is about to appear

    public static GameManager EnsureExists()
    {
        if (Instance == null)
        {
            new GameObject("GameManager").AddComponent<GameManager>();
        }
        return Instance;
    }

    void Awake()
    {
        Instance = this;
        Lives = startingLives;

        if (skipTitle)
        {
            skipTitle = false;
            CurrentState = State.Playing;
            Time.timeScale = 1f;
        }
        else
        {
            CurrentState = State.Title;
            Time.timeScale = 0f;   // freeze the game behind the start screen
        }
    }

    void Start()
    {
        Starfield.Create();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        bool enterPressed = keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame;
        if (!enterPressed) return;

        if (CurrentState == State.Title)
        {
            CurrentState = State.Playing;
            Time.timeScale = 1f;
        }
        else if (CurrentState == State.Won || CurrentState == State.Lost)
        {
            skipTitle = true;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void LoseLife()
    {
        if (!IsPlaying) return;
        Lives--;
        if (Lives <= 0)
        {
            EndGame(false, deathScreenDelay);
        }
    }

    // Ends the game. With a delay, play continues briefly so the final explosion can be seen.
    public void EndGame(bool won, float delay = 0f)
    {
        if (!IsPlaying || ending) return;
        ending = true;
        if (delay > 0f)
        {
            StartCoroutine(EndAfter(won, delay));
        }
        else
        {
            ShowEndScreen(won);
        }
    }

    IEnumerator EndAfter(bool won, float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowEndScreen(won);
    }

    void ShowEndScreen(bool won)
    {
        CurrentState = won ? State.Won : State.Lost;
        Time.timeScale = 0f;
    }

    void OnGUI()
    {
        float w = Screen.width;
        float h = Screen.height;
        float px = Mathf.Max(2f, h / 160f);   // size of one font pixel

        // Lives display: one heart per life in the top right; lost lives turn dark.
        float heartPx = Mathf.Floor(px * 1.5f);
        float heartWidth = PixelFont.HeartWidth * heartPx;
        float gap = heartPx * 2f;
        float x = w - 20f - startingLives * heartWidth - (startingLives - 1) * gap;
        for (int i = 0; i < startingLives; i++)
        {
            Color heartColor = i < Lives ? new Color(1f, 0.15f, 0.25f) : new Color(0.3f, 0.3f, 0.35f);
            PixelFont.DrawHeart(x, 14f, heartPx, heartColor);
            x += heartWidth + gap;
        }

        if (CurrentState == State.Playing) return;

        // Dark overlay for the start and end screens
        GUI.color = new Color(0f, 0f, 0f, 0.75f);
        GUI.DrawTexture(new Rect(0, 0, w, h), Texture2D.whiteTexture);
        GUI.color = Color.white;

        string title;
        string message;
        Color titleColor = new Color(0.4f, 1f, 0.4f);
        Color messageColor = Color.white;
        string prompt = "PRESS ENTER TO PLAY AGAIN";
        switch (CurrentState)
        {
            case State.Title:
                title = "SPACE INVADERS";
                message = "ARROWS OR A/D TO MOVE\nSPACE TO SHOOT";
                prompt = "PRESS ENTER TO START";
                break;
            case State.Won:
                title = "YOU WIN!";
                message = "EVERY ALIEN DESTROYED";
                messageColor = new Color(1f, 0.9f, 0.3f);
                break;
            default:
                title = "GAME OVER";
                message = "YOU HAVE BEEN ELIMINATED";
                titleColor = new Color(1f, 0.25f, 0.2f);
                messageColor = new Color(1f, 0.55f, 0.45f);
                break;
        }

        Rect area = new Rect(w * 0.05f, 0, w * 0.9f, h);
        area.y = h * 0.22f;
        PixelFont.Draw(title, area, px * 3f, titleColor);
        area.y = h * 0.5f;
        PixelFont.Draw(message, area, px * 1.6f, messageColor);
        area.y = h * 0.72f;
        PixelFont.Draw(prompt, area, px, Color.white);

        // Credits (shown on the start screen and both end screens)
        area.y = h * 0.81f;
        PixelFont.Draw("DEVELOPED WITH AI-ASSISTED PROGRAMMING SUPPORT\nFROM CLAUDE BY ANTHROPIC",
                       area, Mathf.Max(2f, px * 0.8f), new Color(0.75f, 0.75f, 0.8f));
    }
}
