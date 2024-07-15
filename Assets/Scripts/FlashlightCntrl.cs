using UnityEngine;

public class FlashlightCntrl : MonoBehaviour
{
    public float rotSpeed = 1f;

    private Transform playerCam;
    private Transform trans;

    private void Start()
    {
        trans = transform;
        playerCam = Camera.main.transform;
    }

    private void LateUpdate()
    {
        trans.position = playerCam.position;
        trans.rotation = Quaternion.Lerp(trans.rotation, playerCam.rotation, rotSpeed * Time.deltaTime);
    }
}
