using UnityEngine;

public class HideWhenFloatStoreIsEmpty : MonoBehaviour
{
    [SerializeField] private FloatStore store;
    [SerializeField] private GameObject toHide;

    private void Update()
    {
        toHide.SetActive(store.Value > 0);
    }
}
