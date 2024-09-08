using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Multithruster : Thruster
{
    public List<Thruster> thrusters;

    // multithruster should produce no thrust of it's own
    protected override void Thrust()
    {
        return;
    }

    // set each child thruster's sc variable
    public override void SetSpacecraft(Spacecraft sc)
    {
        base.SetSpacecraft(sc);

        foreach (SpacecraftPart t in thrusters)
        {
            t.SetSpacecraft(sc);
        }
    }

    public override void SetThrottle(float throttle)
    {
        foreach (Thruster t in thrusters)
        {
            t.SetThrottle(throttle);
        }
    }

    public override void Translate(Vector3 direction)
    {
        foreach (Thruster t in thrusters)
        {
            t.Translate(direction);
        }
    }

    public override void Rotate(Vector3 axis)
    {
        foreach (Thruster t in thrusters)
        {
            t.Rotate(axis);
        }
    }
}
