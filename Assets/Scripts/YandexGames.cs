using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class YandexGames : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern bool SDKInit();

    [DllImport("__Internal")]
    private static extern bool PlayerInit();

    [DllImport("__Internal")]
    private static extern bool AuthCheck();

    [DllImport("__Internal")]
    private static extern void GameReady();

    [DllImport("__Internal")]
    private static extern string GetLang();

    [DllImport("__Internal")]
    private static extern string GetDomain();

    [DllImport("__Internal")]
    private static extern bool IsMobilePlatform();

    [DllImport("__Internal")]
    private static extern void ShowFullscreenAd();

    [DllImport("__Internal")]
    private static extern void ShowRewardedAd();

    [DllImport("__Internal")]
    private static extern void SaveToLb(int score);

    [DllImport("__Internal")]
    private static extern void SaveCloudData(string data);

    [DllImport("__Internal")]
    private static extern void LoadCloudData();

    public delegate void RewardedCallback(bool isRewarded);

    public static YandexGames Instance { get; private set; }
    public static bool IsInit { get; private set; }
    public static bool IsRus { get; private set; }
    public static bool IsAuth { get; private set; }
    public static bool IsMobile { get; private set; }
    public static string Domain { get; private set; }

    private static string[] RusLangDomens = { "ru", "be", "kk", "uk", "uz" };
    private List<TextTranslator> translateQueue = new List<TextTranslator>();
    private Action adCallback;
    private RewardedCallback rewardedCallback;
    private float prevAdShowTime = 0f;
    private bool isRewarded = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    public void AddToTranslateQueue(TextTranslator sender)
    {
        translateQueue.Add(sender);
        if (IsInit || Application.isEditor) sender.UpdateText();
    }

    public void RemoveFromTranslateQueue(TextTranslator sender)
    {
        if (translateQueue.Contains(sender)) translateQueue.Remove(sender);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        CancelInvoke();
    }

    private void Start()
    {
        if (!Application.isEditor) StartCoroutine(nameof(WaitForSDKInit));
    }

    public void ShowAd(Action callback=null)
    {
        if (Application.isEditor || !IsInit)
        {
            Debug.Log("Ad cannot be shown in editor or SDK not initialized");
            callback();
            return;
        }

        if (Time.time - prevAdShowTime > 61f)
        {
            adCallback = callback;
            AudioListener.volume = 0f;
            prevAdShowTime = Time.time;
            ShowFullscreenAd();
        }
        else
        {
            Debug.Log("Ad called too early. Skipped");
            callback();
            return;
        }
    }

    public void ShowRewarded(RewardedCallback callback=null)
    {
        if (Application.isEditor || !IsInit)
        {
            Debug.Log("Rewarded ad cannot be shown in editor or SDK not initialized");
            callback(true);
            return;
        }

        isRewarded = false;
        rewardedCallback = callback;
        AudioListener.volume = 0f;
        ShowRewardedAd();
    }

    public void Rewarded()
    {
        isRewarded = true;
    }

    public void RewardedClosed()
    {
        if (GameData.data.soundEnabled) AudioListener.volume = 1f;
        if (rewardedCallback != null)
        {
            rewardedCallback(isRewarded);
            rewardedCallback = null;
        }
    }

    public void AdShown()
    {
        if (GameData.data.soundEnabled) AudioListener.volume = 1f;
        if (adCallback != null)
        {
            adCallback();
            adCallback = null;
        }
    }

    public void SaveToLeaderboard(int score)
    {
        if (Application.isEditor || !IsInit || !IsAuth)
        {
            Debug.Log("Leaderboard save failed");
            return;
        }

        SaveToLb(score);
        Debug.Log("Saved to lb: " + score.ToString());
    }

    public void SaveData(string str)
    {
        if (IsInit && IsAuth)
        {
            Debug.Log("Saving to cloud: " + str + "...");
            SaveCloudData(str);
        }
        else Debug.Log("Cloud save failed");
    }

    public bool LoadData()
    {
        if (IsInit && IsAuth)
        {
            LoadCloudData();
            return true;
        }
        else
        {
            return false;
        }
    }

    public void DataLoaded(string data)
    {
        Debug.Log("Loaded from cloud: " + data);
        GameData.CloudDataLoaded(data);
    }

    public void DataSaved()
    {
        Debug.Log("Data saved to cloud successfully");
    }

    public void GameInitialized()
    {
        if (IsInit) GameReady();
    }

    public void ForceLang(int langId)
    {
        IsRus = (langId == 0);
        foreach (var text in translateQueue) text.UpdateText();
    }

    private IEnumerator WaitForSDKInit()
    {
        yield return new WaitForSeconds(0.5f);
        while (!SDKInit()) yield return new WaitForSeconds(0.2f);
        IsInit = true;
        prevAdShowTime = Time.time;

        IsRus = RusLangDomens.Contains(GetLang());
        Debug.Log("IsRus: " + IsRus.ToString());

        Domain = GetDomain();
        Debug.Log("Domain: " + Domain);

        IsMobile = IsMobilePlatform();
        Debug.Log("IsMobile: " + IsMobile.ToString());

        foreach (var text in translateQueue) text.UpdateText();

        while (!PlayerInit()) yield return new WaitForSeconds(0.2f);
        IsAuth = AuthCheck();
        Debug.Log("IsAuth: " + IsAuth.ToString());

        GameData.LoadData();
    }
}
