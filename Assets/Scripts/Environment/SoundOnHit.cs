using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class SoundOnHit : MonoBehaviour
{
    public bool destroy_on_hit = false;
    public AudioClip audioClip;

    protected AudioSource _as;
    protected Collider _collider;
    protected bool _destroy;
    
    // Start is called before the first frame update
    void Start()
    {
        _collider = GetComponent<Collider>();
        _as = GetComponent<AudioSource>();
        _destroy = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.isTrigger)
        {
            _as.PlayOneShot(audioClip);
            if (destroy_on_hit)
            {
                _destroy = true;
            }
        }
    }

    void FixedUpdate()
    {
        if (_destroy)
        {
            Destroy(gameObject);
        }
    }
}
