using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bin : MonoBehaviour
{
    public Ammunition PlayerAmmo;
    public bool haveKey = false;
    public string nameKey = "";
    private InteractionUni interaction;
    private int ammoAmount;
    private DialogText dialog;
    private BinController control;
    // Use this for initialization
    void Start()
    {
        PlayerAmmo = FindObjectOfType<Ammunition>();
        interaction = FindObjectOfType<InteractionUni>();
        ammoAmount = Random.Range(1, 3);
        dialog = FindObjectOfType<DialogText>();
        control = FindObjectOfType<BinController>();
    }

    public void GetNameInter()
    {
        interaction.SetTextInteract("Search the trash bin");
    }

    public void Interact()
    {
        string label = "";
        if(!haveKey)
        {
            if (ammoAmount > 0) label = "Found: bullets x " + ammoAmount;
            else label = "Found: nothing";
            PlayerAmmo.SetAmmo(ammoAmount);
            ammoAmount = 0;
        }
        else
        {
            label = "Found: " + nameKey;
            ammoAmount = 0;
            haveKey = false;
            control.SetOpenDoor();
            
        }
        dialog.SetDialogText(label, label);
        control.PlaySound();
    }
}
