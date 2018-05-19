using UnityEngine;

/// <summary>
/// Attach this script to any object to keep it between scenes. Detects duplicates and removes them.
/// </summary>
public class KeepOnLoad : MonoBehaviour {

    private static bool created = false;

    private void Awake()
    {
        if (!created) {
            DontDestroyOnLoad(this.gameObject);
            created = true;
        }

        if (FindObjectsOfType(GetType()).Length > 1)
            Destroy(gameObject);
    }
}
