using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : SpacecraftPart
{
    public Transform emitter;
    public bool single_shot_per_group = false;

    protected Targetable _target;
    public int fire_group;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    abstract public bool Fire();

    public virtual void SetTarget(Targetable target)
    {
        _target = target;
    }

    // public void SetFireGroup(int fire_group)
    // {
    //     this.fire_group = fire_group;
    // }

    // public int GetFireGroup()
    // {
    //     return fire_group;
    // }
}
