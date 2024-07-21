using UnityEngine;

public class ElevatorCntrl : MonoBehaviour
{
    public GameObject doorsCollider;
    private Animator anim;
    private bool isActive = false;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        if (other.CompareTag("Player"))
        {
            isActive = false;
            doorsCollider.SetActive(true);
            anim.Play("Ride");
            other.GetComponent<PlayerCntrl>().InElevator();
            GameObject.FindGameObjectWithTag("Herobrine").GetComponent<HerobrineCntrl>().ForceFlee();
            GameObject.FindGameObjectWithTag("Scanner").GetComponent<ScannerCntrl>().SetState(false);
        }
    }

    public void Activate()
    {
        isActive = true;
        doorsCollider.SetActive(false);
        anim.Play("Open");
    }

    public void AnimEnd()
    {
        GameManager.Instance.LevelCompleted();
    }
}
