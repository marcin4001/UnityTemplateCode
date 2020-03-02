using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using cakeslice;

public class InteractionUni : MonoBehaviour {

    public int layerStop;

    public Transform mCamera;

    public Text interText;

    private AudioSource source;

    public AudioClip pickUpClip;

    public AudioClip paperClip;

    public Outline_mesh outlineObj;

    public Outline_mesh[] outtab;

    public HUDManager HUDCanvas;

    private Health healthPlayer;


    // Use this for initialization
    void Start () {

        source = GetComponent<AudioSource>();
        
        layerStop = ~(1 << 12 | 1 << 10 | 1 << 13 | 1 << 17);
        interText = GameObject.Find("interactionsText").GetComponent<Text>();
        HUDCanvas = FindObjectOfType<HUDManager>();
        healthPlayer = GetComponent<Health>();
        outtab = FindObjectsOfType<Outline_mesh>();
        foreach (Outline_mesh o in outtab)
        {
            o.enabled = false;
        }
    }
	
	// Update is called once per frame
	void Update () {
        //interact
        if(HUDCanvas == null)HUDCanvas = GameObject.FindGameObjectWithTag("HUD").GetComponent<HUDManager>();
        if (!healthPlayer.isDead())
        {
            
            HUDCanvas.SetInteractionsIcon(false);
            RaycastHit target;
            Ray interRay = new Ray(mCamera.position, mCamera.forward);
            if (Physics.Raycast(interRay, out target, 1.7f, layerStop))
            {
                if (target.collider.gameObject.GetComponent<Outline_mesh>() != null)
                {
                    if (outlineObj) outlineObj.enabled = false;
                    outlineObj = target.collider.gameObject.GetComponent<Outline_mesh>();
                    outlineObj.enabled = true;

                }
                else
                {
                    if (outlineObj) outlineObj.enabled = false;
                    outlineObj = null;
                }

                if (target.collider.gameObject.tag == "interact" || target.collider.gameObject.tag == "door" || 
                    target.collider.gameObject.tag == "doorNH" || target.collider.gameObject.tag == "DDoor")
                {
                    target.collider.SendMessage("GetNameInter", SendMessageOptions.DontRequireReceiver);
                    HUDCanvas.SetInteractionsIcon(true);
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        target.collider.SendMessage("Interact", SendMessageOptions.DontRequireReceiver);
                    }
                }
                
            }
            else
            {
                HUDCanvas.SetInteractionsIcon(false);
                if (outlineObj != null)
                {
                    outlineObj.enabled = false;
                    outlineObj = null;
                }
            }

        }
    }

    public void SetTextInteract(string _text)
    {
        interText.text = _text;
    }

    public void PlaySound(int _sound)
    {
        switch(_sound)
        {
            case 0:
                source.PlayOneShot(pickUpClip);
                break;
            case 1:
                source.PlayOneShot(paperClip);
                break;
            default:
                source.PlayOneShot(pickUpClip);
                break;
        }
    }
}
