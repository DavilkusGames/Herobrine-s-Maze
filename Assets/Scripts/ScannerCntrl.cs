using Plugins.Audio.Core;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScannerCntrl : MonoBehaviour
{
    public float rotSpeed = 1f;

    public TMP_Text timeTxt;
    public float timeUpdatePeriod = 30f;
    public RectTransform radarGradient;
    public float radarGradientRotSpeed = 1f;

    public RectTransform enemyMark;
    public Image enemyMarkImg;
    public Vector2 markRadiusRange = new Vector2(10f, 100f);
    public float maxEnemyDistance = 100f;
    public float markColorLerp = 1f;

    private Transform playerCam;
    private Transform herobrine;

    private Transform trans;
    private SourceAudio beepAudio;

    private float prevBeepTime = 0f;
    private float beepDelay = 1f;
    private bool isActive = true;

    private void Start()
    {
        trans = transform;
        beepAudio = GetComponent<SourceAudio>();

        playerCam = Camera.main.transform;
        herobrine = GameObject.FindGameObjectWithTag("Herobrine").transform;
        InvokeRepeating(nameof(UpdateTime), 0f, timeUpdatePeriod);
    }

    private void Update()
    {
        radarGradient.Rotate(Vector3.forward * radarGradientRotSpeed * Time.deltaTime);
        enemyMarkImg.color = Color.Lerp(enemyMarkImg.color, new Color(1f, 0f, 0f, 0f), Time.deltaTime * markColorLerp);

        if (!isActive) return;
        float enemyDist = DistanceToEnemy();
        if (enemyDist <= maxEnemyDistance)
        {
            if (Time.time >= prevBeepTime + beepDelay)
            {
                UpdateEnemyMarkPos(enemyDist);
                enemyMarkImg.color = Color.red;
                beepAudio.PlayOneShot("mtBeep");
                prevBeepTime = Time.time;
            }
        }
    }

    private void UpdateEnemyMarkPos(float enemyDist)
    {
        Vector3 directionToEnemy = herobrine.position - playerCam.position;
        float angle = Mathf.Atan2(directionToEnemy.z, directionToEnemy.x) * Mathf.Rad2Deg;

        float adjustedAngle = angle + playerCam.eulerAngles.y;

        float radarRadius = Mathf.Clamp(enemyDist / 1.4f, markRadiusRange.x, markRadiusRange.y);
        beepDelay = Mathf.Clamp(radarRadius / 75f, 0.3f, 1f);

        float normalizedAngle = adjustedAngle * Mathf.Deg2Rad;

        float x = Mathf.Cos(normalizedAngle) * radarRadius;
        float y = Mathf.Sin(normalizedAngle) * radarRadius;

        enemyMark.anchoredPosition = new Vector2(x, y);
    }

    private void LateUpdate()
    {
        trans.position = playerCam.position;
        trans.rotation = Quaternion.Lerp(trans.rotation, playerCam.rotation, rotSpeed * Time.deltaTime);
    }

    private void UpdateTime()
    {
        timeTxt.text = DateTime.Now.ToString("HH:mm");
    }

    private float DistanceToEnemy()
    {
        float xD = herobrine.position.x - playerCam.position.x;
        float zD = herobrine.position.z - playerCam.position.z;
        float dist2 = xD * xD + zD * zD;
        return dist2;
    }

    public void SetState(bool state)
    {
        isActive = state;
    }
}
