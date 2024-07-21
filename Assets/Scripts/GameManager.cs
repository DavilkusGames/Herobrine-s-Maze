using Plugins.Audio.Core;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int levelId = 0;
    public GameObject pausePanel;
    public GameObject[] keyTips;
    public TextTranslator leverCountTxt;
    public TextTranslator escapeTxt;
    public GameObject levelCompletedTxt;
    public BlackPanelCntrl blackPanel;

    public Slider sensitivitySlider;
    public Toggle soundToggle;
    public TMP_Dropdown languageDropdown;

    private bool isMobile = false;
    private bool isPaused = false;

    public static GameManager Instance;

    private int leverCount = 0;
    private int targetLeverCount = 0;
    private bool unsavedSettings = false;

    private PlayerCntrl player;
    private HerobrineCntrl herobrine;
    private ElevatorCntrl elevator;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        GetComponent<SourceAudio>().Play("ambience");

        isMobile = YandexGames.IsMobile;
        if (!isMobile) Cursor.lockState = CursorLockMode.Locked;
        if (isMobile)
        {
            foreach (var keyTip in keyTips) keyTip.SetActive(false);
        }

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCntrl>();
        herobrine = GameObject.FindGameObjectWithTag("Herobrine").GetComponent<HerobrineCntrl>();
        elevator = GameObject.FindGameObjectWithTag("Elevator").GetComponent<ElevatorCntrl>();
        targetLeverCount = GameObject.FindGameObjectsWithTag("Lever").Length;
        UpdateLeverCount();
        blackPanel.FadeOut();

        if (GameData.data != null)
        {
            player.SetSensitivity(GameData.data.sensitivity);
            sensitivitySlider.value = GameData.data.sensitivity;
            soundToggle.isOn = GameData.data.soundEnabled;
        }
        languageDropdown.value = YandexGames.IsRus ? 0 : 1;
    }

    public bool LeverTurned()
    {
        leverCount++;
        if (leverCount < targetLeverCount)
        {
            UpdateLeverCount();
            return false;
        }
        else
        {
            leverCountTxt.gameObject.SetActive(false);
            escapeTxt.gameObject.SetActive(true);
            herobrine.ForceChase();
            elevator.Activate();
            player.Scanner.EnableElevatorMark(elevator.transform);
            return true;
        }
    }

    public void LeverCancelled()
    {
        leverCount--;
        UpdateLeverCount();
    }

    private void UpdateLeverCount()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(leverCount);
        sb.Append('/');
        sb.Append(targetLeverCount);
        leverCountTxt.AddAdditionalText(sb.ToString());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) PauseState(!isPaused);
    }

    public void PauseState(bool state)
    {
        isPaused = state;
        if (!isMobile) Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        AudioListener.volume = isPaused || (GameData.data != null && !GameData.data.soundEnabled) ? 0f : 1f;
        Time.timeScale = isPaused ? 0f : 1f;
        pausePanel.SetActive(isPaused);
    }

    public void LevelCompleted()
    {
        GameData.data.levelsCompleted[levelId] = true;
        if (levelId < GameData.data.levelsUnlocked.Length - 1) GameData.data.levelsUnlocked[levelId+1] = true;
        GameData.data.levelsCompletedCount++;
        GameData.SaveData();

        YandexGames.Instance?.SaveToLeaderboard(GameData.data.levelsCompletedCount);

        blackPanel.FadeIn(() =>
        {
            levelCompletedTxt.SetActive(true);
            AudioListener.volume = 0f;
            Invoke(nameof(ToMainMenu), 4f);
        });
    }

    public void GameOver()
    {
        DeathSceneManager.FailedLevelId = levelId;
        SceneManager.LoadScene(SceneManager.sceneCountInBuildSettings-1);
    }

    public void ToMainMenu()
    {
        Time.timeScale = 1f;
        if (GameData.data.soundEnabled) AudioListener.volume = 1f;
        YandexGames.Instance.ShowAd(() => SceneManager.LoadScene(0));
    }

    public void ChangeSensitivity(float sens)
    {
        if (GameData.data.sensitivity == sens) return;
        GameData.data.sensitivity = sens;
        player.SetSensitivity(sens);
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

    // Utility
    public static float FastDistance(Vector3 pos1, Vector3 pos2)
    {
        float xD = pos1.x - pos2.x;
        float zD = pos1.z - pos2.z;
        float dist2 = xD * xD + zD * zD;
        return dist2;
    }
}
