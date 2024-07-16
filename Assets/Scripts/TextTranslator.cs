using System.Text;
using TMPro;
using UnityEngine;

public class TextTranslator : MonoBehaviour
{
    public TMP_Text txt;
    public string engStr = string.Empty;

    private string rusStr = string.Empty;
    private string additionalText = string.Empty;

    private void Awake()
    {
        rusStr = txt.text;
    }

    private void Start()
    {
        YandexGames.Instance?.AddToTranslateQueue(this);
    }

    private void OnDestroy()
    {
        YandexGames.Instance?.RemoveFromTranslateQueue(this);
    }

    public void AddAdditionalText(string str)
    {
        additionalText = str;
        UpdateText();
    }

    public void UpdateText()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(YandexGames.IsRus ? rusStr : engStr);
        sb.Append(additionalText);
        txt.text = sb.ToString();
    }
}
