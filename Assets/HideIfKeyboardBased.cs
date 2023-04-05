using UnityEngine;

public class HideIfKeyboardBased : MonoBehaviour
{
    public static bool _IsKeyboardBased =>
            Application.platform == RuntimePlatform.WindowsPlayer
            || Application.platform == RuntimePlatform.WindowsEditor
            || Application.platform == RuntimePlatform.WebGLPlayer;

    private void Awake()
    {
        if (_IsKeyboardBased)
        {
            gameObject.SetActive(false);
        }
    }
}
