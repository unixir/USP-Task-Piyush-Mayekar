using TMPro;
using UnityEngine;

public class CustomDebugConsole : Singleton<CustomDebugConsole>
{
    [Tooltip("If enabled, logs will be shown in the UI text component. If disabled, logs will go to the standard Unity console.")]
    [SerializeField] bool _enableUIConsole = true;
    [SerializeField] TMP_Text _text;

    public void Log(string message, GameObject context = null)
    {
        if (_enableUIConsole)
        {
            _text.text += message + "\n";
        }
        else
            Debug.Log(message, context);
    }

    public void Clear()
    {
        if (_enableUIConsole)
        {
            _text.text = "";
        }
    }
}
