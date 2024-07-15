using Plugins.Audio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject loadingPanel;
    public TMP_Text versionTxt;
    public string devGamesURL = string.Empty;

    public GameObject[] levelsLockPanels;
    public GameObject[] levelsCompleteIcons;

    public static MainMenuManager Instance { get; private set; }

    private int selectedLevelId = 0;

    public void DataLoaded(bool firstTime)
    {
        loadingPanel.SetActive(false);
        if (firstTime) YandexGames.Instance.GameInitialized();

        if (GameData.data.prevGameVersion != Application.version.ToString())
        {
            GameData.data.prevGameVersion = Application.version.ToString();
            GameData.SaveData();
        }

        for (int i = 0; i < levelsLockPanels.Length; i++) levelsLockPanels[i].SetActive(!GameData.data.levelsUnlocked[i]);
        for (int i = 0; i < levelsCompleteIcons.Length; i++) levelsCompleteIcons[i].SetActive(GameData.data.levelsCompleted[i]);
    }
    
    public void PlayGame(int levelId)
    {
        if (GameData.data.levelsUnlocked[levelId])
        {
            loadingPanel.SetActive(true);
            selectedLevelId = levelId;
            Invoke(nameof(LoadLevel), 2f);
        }
    }

    public void LoadLevel()
    {
        SceneManager.LoadScene(selectedLevelId+1);
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
