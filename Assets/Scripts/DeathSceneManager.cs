using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathSceneManager : MonoBehaviour
{
    public static int FailedLevelId = -1;

    private void Start()
    {
        Application.targetFrameRate = 60;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Retry()
    {
        YandexGames.Instance.ShowAd(() => SceneManager.LoadScene(FailedLevelId + 1)); 
    }

    public void MainMenu()
    {
        YandexGames.Instance.ShowAd(() => SceneManager.LoadScene(0));
    }
}
