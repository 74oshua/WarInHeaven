using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Thruster : SpacecraftPart
{
    public float max_thrust;
    public float spool_time = 1;
    // public bool maneuver = true;

    // from 0 to 1, current target throttle
    private float _throttle = 0;
    private float _target_throttle = 0;

    // thruster plume
    private ThrustPlume _plume;

    // audio source
    private AudioSource _audio;

    // thruster audio
    public AudioClip audio_thruster_active;
    
    // volume multiplier
    public float audio_volume = 1f;

    public void Start()
    {
        _plume = GetComponent<ThrustPlume>();
        _audio = GetComponent<AudioSource>();

        if (audio_thruster_active)
        {
            _audio.clip = audio_thruster_active;
            _audio.volume = 0;
            _audio.loop = true;
        }
    }

    public void FixedUpdate()
    {
        if (spool_time != 0)
        {
            _throttle += (_target_throttle - _throttle) / (spool_time / GameManager.Instance.fixedTimestep);
        }
        else
        {
            _throttle = _target_throttle;
        }
        Thrust();
    }

    public void Update()
    {
        Debug.DrawLine(transform.position, transform.position + transform.up * Mathf.Clamp(_throttle, 0, 1) * 10, Color.red);
    }

    protected virtual void Thrust()
    {
        _throttle = Mathf.Clamp(_throttle, 0, 1);
        if (_throttle > 0)
        {
            _rb.AddForceAtPosition(_throttle * max_thrust * transform.up, transform.position);
        }
        
        // animate thruster plume if one is attached
        if (_plume)
        {
            _plume.SimulatePlume(_throttle);
        }

        // play thruster audio if availible, volume dependent on throttle
        if (audio_thruster_active && _throttle > 0)
        {
            if (!_audio.isPlaying)
            {
                _audio.Play();
            }
            _audio.volume = _throttle * audio_volume;
        }
        else if (_audio.isPlaying)
        {
            _audio.Stop();
        }

        // resets throttle. throttle must be set/added to every FixedUpdate
        SetThrottle(0);
    }

    public virtual void SetThrottle(float throttle)
    {
        // _throttle = throttle;
        _target_throttle = throttle;
    }

    public virtual void AddThrottle(float value)
    {
        // _throttle += value;
        _target_throttle += value;
    }

    // causes the thruster to fire at whatever throttle would translate the parent craft in the specified direction
    public virtual void Translate(Vector3 direction)
    {
        // position relative to the center of the ship
        Vector3 rel_position = transform.position - (_sc.transform.position + _sc.transform.rotation * _rb.centerOfMass);

        // Vector3 para_component = Vector3.Project(transform.up, rel_position.normalized);
        // Vector3 perp_component = transform.up - para_component;

        float target_throttle = Vector3.Dot(transform.up, direction.normalized);
        
        AddThrottle(target_throttle);
    }

    // causes the thruster to fire at whatever throttle would add a rotation around axis to a ship with a center of mass at origin
    public virtual void Rotate(Vector3 axis)
    {
        // position relative to the center of the ship
        Vector3 rel_position = transform.position - (_sc.transform.position + _sc.transform.rotation * _rb.centerOfMass);

        // parallel component of force
        Vector3 linear_force = Vector3.Project(transform.up * max_thrust, -rel_position.normalized);

        // ideal direction to thrust given thruster's position
        Vector3 thrust_vector = Vector3.Cross(axis, rel_position.normalized);
        AddThrottle(Mathf.Cos(Mathf.Deg2Rad * Vector3.Angle(transform.up, thrust_vector)) * thrust_vector.magnitude / linear_force.magnitude);
    }
}
