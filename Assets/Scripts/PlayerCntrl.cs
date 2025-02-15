using Plugins.Audio.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCntrl : MonoBehaviour
{
    public Transform cam;
    public Animator playerAnim;
    public GameObject model;

    public float moveSpeed = 4f;
    public float sensitivity = 4f;
    public Vector2 yBorders = Vector2.zero;

    public GameObject mobileControls;
    public JoystickRight rightJoystick;

    public GameObject interactBtn;
    public float interactMaxDist = 5f;
    public GameObject[] crosshairs;
    public float walkDelay = 1f;
    public float jumpscareCamLerp = 7f;

    private CharacterController characterController;
    private Transform trans;
    private SourceAudio walkAudio;

    private Vector3 movementInput = Vector3.zero;
    private bool isMobile = false;

    private float camX = 0f;
    private float camY = 0f;

    private GameObject interactObj = null;
    private float prevWalkSfxTime = 0f;
    private bool isAlive = true;

    private GameObject canvas;
    private ScannerCntrl scanner;

    private Quaternion targetJumpscareRot;

    public ScannerCntrl Scanner { get { return scanner; } private set { scanner = value; } } 

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        trans = GetComponent<Transform>();
        walkAudio = GetComponent<SourceAudio>();

        isMobile = YandexGames.IsMobile;
        mobileControls.SetActive(isMobile);
        moveSpeed /= 100f;

        canvas = GameObject.Find("Canvas");
        scanner = GameObject.Find("Scanner").GetComponent<ScannerCntrl>();
        camX = trans.rotation.eulerAngles.y;
        if (isMobile)
        {
            JoystickRight.rotY = 0f;
            JoystickRight.rotX = camX;
            JoystickLeft.positionX = 0f;
            JoystickLeft.positionY = 0f;
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0f || !isAlive) return;

        if (!isMobile) {
            camX += Input.GetAxis("Mouse X") * sensitivity;
            camY += Input.GetAxis("Mouse Y") * sensitivity;
            camY = Mathf.Clamp(camY, yBorders.x, yBorders.y);
        }
        else
        {
            camX = JoystickRight.rotX;
            camY = JoystickRight.rotY;
        }

        trans.rotation = Quaternion.Euler(0f, camX, 0f);
        cam.localRotation = Quaternion.Euler(-camY, 0f, 0f);
        playerAnim.SetBool("IsWalking", movementInput != Vector3.zero);

        if (movementInput != Vector3.zero)
        {
            if (Time.time > prevWalkSfxTime + walkDelay)
            {
                walkAudio.PlayOneShot("Step_" + Random.Range(1, 9).ToString());
                prevWalkSfxTime = Time.time;
            }
        }

        if (Input.GetKeyDown(KeyCode.E)) Interact();
    }

    private void LateUpdate()
    {
        if (!isAlive)
        {
            cam.rotation = Quaternion.Lerp(cam.rotation, targetJumpscareRot, jumpscareCamLerp*Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (Time.timeScale == 0f || !isAlive) return;
        CheckInteractionRaycast();

        if (!isMobile)
            movementInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        else
            movementInput = new Vector3(JoystickLeft.positionX, 0f, JoystickLeft.positionY);
        movementInput.Normalize();

        characterController.Move(trans.TransformDirection(movementInput) * moveSpeed);
    }

    public void Interact()
    {
        if (interactObj == null) return;
        switch (interactObj.tag)
        {
            case "Lever":
                interactObj.GetComponent<LeverCntrl>().Interact();
                break;
        }

        interactBtn.SetActive(false);
        crosshairs[0].SetActive(true);
        crosshairs[1].SetActive(false);
        interactObj = null;
    }

    private void CheckInteractionRaycast()
    {
        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, interactMaxDist))
        {
            GameObject obj = hit.collider.gameObject;
            if (obj.CompareTag("Lever"))
            {
                interactObj = obj;
            }
            else interactObj = null;
        }
        else interactObj = null;
        interactBtn.SetActive(interactObj != null);
        crosshairs[0].SetActive(interactObj == null);
        crosshairs[1].SetActive(interactObj != null);
    }

    public void SetSensitivity(float sens)
    {
        sensitivity = sens;
        rightJoystick.sensitivity = sens;
    }

    public void GameOver(Transform herobrine)
    {
        isAlive = false;
        targetJumpscareRot = Quaternion.LookRotation(herobrine.position - trans.position);
        mobileControls.SetActive(false);
        canvas.SetActive(false);
        scanner.gameObject.SetActive(false);
        model.SetActive(false);
    }

    public void InElevator()
    {
        gameObject.tag = "Untagged";
    }
}
