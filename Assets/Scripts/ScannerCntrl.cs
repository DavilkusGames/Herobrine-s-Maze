using Plugins.Audio.Core;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeverMark
{
    public Transform objT;
    public GameObject markObj;
    public RectTransform markT;
}

public class ScannerCntrl : MonoBehaviour
{
    public float rotSpeed = 1f;

    public TMP_Text timeTxt;
    public float timeUpdatePeriod = 30f;
    public RectTransform radarGradient;
    public float radarGradientRotSpeed = 1f;

    public RectTransform enemyMark;
    public RectTransform elevatorMark;
    public Image enemyMarkImg;
    public Vector2 markRadiusRange = new Vector2(10f, 100f);
    public float maxEnemyDistance = 100f;
    public float markColorLerp = 1f;

    public GameObject leverMarkPrefab;
    public Transform markParent;

    private Transform playerCam;
    private Transform herobrine;

    private Transform trans;
    private SourceAudio beepAudio;

    private float prevBeepTime = 0f;
    private float beepDelay = 1f;
    private bool isActive = true;

    private Transform elevator;
    private List<LeverMark> leverMarks = new List<LeverMark>();
    private bool elevatorMarkEnabled = false;

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

        if (elevatorMarkEnabled) UpdateMarkPos(elevator, elevatorMark, GameManager.FastDistance(elevator.position, playerCam.position));
        foreach (var lm in leverMarks) UpdateMarkPos(lm.objT, lm.markT, GameManager.FastDistance(lm.objT.position, playerCam.position));

        float enemyDist = GameManager.FastDistance(herobrine.position, playerCam.position);
        if (enemyDist <= maxEnemyDistance)
        {
            if (Time.time >= prevBeepTime + beepDelay)
            {
                UpdateMarkPos(herobrine, enemyMark, enemyDist, true);
                enemyMarkImg.color = Color.red;
                beepAudio.PlayOneShot("mtBeep");
                prevBeepTime = Time.time;
            }
        }
    }

    private void UpdateMarkPos(Transform target, RectTransform markT, float markDist, bool isEnemy = false)
    {
        Vector3 directionToTarget = target.position - playerCam.position;
        float angle = Mathf.Atan2(directionToTarget.z, directionToTarget.x) * Mathf.Rad2Deg;

        float adjustedAngle = angle + playerCam.eulerAngles.y;

        float radarRadius = Mathf.Clamp(markDist / 1.5f, markRadiusRange.x, markRadiusRange.y);
        if (isEnemy) beepDelay = Mathf.Clamp(radarRadius / 75f, 0.25f, 1f);

        float normalizedAngle = adjustedAngle * Mathf.Deg2Rad;

        float x = Mathf.Cos(normalizedAngle) * radarRadius;
        float y = Mathf.Sin(normalizedAngle) * radarRadius;

        markT.anchoredPosition = new Vector2(x, y);
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

    public void EnableElevatorMark(Transform elevator)
    {
        elevatorMark.gameObject.SetActive(true);
        this.elevator = elevator;
        elevatorMarkEnabled = true;
    }

    public void AddLeverToMarkList(Transform lever)
    {
        LeverMark lm = new LeverMark();
        lm.objT = lever;
        lm.markObj = Instantiate(leverMarkPrefab, markParent);
        lm.markT = lm.markObj.GetComponent<RectTransform>();
        leverMarks.Add(lm);
    }

    public void RemoveLeverFromMarkList(Transform lever)
    {
        for (int i = 0; i < leverMarks.Count; i++)
        {
            if (leverMarks[i].objT == lever)
            {
                Destroy(leverMarks[i].markObj);
                leverMarks.RemoveAt(i);
                return;
            }
        }
    }

    public void SetState(bool state)
    {
        isActive = state;
    }
}
