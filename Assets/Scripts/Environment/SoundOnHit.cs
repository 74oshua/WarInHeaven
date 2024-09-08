using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class BonkOnHit : MonoBehaviour
{
    public AudioClip audioClip;

    protected AudioSource _as;
    protected Collider _collider;
    
    // Start is called before the first frame update
    void Start()
    {
        _collider = GetComponent<Collider>();
        _as = GetComponent<AudioSource>();
    }

    void OnCollisionEnter()
    {
        _as.PlayOneShot(audioClip);
    }
}
