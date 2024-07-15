using TMPro;
using UnityEngine;

public class TextTranslator : MonoBehaviour
{
    public TMP_Text txt;
    public string engStr = string.Empty;

    private string baseText = string.Empty;
    private string additionalText = string.Empty;
    public bool isRus { get; private set; }
    private bool isTranslated = false;

    private void Start()
    {
        if (YandexGames.Instance != null) YandexGames.Instance.AddToTranslateQueue(this);
        else Translate(true);
    }

    private void OnDestroy()
    {
        if (YandexGames.Instance != null) YandexGames.Instance.RemoveFromTranslateQueue(this);
    }

    public void AddAdditionalText(string str)
    {
        additionalText = str;
        if (isTranslated) txt.text = baseText + additionalText;
    }

    public void SetText(string text)
    {
        txt.text = text;
        isTranslated = true;
    }

    public void Translate(bool isRus)
    {
        if (isTranslated) return;
        if (!isRus) baseText = engStr;
        else baseText = txt.text;

        txt.text = baseText + additionalText;
        this.isRus = isRus;
        isTranslated = true;
    }
}
