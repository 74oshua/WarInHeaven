using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class HUDComponent : MonoBehaviour
{
    private UIDocument _ui;

    // tracking indicators for every target in range
    private List<Indicator> _target_indicators = new List<Indicator>();
    // private Targetable primaryTarget = null;

    private Indicator _prograde;
    // private List<Indicator> _path = new List<Indicator>();
    private PathIndicator _path;

    public Targetable pov;
    private Targetable _reference;

    public Color neutral_color = Color.white;
    public Color targeted_color = Color.red;
    public Color reference_color = Color.cyan;
    public Color instrument_color = Color.green;

    public float satellite_indicator_size = 8;
    public float planet_indicator_size = 10;
    public float instrument_indicator_size = 5;
    public float indicator_frame_width = 1;

    public Color near_path_indicator_color = Color.magenta;
    public Color far_path_indicator_color = Color.blue;
    public float path_indicator_width = 12;

    public OverviewIcon spacecraft_icon;
    public float spacecraft_icon_width = 10;
    
    public LineSegment trajectoryLine;

    // private bool _refresh = false;

    void Awake()
    {
        _ui = GetComponent<UIDocument>();
    }

    void Start()
    {

        // add prograde indicator
        _prograde = new SquareIndicator(_ui, pov, instrument_color, instrument_indicator_size, indicator_frame_width);
        _path = new PathIndicator(_ui, trajectoryLine, new(), pov, near_path_indicator_color, far_path_indicator_color, path_indicator_width);

        SetProgradeVisible(false);
        // _prograde.SetVisible
        // _ui.rootVisualElement.Add(_prograde);
    }

    void LateUpdate()
    {
        // if (_refresh)
        // {
            // SetProgradeVisible(primaryTarget != null);
        // }

        // for (int i = 0; i < _target_indicators.Count; i++)
        // {
        //     _target_indicators[i].UpdatePosition();

        //     if (_refresh)
        //     {
        //         if (_target_indicators[i].target == primaryTarget)
        //         {
        //             _target_indicators[i].ChangeColor(targeted_color);
        //         }
        //         else
        //         {
        //             _target_indicators[i].ChangeColor(neutral_color);
        //         }
        //     }
        // }
        // _prograde.UpdatePosition();
        // foreach (Indicator indicator in _path)
        // {
        //     indicator.UpdatePosition();
        // }
        for (int i = 0; i < _target_indicators.Count; i++)
        {
            if (_target_indicators[i].target == null)
            {
                RemoveTarget(_target_indicators[i].target);
            }
        }
        foreach (Indicator indicator in _ui.rootVisualElement.Children())
        {
            if (indicator.valid)
            {
                indicator.UpdatePosition();
            }
        }

        // _refresh = false;
    }

    public Indicator AddTarget(Targetable target)
    {
        // Indicator indicator = new Indicator(_ui, target, neutral_color, satellite_indicator_size, indicator_frame_width);
        Indicator indicator = null;

        switch (target.type)
        {
        case TargetType.Planet:
            indicator = new OctIndicator(_ui, target, neutral_color, planet_indicator_size, indicator_frame_width);
            _target_indicators.Add(indicator);
            break;
        case TargetType.Satellite:
            indicator = new SquareIndicator(_ui, target, neutral_color, satellite_indicator_size, indicator_frame_width)
            {
                hide_when_occluded = false,
                occlusion_mask = 0
            };
            _target_indicators.Add(indicator);
            break;
        case TargetType.Projectile:
            indicator = new DiamondIndicator(_ui, target, neutral_color, satellite_indicator_size, indicator_frame_width)
            {
                hide_when_occluded = false,
                occlusion_mask = 0
            };
            _target_indicators.Add(indicator);
            break;
        case TargetType.Spacecraft:
            if (spacecraft_icon)
            {
                indicator = new MeshIndicator(_ui, spacecraft_icon, target, neutral_color, false, spacecraft_icon_width, indicator_frame_width);
                _target_indicators.Add(indicator);

                // if (target != pov)
                // {
                //     indicator = new SquareIndicator(_ui, target, neutral_color, satellite_indicator_size, indicator_frame_width)
                //     {
                //         realview_only = true,
                //         hide_when_occluded = false,
                //         occlusion_mask = 0
                //     };
                //     _target_indicators.Add(indicator);
                // }
            }
            else if (target != pov)
            {
                    indicator = new SquareIndicator(_ui, target, neutral_color, satellite_indicator_size, indicator_frame_width)
                    {
                        hide_when_occluded = false,
                        occlusion_mask = 0
                    };
                    _target_indicators.Add(indicator);
            }
            break;
            
        default:
            indicator = new SquareIndicator(_ui, target, neutral_color, planet_indicator_size, indicator_frame_width);
            _target_indicators.Add(indicator);
            break;
        }

        return indicator;
    }

    public void RemoveTarget(Targetable target)
    {
        List<Indicator> indicators = GetTargetIndicators(target);
        foreach (Indicator indicator in indicators)
        {
            // if (primaryTarget == indicator.target)
            // {
            //     primaryTarget = null;
            //     _refresh = true;
            // }

            indicator.SetEnabled(false);
            indicator.RemoveFromHierarchy();
            _target_indicators.Remove(indicator);
        }
    }

    public List<Indicator> GetTargetIndicators(Targetable target)
    {
        List<Indicator> indicators = new();
        foreach (Indicator indicator in _target_indicators)
        {
            if (target == indicator.target)
            {
                indicators.Add(indicator);
            }
        }
        return indicators;
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

    public void SetTargetColor(Targetable target)
    {
        foreach (Indicator indicator in GetTargetIndicators(target))
        {
            indicator.ChangeColor(targeted_color);
        }
    }

    public void SetNeutralColor(Targetable target)
    {
        foreach (Indicator indicator in GetTargetIndicators(target))
        {
            indicator.ChangeColor(neutral_color);
        }
    }

    public void SetReferenceColor(Targetable target)
    {
        foreach (Indicator indicator in GetTargetIndicators(target))
        {
            indicator.ChangeColor(reference_color);
        }
    }

    public void DrawPath(List<Vector3> positions, Targetable reference, float lerp_factor = 1)
    {
        _path.UpdatePath(positions, reference, lerp_factor);
        _path.visible = true;
        // _path.SetPosition(pov.transform.position, reference);
        // _path.UpdatePosition();

        // make sure the correct number of indicators are displayed
        // if (positions.Count != _path.Count)
        // {
        //     _path.Clear();
        //     for (int i = 0; i < positions.Count; i++)
        //     {
        //         _path.Add(new OctIndicator(_ui, reference, Color.Lerp(near_path_indicator_color, far_path_indicator_color, i / (float)positions.Count), Mathf.Max(path_indicator_max_width - i * path_indicator_delta_width, path_indicator_min_width), indicator_frame_width));
        //         _path[i].hide_when_occluded = true;
        //         _path[i].occlusion_mask = LayerMask.GetMask("CelestialBody");
        //         _path[i].SendToBack();
        //     }
        // }

        // for (int i = 0; i < positions.Count; i++)
        // {
        //     _path[i].SetPosition(positions[i], reference, lerp_factor);
        // }
    }

    public void SetPathValid(bool valid)
    {
        // foreach (Indicator indicator in _path)
        // {
        //     indicator.SetEnabled(visible);
        // }
        _path.SetValid(valid);
    }

    public VectorIndicator AddVectorIndicator(Targetable reference, ArrowIcon arrow, Color color, float width = 1)
    {
        VectorIndicator indicator = new VectorIndicator(_ui, arrow, reference, color, width);
        return indicator;
    }

    public void RemoveIndicator(Indicator indicator)
    {
        indicator.SetEnabled(false);
        indicator.RemoveFromHierarchy();
    }


}
