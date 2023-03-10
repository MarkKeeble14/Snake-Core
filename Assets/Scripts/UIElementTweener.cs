using DG.Tweening;
using UnityEngine;

public abstract class UIElementTweener : MonoBehaviour
{
    [SerializeField] protected float inDuration = .5f;
    [SerializeField] protected float outDuration = .5f;
    [SerializeField] protected Ease inEase = Ease.InOutSine;
    [SerializeField] protected Ease outEase = Ease.InOutSine;

    public abstract void In();
    public abstract void Out();
}
