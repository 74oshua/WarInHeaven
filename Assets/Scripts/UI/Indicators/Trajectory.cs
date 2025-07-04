using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Scanner))]
[RequireComponent(typeof(OrbitalBody))]
public class Trajectory : MonoBehaviour
{
    public HUDComponent hud;

    protected Scanner _scanner;
    protected OrbitalBody _ob;

    protected OrbitalBody _target;
    protected List<Vector3> _path = new();
    protected List<Vector3> _old_path = new();

    public float path_spacing = 1;
    public float path_resolution = 20;
    public int path_length = 20;
    public float path_update_interval = 1;
    public int calc_speed = 1;
    public float lerp_speed = 1;

    protected float _path_finished_time = 0;
    protected bool _calculating_path = false;
    protected bool _drawing_path = false;
    protected float _prev_offset = 0;
    protected bool _roll = false;

    // Start is called before the first frame update
    void Start()
    {
        _scanner = GetComponent<Scanner>();
        _ob = GetComponent<OrbitalBody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.in_overview != _drawing_path)
        {
            SetDrawPath(GameManager.Instance.in_overview);
        }

        if (_drawing_path && !_calculating_path && Time.time - _path_finished_time > path_update_interval && hud && _scanner.GetPrimaryTarget())
        {
            _target = _scanner.GetReferenceTarget().ob;

            StartCoroutine(CalcPath());
            _calculating_path = true;
        }
    }

    void LateUpdate()
    {

    }
    
    private IEnumerator CalcPath()
    {
        List<OrbitalBody> attractors = new List<OrbitalBody>();
        foreach (Targetable t in _scanner.GetVisibleTargets(TargetType.Planet))
        {
            if (!t.ob.attractor)
            {
                continue;
            }
            OrbitalBody attractor = t.ob;
            attractors.Add(attractor);
        }
        OrbitalBody.BodyState curr_state = _ob.state;

        OrbitalBody.BodyState target_state = _target.state;

        List<OrbitalBody> other_attractors = new List<OrbitalBody>(attractors);
        other_attractors.Remove(_target);

        List<Vector3> path = new();
        float rolling_offset = path_spacing - Time.time % path_spacing;

        if (rolling_offset > _prev_offset)
        {
            _roll = true;
        }
        _prev_offset = rolling_offset;

        float timestamp = 0;
        float exitTime;
        float deltaTime;

        OrbitalBody.BodyState future_state = new OrbitalBody.BodyState(curr_state);
        OrbitalBody.BodyState target_future_state = new OrbitalBody.BodyState(target_state);
        
        // first prediction length changes to cause rolling effect
        // future_state = OrbitalBody.PredictState(future_state, attractors, rolling_offset, 1, timestamp);
        // target_future_state = OrbitalBody.PredictState(target_future_state, other_attractors, rolling_offset, 1, timestamp);
        // path.Add(future_state.position - target_future_state.position);
        // timestamp += rolling_offset;

        path.Add(future_state.position - target_future_state.position + target_state.position);

        for (int i = 0; i < path_length; i++)
        {
            // calulate future positions for both target and scanner and add the relative position of the scanner to the path
            future_state = OrbitalBody.PredictState(future_state, attractors, 1f / path_resolution, Mathf.RoundToInt(path_resolution * path_spacing), timestamp);
            target_future_state = OrbitalBody.PredictState(target_future_state, other_attractors, 1f / path_resolution, Mathf.RoundToInt(path_resolution * path_spacing), timestamp);
            
            path.Add(future_state.position - target_future_state.position + target_state.position);

            timestamp += Mathf.RoundToInt(path_resolution * path_spacing) / path_resolution;

            if (i % calc_speed == 0)
            {
                exitTime = Time.time;
                yield return new();

                deltaTime = Time.time - exitTime;
                timestamp -= deltaTime;
            }
        }

        _path.Clear();
        _path = new(path);
        
        // if (_roll)
        // {
        //     hud.DrawPath(_path, _target.transform, 1f);
        //     _roll = false;
        // }
        // else
        // {
        if (_target)
        {
            hud.DrawPath(_path, _target.GetComponent<Targetable>(), lerp_speed);
        }
        else
        {
            hud.SetPathValid(false);
        }
        // }

        _calculating_path = false;
        _path_finished_time = Time.time;
    }

    public void SetDrawPath(bool should_draw)
    {
        _drawing_path = should_draw;
        hud.SetPathValid(should_draw);
    }
}
