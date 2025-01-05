using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TargetType
{
    None =          0b000,
    Satellite =     1 << 1,
    Planet =        1 << 2,
    Projectile =    1 << 3,
    Spacecraft =    1 << 4
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
    private SphereCollider _trigger;

    public Vector3 velocity
    {
        get { return _ob.rb.velocity; }
    }

    // scanners which currently detect this object
    private List<Scanner> _detected_scanners = new();

    void OnDestroy()
    {
        // in case of destruction, remove this target from all scanners
        foreach (Scanner s in _detected_scanners)
        {
            if (!s)
            {
                continue;
            }
            s.RemoveTarget(this, true);
        }
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
            _detected_scanners.Add(scanner);
        }
    }

    void OnTriggerExit(Collider collider)
    {
        Scanner scanner = collider.GetComponent<Scanner>();
        float distance = Vector3.Distance(transform.position, collider.transform.position);
        if (scanner)
        {
            if (distance > scanner.scan_range && distance > passive_range)
            {
                scanner.RemoveTarget(this);
                _detected_scanners.Remove(scanner);
            }
        }
    }
}
