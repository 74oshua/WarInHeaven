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
        if (!_visible_targets.Contains(target))
        {
            return;
        }
        
        _primary_target = target;
        _primary_target_index = _visible_targets.IndexOf(target);
        if (hud)
        {
            hud.SelectTarget(target);
        }
    }

    public void SelectForwardTarget()
    {
        Vector3 facing = GameManager.Instance.main_camera.transform.forward;
        for (int i = _primary_target_index >= 0 ? _primary_target_index : 0; i < _visible_targets.Count + _primary_target_index; i++)
        {
            int index = i % _visible_targets.Count;
            if (_primary_target != _visible_targets[index] && Vector3.Dot((_visible_targets[index].transform.position - transform.position).normalized, facing.normalized) > 0.9f)
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
}
