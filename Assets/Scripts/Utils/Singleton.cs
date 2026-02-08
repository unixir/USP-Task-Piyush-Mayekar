using Unity.VisualScripting;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();

    public static T Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance != null)
                {
                    return _instance;
                }
                else
                {
                    _instance = FindAnyObjectByType<T>();
                }
                return _instance;
            }
        }
    }
}