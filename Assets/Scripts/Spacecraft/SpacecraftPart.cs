using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpacecraftPart : MonoBehaviour
{
    protected Rigidbody _rb;
    protected Spacecraft _sc;

    // sets which spacecraft this part belongs to
    virtual public void SetSpacecraft(Spacecraft sc)
    {
        _sc = sc;

        _rb = sc.GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.drag = 0;
        _rb.angularDrag = 0;
    }
}
