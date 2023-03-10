using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialFloatStoreDisplay : MonoBehaviour
{
    [SerializeField] private FloatStore percent;
    [SerializeField] private bool invert = true;
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        image.fillAmount = (invert ? 1 - percent.Value : percent.Value);
    }
}
