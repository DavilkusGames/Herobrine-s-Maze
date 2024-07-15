using System;
using UnityEngine;
using UnityEngine.UI;

public class BlackPanelCntrl : MonoBehaviour
{
    public float fadeSpeed = 1f;
    private Image img;

    private float startAlpha = 0f;
    private float targetAlpha = 0f;
    private float progress = 1f;

    private Action callback = null;

    private void Start()
    {
        img = GetComponent<Image>();
    }

    private void Update()
    {
        if (progress < 1f)
        {
            progress += fadeSpeed * Time.deltaTime;
            if (progress >= 1f)
            {
                progress = 1f;
                if (callback != null) callback();
            }
            UpdateAlpha(Mathf.Lerp(startAlpha, targetAlpha, progress));
        }
    }

    public void FadeIn(Action callback = null)
    {
        startAlpha = 0f;
        targetAlpha = 1f;
        progress = 0f;
        UpdateAlpha(startAlpha);
        this.callback = callback;
    }

    public void FadeOut(Action callback = null)
    {
        startAlpha = 1f;
        targetAlpha = 0f;
        progress = 0f;
        UpdateAlpha(startAlpha);
        this.callback = callback;
    }

    private void UpdateAlpha(float alpha)
    {
        img.color = new Color(0, 0, 0, alpha);
    }
}
