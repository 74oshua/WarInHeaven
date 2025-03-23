using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Targetable))]
public class TelemetryVisualizer : MonoBehaviour
{
    public HUDComponent hud;
    public ArrowIcon arrow;
    public Targetable reference;

    private VectorIndicator _velocity_indicator = null;
    private VectorIndicator _acceleration_indicator = null;
    private Targetable _self_target;

    void Start()
    {
        _self_target = GetComponent<Targetable>();
        if (!hud)
        {
            Debug.LogError("TelemetryVisualizer needs HUD!");
            return;
        }

        _velocity_indicator = hud.AddVectorIndicator(_self_target, arrow, Color.green, 10);
        _acceleration_indicator = hud.AddVectorIndicator(_self_target, arrow, Color.red, 10);
    }

    void Update()
    {
        if (_velocity_indicator == null || !reference)
        {
            return;
        }

        _velocity_indicator.UpdatePosition();
        _velocity_indicator.UpdateVector(_self_target.ob.state.velocity - reference.ob.state.velocity);

        _acceleration_indicator.UpdatePosition();
        _acceleration_indicator.UpdateVector(_self_target.ob.GetAcceleration() * 5);
    }

    void OnDestroy()
    {
        hud.RemoveIndicator(_velocity_indicator);
    }
}
