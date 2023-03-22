using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager _Instance { get; private set; }
    private void Awake()
    {
        _Instance = this;
    }

    [SerializeField] private string segmentsKey = "Segments";
    [SerializeField] private string durationKey = "Duration";

    [SerializeField] private TextMeshProUGUI currentScoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;

    [SerializeField] private IntStore segments;
    [SerializeField] private IntStore coins;

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
    [SerializeField] private SerializableDictionary<CardType, Color> cardTypeColorDictionary = new SerializableDictionary<CardType, Color>();
    public SerializableDictionary<CardType, Color> CardTypeColorDictionary { get { return cardTypeColorDictionary; } }

    [SerializeField] private CallNextSelectionOnBarFill barFill;

    [SerializeField] private float restartTime = 1f;
    [SerializeField] private FloatStore restartTimer;

    // if player has more segments, high score
    // if player has same segments, less duration, high score
    // if player has same segments, more duration, no high score
    // if player has less segments, no high score

    [ContextMenu("ClearHighScores")]
    private void ClearHighScores()
    {
        PlayerPrefs.DeleteAll();
    }

    public void SetHighScore()
    {
        float duration = (float)System.Math.Round(Time.timeSinceLevelLoad, 1);
        if (PlayerPrefs.HasKey(segmentsKey))
        {
            int hsSegments = PlayerPrefs.GetInt(segmentsKey);
            float hsDuration = PlayerPrefs.GetFloat(durationKey);

            currentScoreText.text = "Size = " + segments.Value + " | " + duration + "s";

            // Saving High Score
            if (segments.Value > hsSegments)
            {
                PlayerPrefs.SetInt(segmentsKey, segments.Value);
                PlayerPrefs.SetFloat(durationKey, duration);
                highScoreText.text = "New High Score!: " + currentScoreText.text;
            }
            else if (segments.Value == hsSegments && duration < hsDuration)
            {
                PlayerPrefs.SetInt(segmentsKey, segments.Value);
                PlayerPrefs.SetFloat(durationKey, duration);
                highScoreText.text = "New High Score!: " + currentScoreText.text;
            }
            else
            {
                highScoreText.text = "High Score: Size = " + hsSegments + " | " + hsDuration + "s";
            }
        }
        else
        {
            PlayerPrefs.SetInt(segmentsKey, segments.Value);
            PlayerPrefs.SetFloat(durationKey, duration);

            currentScoreText.text = "Size = " + segments.Value + " | " + duration + "s";
            highScoreText.text = "New High Score!: " + currentScoreText.text;
        }
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
    }

    [ContextMenu("CloseSelectionScreen")]
    public void CloseSelectionScreen()
    {
        ClearSelections();

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

        while (restartTimer.Value > 0)
        {
            restartTimer.Value -= Time.unscaledDeltaTime;
            yield return null;
        }

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
