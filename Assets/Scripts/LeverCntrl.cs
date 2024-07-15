using Plugins.Audio.Core;
using UnityEngine;

public class LeverCntrl : MonoBehaviour
{
    public Light leverLight;
    public Color[] leverLightColors;
    public float playerToLightDist = 1f;

    private Transform player;
    private Transform trans;
    private HerobrineCntrl herobrine;
    private Animator anim;
    private SourceAudio sfx;
    private bool isOn = false;

    private void Start()
    {
        trans = transform;
        anim = GetComponent<Animator>();
        sfx = GetComponent<SourceAudio>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        herobrine = GameObject.FindGameObjectWithTag("Herobrine").GetComponent<HerobrineCntrl>();
        leverLight.color = leverLightColors[0];
    }

    private void Update()
    {
        leverLight.enabled = (Vector3.Distance(trans.position, player.position) <= playerToLightDist);
    }

    public void Interact()
    {
        if (isOn) return;
        isOn = true;
        bool isLastLever = GameManager.Instance.LeverTurned();
        gameObject.tag = "Untagged";
        leverLight.color = leverLightColors[1];
        anim.Play("leverTurn");
        sfx.Play("leverFlip");

        if (!isLastLever)
        {
            herobrine.CheckLeverVisibility(this);
            herobrine.SoundReaction(player);
        }
    }

    public void Cancel()
    {
        if (!isOn) return;
        isOn = false;
        GameManager.Instance.LeverCancelled();
        gameObject.tag = "Lever";
        leverLight.color = leverLightColors[0];
        anim.Play("leverCancel");
        sfx.Play("leverFlip");
    }
}
