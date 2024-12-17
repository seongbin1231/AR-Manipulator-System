using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void GotoScene(string sceneName)
    {
        // 현재 활성화된 모든 씬을 언로드
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name != sceneName)
            {
                SceneManager.UnloadSceneAsync(scene);
            }
        }

        // 새로운 씬을 Single 모드로 로드
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
