using UnityEngine;
using System.Collections;

public class CallNextSelectionOnBarFill : MonoBehaviour
{
    [SerializeField] private int next;
    [SerializeField] private IntStore store;
    [SerializeField] private ImageSliderBar bar;
    private int nextTracker;
    private int lastFoodCount;

    private void Awake()
    {
        AddOnFullActions();
    }

    public void AddOnFullActions()
    {
        bar.AddOnFullAction(delegate
        {
            StartCoroutine(ExecuteOnFill());
        });
    }

    private IEnumerator ExecuteOnFill()
    {
        yield return new WaitUntil(() => !SnakeBehaviour._Instance.IsOnTargetCell);

        yield return new WaitUntil(() => SnakeBehaviour._Instance.IsOnTargetCell);

        UIManager._Instance.OpenSelectionScreen();
        nextTracker = 0;
        bar.Set(nextTracker, next);
        bar.ClearOnFullAction();
    }

    private void Update()
    {
        if (lastFoodCount != store.Value)
            nextTracker++;

        if (store.Value == 0)
            bar.Set(0, 1);
        else
            bar.Set(nextTracker, next);

        lastFoodCount = store.Value;
    }
}
