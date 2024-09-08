using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scanner : MonoBehaviour
{
    // hud target indicators should be drawn on
    public HUDComponent hud;
    public float scan_range = 10000;

    private List<Targetable> _visible_targets = new List<Targetable>();
    private SphereCollider _trigger;
    private Targetable _primary_target = null;
    private int _primary_target_index = -1;
    private OrbitalBody _ob;
    private List<Vector3> _path = new List<Vector3>();

    void Start()
    {
        // add trigger collider to parent object
        _trigger = gameObject.AddComponent<SphereCollider>();
        _trigger.isTrigger = true;
        _trigger.radius = scan_range;
        _ob = gameObject.GetComponent<OrbitalBody>();
    }

    // private IEnumerator CalcPath()
    // {
    //     List<OrbitalBody> attractors = new List<OrbitalBody>();
    //     foreach (Targetable t in _visible_targets)
    //     {
    //         if (!t.ob.attractor)
    //         {
    //             continue;
    //         }
    //         OrbitalBody attractor = t.ob;
    //         attractors.Add(attractor);
    //     }
    //     OrbitalBody.BodyState curr_state = _ob.state;

    //     OrbitalBody.BodyState target_state = _primary_target.ob.state;

    //     List<OrbitalBody> other_attractors = new List<OrbitalBody>(attractors);
    //     other_attractors.Remove(_primary_target.ob);

    //     List<Vector3> path = new();
    //     float rolling_offset = path_spacing - Time.time % path_spacing;
    //     float timestamp = 0;
    //     float exitTime;
    //     float deltaTime;

    //     OrbitalBody.BodyState future_state = new OrbitalBody.BodyState(curr_state);
    //     OrbitalBody.BodyState target_future_state = new OrbitalBody.BodyState(target_state);
        
    //     // first prediction length changes to cause rolling effect
    //     future_state = OrbitalBody.PredictState(future_state, attractors, rolling_offset, 1, timestamp);
    //     target_future_state = OrbitalBody.PredictState(target_future_state, other_attractors, rolling_offset, 1, timestamp);
    //     path.Add(future_state.position - target_future_state.position);
    //     timestamp += rolling_offset;

    //     for (int i = 0; i < path_length; i++)
    //     {
    //         // calulate future positions for both target and scanner and add the relative position of the scanner to the path
    //         future_state = OrbitalBody.PredictState(future_state, attractors, 1f / path_resolution, Mathf.RoundToInt(path_resolution * path_spacing), timestamp);
    //         target_future_state = OrbitalBody.PredictState(target_future_state, other_attractors, 1f / path_resolution, Mathf.RoundToInt(path_resolution * path_spacing), timestamp);
    //         path.Add(future_state.position - target_future_state.position);
    //         timestamp += Mathf.RoundToInt(path_resolution * path_spacing) / path_resolution;

    //         if (i % calc_speed == 0)
    //         {
    //             exitTime = Time.time;
    //             yield return new WaitForEndOfFrame();

    //             deltaTime = Time.time - exitTime;
    //             timestamp -= deltaTime;
    //         }
    //     }

    //     _path.Clear();
    //     _path = new(path);
    //     hud.DrawPath(_path, _primary_target.transform, 0.5f);

    //     calculating_path = false;
    //     _path_finished_time = Time.time;
    // }

    // private void SetDrawPath(bool should_draw)
    // {
    //     drawing_path = should_draw;
    //     hud.SetPathVisible(should_draw);
    // }

    // public void Update()
    // {
    //     CameraController cameraController = GameManager.Instance.main_camera.GetComponent<CameraController>();
    //     if (cameraController && GameManager.Instance.in_overview != drawing_path)
    //     {
    //         SetDrawPath(cameraController.in_overview);
    //     }

    //     if (drawing_path && !calculating_path && Time.time - _path_finished_time > path_update_interval && hud && _primary_target && _ob)
    //     {
    //         StartCoroutine(CalcPath());
    //         calculating_path = true;
    //     }
    // }

    void OnTriggerEnter(Collider collider)
    {
        Targetable target = collider.gameObject.GetComponent<Targetable>();
        if (target)
        {
            AddTarget(target);
        }
    }

    void OnTriggerExit(Collider collider)
    {
        Targetable target = collider.GetComponent<Targetable>();
        if (target)
        {
            RemoveTarget(target);
        }
    }

    public void AddTarget(Targetable target)
    {
        // return if target is already visible
        if (_visible_targets.Contains(target))
        {
            return;
        }

        _visible_targets.Add(target);
        
        if (hud)
        {
            hud.AddTarget(target);
        }
    }

    public void RemoveTarget(Targetable target)
    {
        // return if target is already not visible
        if (!_visible_targets.Contains(target))
        {
            return;
        }

        // return if we're still in active or passive range
        float distance = Vector3.Distance(target.transform.position, transform.position);
        if (distance <= scan_range || distance <= target.passive_range)
        {
            return;
        }
        
        _visible_targets.Remove(target);

        if (hud)
        {
            hud.RemoveTarget(target);

            // unset target if it has gone out of range
            if (target == _primary_target)
            {
                _primary_target = null;
            }
        }
    }

    public void CycleTarget()
    {
        _primary_target_index = (_primary_target_index + 1) % _visible_targets.Count;
        SelectTarget(_visible_targets[_primary_target_index]);
    }

    private void SelectTarget(Targetable target)
    {
        _primary_target = target;
        if (hud)
        {
            hud.SelectTarget(target);
        }
    }

    public void SelectForwardTarget()
    {
        Vector3 facing = GameManager.Instance.main_camera.transform.forward;
        for (int i = 0; i < _visible_targets.Count; i++)
        {
            if (_primary_target != _visible_targets[i] && Vector3.Dot((_visible_targets[i].transform.position - transform.position).normalized, facing.normalized) > 0.9f)
            {
                SelectTarget(_visible_targets[i]);
                break;
            }
        }
    }

    public List<Targetable> GetVisibleTargets(TargetType types)
    {
        List<Targetable> targets = new();
        foreach (Targetable t in _visible_targets)
        {
            if ((t.type & types) != 0)
            {
                targets.Add(t);
            }
        }
        return targets;
    }

    public Targetable GetPrimaryTarget()
    {
        return _primary_target;
    }
}
