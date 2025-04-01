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
    private Targetable _reference_target = null;
    private int _primary_target_index = -1;
    private OrbitalBody _ob;
    private Targetable _search_target;

    void Start()
    {
        // add trigger collider to parent object
        _trigger = gameObject.AddComponent<SphereCollider>();
        _trigger.isTrigger = true;
        _trigger.radius = scan_range;
        _ob = gameObject.GetComponent<OrbitalBody>();
        
        // add an indicator for ourselves if we are targetable
        Targetable t = GetComponent<Targetable>();
        if (hud && t)
        {
            hud.AddTarget(t);
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.isTrigger || Vector3.Distance(collider.transform.position, transform.position) > scan_range)
        {
            return;
        }

        Targetable target = collider.gameObject.GetComponent<Targetable>();
        if (target)
        {
            AddTarget(target);
        }
    }

    void OnTriggerExit(Collider collider)
    {
        // Debug.Log(collider.name);
        Targetable target = collider.GetComponent<Targetable>();
        if (target)
        {
            RemoveTarget(target);
        }
    }

    public void AddTarget(Targetable target)
    {
        float distance = Vector3.Distance(target.transform.position, transform.position);
        if (distance > scan_range && distance > target.passive_range)
        {
            return;
        }

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

        if (target == _search_target)
        {
            SelectTarget(target);
        }
    }

    // force parameter guarentees removal
    public void RemoveTarget(Targetable target, bool force = false)
    {
        if (!_visible_targets.Contains(target))
        // return if target is already not visible
        {
            return;
        }

        // return if we're still in active or passive range (ignore if force is true)
        float distance = Vector3.Distance(target.transform.position, transform.position);
        if (!force && (distance < scan_range || distance <= target.passive_range))
        {
            return;
        }

        // if primary target, reset primary target to null
        if (target == _primary_target)
        {
            _primary_target = null;
            _primary_target_index = -1;
        }

        if (target == _reference_target)
        {
            _reference_target = null;
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

    public void ClearTargets()
    {
        for (int i = _visible_targets.Count-1; i >= 0; i--)
        {
            RemoveTarget(_visible_targets[i]);
        }
    }

    public void CycleTarget()
    {
        _primary_target_index = (_primary_target_index + 1) % _visible_targets.Count;
        SelectTarget(_visible_targets[_primary_target_index]);
    }

    public void SelectTarget(Targetable target)
    {
        if (!_visible_targets.Contains(target))
        {
            Debug.Log(name);
            foreach (Targetable t in _visible_targets)
            {
                Debug.Log(t.name);
            }
            return;
        }
        
        Targetable _prev_target = _primary_target;
        _primary_target = target;
        _primary_target_index = _visible_targets.IndexOf(target);
        UpdateColor(_primary_target);
        UpdateColor(_prev_target);
        // if (hud)
        // {
        //     if (_prev_target)
        //     {
        //         hud.SetNeutralColor(_prev_target);
        //     }
            
        //     hud.SetTargetColor(target);
        // }
    }

    public void SetTargetAsReference()
    {
        Targetable _prev_reference = _reference_target;
        _reference_target = _primary_target;
        
        UpdateColor(_reference_target);
        UpdateColor(_prev_reference);
    }

    public void ClearReference()
    {
        Targetable _prev_reference = _reference_target;
        _reference_target = null;
        
        UpdateColor(_prev_reference);
    }

    public void UpdateColor(Targetable target)
    {
        if (!hud || !target)
        {
            return;
        }

        if (_primary_target == target)
        {
            hud.SetTargetColor(target);
        }
        else if (_reference_target == target)
        {
            hud.SetReferenceColor(target);
        }
        else
        {
            hud.SetNeutralColor(target);
        }
    }

    public void SelectCameraForwardTarget()
    {
        Vector3 facing = GameManager.Instance.main_camera.transform.forward;
        for (int i = _primary_target_index >= 0 ? _primary_target_index : 0; i < _visible_targets.Count + _primary_target_index + 1; i++)
        {
            int index = i % _visible_targets.Count;
            if (_primary_target != _visible_targets[index] && Vector3.Dot((_visible_targets[index].transform.position - transform.position).normalized, facing.normalized) > 0.95f)
            {
                SelectTarget(_visible_targets[index]);
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

    // use primary target as reference by default
    public Targetable GetReferenceTarget()
    {
        if (_reference_target != null)
        {
            return _reference_target;
        }

        return GetPrimaryTarget();
    }
    
    public void SetSearchTarget(Targetable search_target)
    {
        _search_target = search_target;
    }

    public void SetScanRange(float range)
    {
        scan_range = range;
        _trigger.radius = scan_range;
    }
}
