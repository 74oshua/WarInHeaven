using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(OnRailsBody))]
public class OnRailsOriginShiftController : OriginShiftController
{
    private OnRailsBody _orb;

    protected override void Start()
    {
        base.Start();

        _orb = GetComponent<OnRailsBody>();
    }

    public override void Shift(Vector3 pos_offset, Vector3 vel_offset)
    {
        _orb.Shift(pos_offset, vel_offset);
    }
}
