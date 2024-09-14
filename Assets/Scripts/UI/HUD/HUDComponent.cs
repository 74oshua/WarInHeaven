using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class HUDComponent : MonoBehaviour
{
    private UIDocument _ui;

    // tracking indicators for every target in range
    private List<Indicator> _target_indicators = new List<Indicator>();
    private Targetable primaryTarget = null;

    private Indicator _prograde;
    private List<Indicator> _path = new List<Indicator>();

    public Targetable pov;

    public Color neutral_color = Color.white;
    public Color targeted_color = Color.red;
    public Color instrument_color = Color.green;

    public float satellite_indicator_size = 8;
    public float planet_indicator_size = 10;
    public float instrument_indicator_size = 5;
    public float indicator_frame_width = 1;

    public Color near_path_indicator_color = Color.magenta;
    public Color far_path_indicator_color = Color.blue;
    public float path_indicator_max_width = 12;
    public float path_indicator_min_width = 2;
    public float path_indicator_delta_width = 15;

    private bool _refresh = false;

    void Start()
    {
        _ui = GetComponent<UIDocument>();

        // add prograde indicator
        _prograde = new Indicator(_ui, pov, instrument_color, instrument_indicator_size, indicator_frame_width);

        SetProgradeVisible(false);
        // _prograde.SetVisible
        // _ui.rootVisualElement.Add(_prograde);
    }

    void LateUpdate()
    {
        if (_refresh)
        {
            // SetProgradeVisible(primaryTarget != null);
        }

        for (int i = 0; i < _target_indicators.Count; i++)
        {
            _target_indicators[i].UpdatePosition();

            if (_refresh)
            {
                if (_target_indicators[i].target == primaryTarget)
                {
                    _target_indicators[i].color = targeted_color;
                }
                else
                {
                    _target_indicators[i].color = neutral_color;
                }
                _target_indicators[i].MarkDirtyRepaint();
            }
        }
        _prograde.UpdatePosition();
        foreach (Indicator indicator in _path)
        {
            indicator.UpdatePosition();
        }

        _refresh = false;
    }

    public Indicator AddTarget(Targetable target)
    {
        Indicator indicator = new Indicator(_ui, target, neutral_color, satellite_indicator_size, indicator_frame_width);

        switch (target.type)
        {
        case TargetType.Planet:
            indicator = new OctIndicator(_ui, target, neutral_color, planet_indicator_size, indicator_frame_width);
            break;
        case TargetType.Satellite:
            indicator = new Indicator(_ui, target, neutral_color, satellite_indicator_size, indicator_frame_width);
            break;
            
        default:
            indicator = new Indicator(_ui, target, neutral_color, planet_indicator_size, indicator_frame_width);
            break;
        }

        _target_indicators.Add(indicator);

        return indicator;
    }

    public void RemoveTarget(Targetable target)
    {
        Indicator indicator = GetTargetIndicator(target);
        if (indicator != null)
        {
            if (primaryTarget == indicator.target)
            {
                primaryTarget = null;
                _refresh = true;
            }

            indicator.SetEnabled(false);
            indicator.RemoveFromHierarchy();
            _target_indicators.Remove(indicator);
        }
    }

    public Indicator GetTargetIndicator(Targetable target)
    {
        foreach (Indicator indicator in _target_indicators)
        {
            if (target == indicator.target)
            {
                return indicator;
            }
        }
        return null;
    }

    public void SetProgradeIndicator(Vector3 pov, Vector3 direction)
    {
        _prograde.SetPosition(pov + direction.normalized * 10000000f);
    }

    public void SetProgradeIndicator(Vector3 position)
    {
        _prograde.SetPosition(position);
    }

    public void SetProgradeVisible(bool visible)
    {
        _prograde.SetEnabled(visible);
    }

    public void SelectTarget(Targetable target)
    {
        primaryTarget = target;
        _refresh = true;
    }

    public void DrawPath(List<Vector3> positions, Targetable reference = null, float lerp_factor = 1)
    {
        // make sure the correct number of indicators are displayed
        if (positions.Count != _path.Count)
        {
            _path.Clear();
            for (int i = 0; i < positions.Count; i++)
            {
                _path.Add(new OctIndicator(_ui, pov, Color.Lerp(near_path_indicator_color, far_path_indicator_color, i / (float)positions.Count), Mathf.Max(path_indicator_max_width - i * path_indicator_delta_width, path_indicator_min_width), indicator_frame_width));
                _path[i].occlude = true;
                _path[i].occlusion_mask = ~LayerMask.GetMask("Spacecraft", "Ignore Raycast");
                _path[i].SendToBack();
            }
        }

        for (int i = 0; i < positions.Count; i++)
        {
            _path[i].SetPosition(positions[i], reference, lerp_factor);
        }
    }

    public void SetPathVisible(bool visible)
    {
        foreach (Indicator indicator in _path)
        {
            indicator.SetEnabled(visible);
        }
    }
}
