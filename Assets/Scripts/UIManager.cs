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

    [SerializeField] private TextMeshProUGUI currentSegmentsText;
    [SerializeField] private TextMeshProUGUI highScoreSegmentsText;
    [SerializeField] private TextMeshProUGUI currentDurationText;
    [SerializeField] private TextMeshProUGUI highScoreDurationText;

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

    public void SetHighScore(string key, int score, string prefix, string suffix, TextMeshProUGUI reg, TextMeshProUGUI hs)
    {
        string plurality = (score == 1 ? "" : "s");
        reg.text = prefix + score.ToString() + suffix + plurality;

        // Seconds Survived
        if (PlayerPrefs.HasKey(key))
        {
            float hsValue = PlayerPrefs.GetInt(key);
            if (score > hsValue)
            {
                PlayerPrefs.SetInt(key, score);
                hs.text = "New High Score!: " + prefix + score.ToString() + suffix + plurality;
            }
            else
            {
                hs.text = "High Score: " + prefix + hsValue.ToString() + suffix + plurality;
            }
        }
        else
        {
            PlayerPrefs.SetInt(key, score);
            hs.text = "New High Score!: " + prefix + score.ToString() + suffix + plurality;
        }
    }

    public void SetScores()
    {
        SetHighScore(segmentsKey, segments.Value, "Ate ", " Apple", currentSegmentsText, highScoreSegmentsText);
        SetHighScore(durationKey, Mathf.RoundToInt(Time.timeSinceLevelLoad), "Lasted ", " Second", currentDurationText, highScoreDurationText);
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

        // Re-enable Snake
        // Disable Snake
        SnakeBehaviour._Instance.StartMoving();

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
