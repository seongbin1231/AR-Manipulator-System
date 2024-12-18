using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
///     This class control the coversion of scenes
/// </summary>
public class SceneController : MonoBehaviour
{
    public void GotoScene(string sceneName)
    {
        // All of the scene activated Unload
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name != sceneName)
            {
                SceneManager.UnloadSceneAsync(scene);
            }
        }

        // Load new scene by single mode
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
