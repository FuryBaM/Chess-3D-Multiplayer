using UnityEngine;

public class DebugConsoleBuild : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tilde))
        {
            Debug.developerConsoleVisible = !Debug.developerConsoleVisible;
        }
    }
}
