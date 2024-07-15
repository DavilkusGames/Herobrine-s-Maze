using Plugins.Audio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject loadingPanel;
    public TMP_Text versionTxt;
    public string devGamesURL = string.Empty;

    public static MainMenuManager Instance { get; private set; }

    public void DataLoaded(bool firstTime)
    {
        loadingPanel.SetActive(false);
        if (firstTime) YandexGames.Instance.GameInitialized();

        if (GameData.data.prevGameVersion != Application.version.ToString())
        {
            GameData.data.prevGameVersion = Application.version.ToString();
            GameData.SaveData();
        }
    }

    public void PlayGame(int mapId)
    {
        loadingPanel.SetActive(true);
        SceneManager.LoadScene(mapId+1);
    }

    public void MoreGames()
    {
        Application.OpenURL(devGamesURL);
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        GetComponent<SourceAudio>().Play("menuMusic");
        versionTxt.text = "v." + Application.version;
        Cursor.lockState = CursorLockMode.None;

        if (GameData.dataLoaded) DataLoaded(false);
        else if (Application.isEditor) GameData.LoadData();
        else loadingPanel.SetActive(true);
    }

    private void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(Instance.gameObject);
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        CancelInvoke();
    }
}
