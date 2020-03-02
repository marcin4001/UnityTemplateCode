using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BinController : MonoBehaviour
{
    public Bin[] bins;
    public Door doorLock;
    public string nameKey;
    public AudioClip soundBin;
    private AudioSource source;
    // Start is called before the first frame update
    void Start()
    {
        doorLock.isLock = true;
        bins = FindObjectsOfType<Bin>();
        int random_index = Random.Range(0, bins.Length);
        bins[random_index].haveKey = true;
        bins[random_index].nameKey = nameKey;
        source = GetComponent<AudioSource>();
    }

    public void SetOpenDoor()
    {
        FindObjectOfType<EquipmentSystem>().AddItem(nameKey);
        doorLock.isLock = false;
    }

    public void PlaySound()
    {
        source.PlayOneShot(soundBin);
    }
}
