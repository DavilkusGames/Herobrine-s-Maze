using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCompletedSceneManager : MonoBehaviour
{
    public GameObject nextLevelBtn;
    public static int CompletedLevelId = -1;

    private void Start()
    {
        Application.targetFrameRate = 60;
        Cursor.lockState = CursorLockMode.None;
        nextLevelBtn.SetActive(CompletedLevelId < GameData.data.levelsUnlocked.Length - 1);
    }

    public void NextLevel()
    {
        
        if (CompletedLevelId < GameData.data.levelsUnlocked.Length-1)
            YandexGames.Instance.ShowAd(() => SceneManager.LoadScene(CompletedLevelId+2));
    }

    public void MainMenu()
    {
        YandexGames.Instance.ShowAd(() => SceneManager.LoadScene(0));
    }
}
