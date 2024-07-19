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
        SceneManager.LoadScene(FailedLevelId+1);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
