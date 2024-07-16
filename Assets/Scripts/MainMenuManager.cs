using Plugins.Audio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public GameObject loadingPanel;
    public TMP_Text versionTxt;
    public string devGamesURL = string.Empty;

    public GameObject[] levelsLockPanels;
    public GameObject[] levelsCompleteIcons;
    public GameObject adConfirmPanel;

    public Slider sensitivitySlider;
    public Toggle soundToggle;
    public TMP_Dropdown languageDropdown;

    public static MainMenuManager Instance { get; private set; }

    private int selectedLevelId = -1;
    private int levelAdsUnlockId = -1;
    private bool unsavedSettings = false;

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

        sensitivitySlider.value = GameData.data.sensitivity;
        soundToggle.isOn = GameData.data.soundEnabled;
        languageDropdown.value = YandexGames.IsRus ? 0 : 1;
        AudioListener.volume = GameData.data.soundEnabled ? 1f : 0f;
    }

    public void PlayGame(int levelId)
    {
        if (GameData.data.levelsUnlocked[levelId])
        {
            loadingPanel.SetActive(true);
            selectedLevelId = levelId;
            Invoke(nameof(LoadLevel), 2f);
        }
        else
        {
            levelAdsUnlockId = levelId;
            adConfirmPanel.SetActive(true);
        }
    }

    public void ConfirmAdsLevelUnlock()
    {
        loadingPanel.SetActive(true);
        YandexGames.Instance.ShowRewarded((bool wasRewarded) =>
        {
            loadingPanel.SetActive(false);
            adConfirmPanel.SetActive(false);
            if (wasRewarded)
            {
                GameData.data.levelsUnlocked[levelAdsUnlockId] = true;
                GameData.SaveData();

                levelsLockPanels[levelAdsUnlockId].SetActive(false);
            }
        });
    }

    public void LoadLevel()
    {
        SceneManager.LoadScene(selectedLevelId + 1);
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

    public void ChangeSensitivity(float sens)
    {
        if (GameData.data.sensitivity == sens) return;
        GameData.data.sensitivity = sens;
        unsavedSettings = true;
    }

    public void ChangeSoundState(bool state)
    {
        if (GameData.data.soundEnabled == state) return;
        GameData.data.soundEnabled = state;
        unsavedSettings = true;
        AudioListener.volume = GameData.data.soundEnabled ? 1f : 0f;
    }

    public void ForceChangeLang(int langId)
    {
        if ((langId == 1 && YandexGames.IsRus) || (langId == 0 && !YandexGames.IsRus))
        {
            YandexGames.Instance.ForceLang(langId);
        }
    }

    public void ApplySettings()
    {
        if (unsavedSettings)
        {
            GameData.SaveData();
            unsavedSettings = false;
        }
    }
}
