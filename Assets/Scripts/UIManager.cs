using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager _Instance { get; private set; }
    private void Awake()
    {
        _Instance = this;
    }

    private void Start()
    {
        // 
        LoadSettings();
    }

    [SerializeField] private string segmentsKey = "Segments";
    [SerializeField] private string durationKey = "Duration";

    [SerializeField] private TextMeshProUGUI difficultyText;
    [SerializeField] private TextMeshProUGUI currentScoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;

    [SerializeField] private IntStore segments;
    [SerializeField] private IntStore coins;

    [Header("Control Scheme")]
    private ControlScheme controlScheme = ControlScheme.SWIPE;
    public bool UseSwipe => controlScheme == ControlScheme.SWIPE;
    public bool UseButtons => controlScheme == ControlScheme.BUTTONS;
    [SerializeField] private SerializableDictionary<ControlScheme, Sprite> controlSchemeSpriteDict = new SerializableDictionary<ControlScheme, Sprite>();
    [SerializeField] private Image controlSchemeIcon;
    [SerializeField] private GameObject changeDirectionsButtonsContainer;
    private string controlSchemeKey = "controlScheme";

    [Header("Lose Screen")]
    [SerializeField] private GameObject[] enableOnOpenLoseScreen;
    [SerializeField] private GameObject[] disableOnOpenLoseScreen;

    [Header("Selection Screen")]
    [SerializeField] private GameObject[] enableOnOpenSelectionScreen;
    [SerializeField] private GameObject[] disableOnOpenSelectionScreen;
    [SerializeField] private PercentageMap<SelectionCard> selectionPrefabs = new PercentageMap<SelectionCard>();
    [SerializeField] private Transform selectionHolder;
    [SerializeField] private int numSelections;
    private List<SelectionCard> spawnedSelections = new List<SelectionCard>();
    [SerializeField] private int rerollCost;
    [SerializeField] private float rerollCostMultiplyBy;
    [SerializeField] private TextMeshProUGUI rerollCostText;
    [SerializeField] private SerializableDictionary<CardType, CardTypeDetails> cardTypeDetailsDictionary = new SerializableDictionary<CardType, CardTypeDetails>();
    public SerializableDictionary<CardType, CardTypeDetails> CardTypeDetailsDictionary { get { return cardTypeDetailsDictionary; } }

    [SerializeField] private CallNextSelectionOnBarFill barFill;

    [SerializeField] private float restartTime = 1f;
    [SerializeField] private FloatStore restartTimer;

    [SerializeField] private float selectionPopupGracePeriod = 1f;
    private float selectionPopupGracePeriodTimer;
    public bool InSelectionPopGracePeriod => selectionPopupGracePeriodTimer > 0;

    [Header("Audio")]
    [SerializeField] private AudioClipContainer onOpenCardSelectionScreen;
    [SerializeField] private AudioClipContainer onCloseCardSelectionScreen;
    [SerializeField] private AudioClipContainer onOpenEndGameScreen;
    [SerializeField] private AudioClipContainer onTellSnakeMove;
    [SerializeField] private AudioClipContainer onSnakeStartMove;
    [SerializeField] private AudioClipContainer onNewHighScore;
    [SerializeField] private AudioClipContainer onRerollCardSelections;
    [SerializeField] private AudioClipContainer onFailSelectCard;
    public AudioClipContainer OnFailSelectCard => onFailSelectCard;

    [SerializeField] private bool sfxVolumeEnabled = true;
    [SerializeField] private bool musicVolumeEnabled = true;
    [SerializeField] private float sfxVolumeDefault = .7f;
    [SerializeField] private float musicVolumeDefault = .7f;
    [SerializeField] private float sfxVolumeMin = .0001f;
    [SerializeField] private float musicVolumeMin = .0001f;
    private string sfxVolumeKey = "sfxVolume";
    private string musicVolumeKey = "musicVolume";
    [SerializeField] private GameObject sfxVolumeCross;
    [SerializeField] private GameObject musicVolumeCross;
    [SerializeField] private TextMeshProUGUI controlsText;
    [SerializeField] private TextMeshProUGUI sfxText;
    [SerializeField] private TextMeshProUGUI musicText;
    [SerializeField] private AudioMixer mixer;

    // if player has more segments, high score
    // if player has same segments, less duration, high score
    // if player has same segments, more duration, no high score
    // if player has less segments, no high score

    private void Update()
    {
        if (selectionPopupGracePeriodTimer > 0)
        {
            selectionPopupGracePeriodTimer -= Time.unscaledDeltaTime;
        }
    }

    public string GetDifficultyString(Difficulty d)
    {
        switch (d)
        {
            case Difficulty.SLOW:
                return "Slow";
            case Difficulty.REGULAR:
                return "Regular";
            case Difficulty.FAST:
                return "Fast";
            case Difficulty.EXTRA_FAST:
                return "Extra Fast";
            case Difficulty.IMPOSSIBLE:
                return "Impossible";
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    [ContextMenu("ClearHighScores")]
    private void ClearHighScores()
    {
        PlayerPrefs.DeleteAll();
    }

    public void ToggleControlScheme()
    {
        switch (controlScheme)
        {
            case ControlScheme.BUTTONS:
                controlScheme = ControlScheme.SWIPE;
                break;
            case ControlScheme.SWIPE:
                controlScheme = ControlScheme.BUTTONS;
                break;
        }
        SetControlScheme(controlScheme);
    }

    private void SetControlScheme(ControlScheme scheme)
    {
        controlScheme = scheme;
        switch (controlScheme)
        {
            case ControlScheme.BUTTONS:
                changeDirectionsButtonsContainer.SetActive(true);
                break;
            case ControlScheme.SWIPE:
                changeDirectionsButtonsContainer.SetActive(false);
                break;
        }
        controlSchemeIcon.sprite = controlSchemeSpriteDict[controlScheme];
        PlayerPrefs.SetInt(controlSchemeKey, (int)scheme);

        string before = controlScheme.ToString();
        string after = char.ToUpper(before.First()) + before.Substring(1).ToLower();
        controlsText.text = "Controls:\n" + after;
    }

    public void ToggleMusic()
    {
        musicVolumeEnabled = !musicVolumeEnabled;
        musicVolumeCross.SetActive(!musicVolumeEnabled);
        musicText.text = "Music\n" + (musicVolumeEnabled ? "On" : "Off");
        SetMusicVolume(musicVolumeEnabled ? musicVolumeDefault : musicVolumeMin);
    }

    public void ToggleSFX()
    {
        sfxVolumeEnabled = !sfxVolumeEnabled;
        sfxVolumeCross.SetActive(!sfxVolumeEnabled);
        sfxText.text = "SFX\n" + (sfxVolumeEnabled ? "On" : "Off");
        SetSFXVolume(sfxVolumeEnabled ? sfxVolumeDefault : sfxVolumeMin);
    }

    public void SetSFXVolume(float percent)
    {
        PlayerPrefs.SetFloat(sfxVolumeKey, percent);
        mixer.SetFloat("SFXVolume", Mathf.Log10(percent) * 20);
    }

    public void SetMusicVolume(float percent)
    {
        PlayerPrefs.SetFloat(musicVolumeKey, percent);
        mixer.SetFloat("MusicVolume", Mathf.Log10(percent) * 20);
    }

    public void LoadSettings()
    {
        if (PlayerPrefs.HasKey(musicVolumeKey))
        {
            float musicVolume = PlayerPrefs.GetFloat(musicVolumeKey);
            musicVolumeEnabled = musicVolume > musicVolumeMin;
            musicVolumeCross.SetActive(!musicVolumeEnabled);
            musicText.text = "Music\n" + (musicVolumeEnabled ? "On" : "Off");
            SetMusicVolume(musicVolume);
        }

        if (PlayerPrefs.HasKey(sfxVolumeKey))
        {
            float sfxVolume = PlayerPrefs.GetFloat(sfxVolumeKey);
            sfxVolumeEnabled = sfxVolume > sfxVolumeMin;
            sfxVolumeCross.SetActive(!sfxVolumeEnabled);
            sfxText.text = "SFX\n" + (sfxVolumeEnabled ? "On" : "Off");
            SetSFXVolume(sfxVolume);
        }

        if (PlayerPrefs.HasKey(controlSchemeKey))
        {
            int storedScheme = PlayerPrefs.GetInt(controlSchemeKey);
            // Debug.Log((ControlScheme)storedScheme);
            SetControlScheme((ControlScheme)storedScheme);
        }
        else
        {
            PlayerPrefs.SetInt(controlSchemeKey, (int)controlScheme);
        }
    }

    public void SetHighScore()
    {
        string difficulty = GetDifficultyString(GridGenerator._Instance.Difficulty);
        string fullSegmentsKey = difficulty.Replace(" ", "") + segmentsKey;
        string fullDurationKey = difficulty.Replace(" ", "") + durationKey;
        difficultyText.text = difficulty;

        float duration = (float)System.Math.Round(GridGenerator._Instance.GameDuration, 1);
        if (PlayerPrefs.HasKey(fullSegmentsKey))
        {
            int hsSegments = PlayerPrefs.GetInt(fullSegmentsKey);
            float hsDuration = PlayerPrefs.GetFloat(fullDurationKey);

            currentScoreText.text = "Size = " + segments.Value + " | " + duration + "s";

            // Saving High Score
            if (segments.Value > hsSegments)
            {
                PlayerPrefs.SetInt(fullSegmentsKey, segments.Value);
                PlayerPrefs.SetFloat(fullDurationKey, duration);
                highScoreText.text = "New High Score!: " + currentScoreText.text;
                onNewHighScore.PlayOneShot();
            }
            else if (segments.Value == hsSegments && duration < hsDuration)
            {
                PlayerPrefs.SetInt(fullSegmentsKey, segments.Value);
                PlayerPrefs.SetFloat(fullDurationKey, duration);
                highScoreText.text = "New High Score!: " + currentScoreText.text;
                onNewHighScore.PlayOneShot();
            }
            else
            {
                highScoreText.text = "High Score: Size = " + hsSegments + " | " + hsDuration + "s";
            }
        }
        else
        {
            PlayerPrefs.SetInt(fullSegmentsKey, segments.Value);
            PlayerPrefs.SetFloat(fullDurationKey, duration);

            currentScoreText.text = "Size = " + segments.Value + " | " + duration + "s";
            highScoreText.text = "New High Score!: " + currentScoreText.text;
            onNewHighScore.PlayOneShot();
        }
        PlayerPrefs.Save();
    }

    public void SetScores()
    {
        SetHighScore();
    }

    public void OpenLoseScreen()
    {
        // Set scores
        SetScores();

        foreach (GameObject obj in disableOnOpenLoseScreen)
        {
            obj.SetActive(false);
        }
        foreach (GameObject obj in enableOnOpenLoseScreen)
        {
            obj.SetActive(true);
        }

        onOpenEndGameScreen.PlayOneShot();

        GridGenerator._Instance.Pause();
    }

    public void RerollSelections()
    {
        if (coins.Value < rerollCost) return;

        // Reset Selections
        ClearSelections();
        SetSelections();

        // Change Reroll Cost
        coins.Value -= rerollCost;
        rerollCost = Mathf.CeilToInt(rerollCost * rerollCostMultiplyBy);
        rerollCostText.text = rerollCost.ToString();

        onRerollCardSelections.PlayOneShot();
    }

    [ContextMenu("CloseSelectionScreen")]
    public void CloseSelectionScreen()
    {
        ClearSelections();
        onCloseCardSelectionScreen.PlayOneShot();

        foreach (GameObject obj in disableOnOpenSelectionScreen)
        {
            obj.SetActive(true);
        }
        foreach (GameObject obj in enableOnOpenSelectionScreen)
        {
            obj.SetActive(false);
        }
        GridGenerator._Instance.Resume();
        barFill.AddOnFullActions();
        StartCoroutine(StartSnake());
    }

    public IEnumerator StartSnake()
    {
        restartTimer.Value = restartTime;

        onTellSnakeMove.PlayOneShot();

        while (restartTimer.Value > 0)
        {
            restartTimer.Value -= Time.unscaledDeltaTime;
            yield return null;
        }

        onSnakeStartMove.PlayOneShot();

        // Re-enable Snake
        SnakeBehaviour._Instance.StartMoving();
    }

    public IEnumerator StartSnake(float delay)
    {
        restartTimer.Value = delay;

        while (restartTimer.Value > 0)
        {
            restartTimer.Value -= Time.unscaledDeltaTime;
            yield return null;
        }

        // Re-enable Snake
        SnakeBehaviour._Instance.StartMoving();
    }

    [ContextMenu("OpenSelectionScreen")]
    public void OpenSelectionScreen()
    {
        // Set Reroll cost text
        rerollCostText.text = rerollCost.ToString();

        // Disable Snake
        SnakeBehaviour._Instance.StopMoving();

        onOpenCardSelectionScreen.PlayOneShot();

        selectionPopupGracePeriodTimer = selectionPopupGracePeriod;

        SetSelections();
        foreach (GameObject obj in disableOnOpenSelectionScreen)
        {
            obj.SetActive(false);
        }
        foreach (GameObject obj in enableOnOpenSelectionScreen)
        {
            obj.SetActive(true);
        }
        GridGenerator._Instance.Pause();
    }

    private void ClearSelections()
    {
        while (spawnedSelections.Count > 0)
        {
            Destroy(spawnedSelections[0].gameObject);
            spawnedSelections.RemoveAt(0);
        }
    }

    private void SetSelections()
    {
        for (int i = 0; i < numSelections; i++)
        {
            SelectionCard spawned = Instantiate(selectionPrefabs.GetOption(), selectionHolder);
            spawnedSelections.Add(spawned);
            spawned.AddOnSelectAction(() => CloseSelectionScreen());
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
