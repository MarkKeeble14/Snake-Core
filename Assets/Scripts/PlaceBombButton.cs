using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceBombButton : MonoBehaviour
{

    public void OnPress()
    {
        SnakeBehaviour._Instance.TryPlaceBomb();
    }
}
