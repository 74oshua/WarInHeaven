using UnityEngine;

public class AutoAccelSpacecraftController : SpacecraftController
{
    public float enginePower = 1f;

    protected override void Init()
    {
        _sc.SetMainThrottle(enginePower);
    }

    void FixedUpdate()
    {
        RotateTo(Vector3.up);
    }
}
