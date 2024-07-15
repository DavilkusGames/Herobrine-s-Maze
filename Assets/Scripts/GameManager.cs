using Plugins.Audio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject[] keyTips;
    public TextTranslator leverCountTxt;
    public TextTranslator escapeTxt;
    public BlackPanelCntrl blackPanel;

    private bool isMobile = false;
    private bool isPaused = false;

    public static GameManager Instance;

    private int leverCount = 0;
    private int targetLeverCount = 0;

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

        herobrine = GameObject.FindGameObjectWithTag("Herobrine").GetComponent<HerobrineCntrl>();
        elevator = GameObject.FindGameObjectWithTag("Elevator").GetComponent<ElevatorCntrl>();
        targetLeverCount = GameObject.FindGameObjectsWithTag("Lever").Length;
        UpdateLeverCount();
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
        leverCountTxt.AddAdditionalText(leverCount.ToString() + '/' + targetLeverCount.ToString());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) PauseState(!isPaused);
    }

    public void PauseState(bool state)
    {
        isPaused = state;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        AudioListener.volume = isPaused ? 0f : 1f;
        Time.timeScale = isPaused ? 0f : 1f;
        pausePanel.SetActive(isPaused);
    }

    public void LevelCompleted()
    {
        blackPanel.FadeIn(ToMainMenu);
    }

    public void ToMainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.volume = 1f;
        SceneManager.LoadScene(0);
    }
}
