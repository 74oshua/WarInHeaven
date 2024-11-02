using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TargetType
{
    None =          0b00,
    Satellite =     0b01,
    Planet =        0b10
}

[RequireComponent(typeof(OrbitalBody))]
public class Targetable : MonoBehaviour
{
    // how close a scanner has to be to see this object passively, regardless of the scanner's ranger
    public float passive_range = 0f;
    public TargetType type;

    private OrbitalBody _ob;
    public OrbitalBody ob
    {
        get { return _ob; }
    }
    // private static List<Scanner> _scanners = new List<Scanner>();
    private SphereCollider _trigger;

    public Vector3 velocity
    {
        get { return _ob.rb.linearVelocity; }
    }

    void Start()
    {
        _ob = GetComponent<OrbitalBody>();

        // add trigger collider to parent object
        if (passive_range > 0)
        {
            _trigger = gameObject.AddComponent<SphereCollider>();
            _trigger.isTrigger = true;
            _trigger.radius = passive_range;
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        Scanner scanner = collider.GetComponent<Scanner>();
        if (scanner)
        {
            scanner.AddTarget(this);
        }
    }

    void OnTriggerExit(Collider collider)
    {
        Scanner scanner = collider.GetComponent<Scanner>();
        if (scanner)
        {
            scanner.RemoveTarget(this);
        }
    }
}
