using DG.Tweening;
using UnityEngine;

public class ScaleTweener : UIElementTweener
{
    [SerializeField] private float scaleToMultiplier = 1.25f;

    private Vector3 originalScale;
    private Vector3 scaleTo;

    private void Awake()
    {
        originalScale = transform.localScale;
        scaleTo = originalScale * scaleToMultiplier;
    }

    public override void In()
    {
        transform.DOComplete();
        transform.DOScale(scaleTo, inDuration)
            .SetEase(inEase)
            .SetUpdate(true);
    }

    public override void Out()
    {
        transform.DOComplete();
        transform.DOScale(originalScale, outDuration)
            .SetEase(outEase)
            .SetUpdate(true);
    }
}
