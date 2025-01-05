using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    // camera target object
    public GameObject target;
    public Transform up_reference;

    // input actions used for camera movement
    public InputActionAsset input_actions;

    // constraints for camera
    public float min_radius = 10;
    public float max_radius = 100;
    public float overview_distance = 100;
    public float rotation_sensitivity = 0.5f;
    public float zoom_sensitivity = 1;

    private float _radius = 50;
    private float _distance_modifier = 1;
    private Vector3 _direction = Vector3.back;
    private float _pitch;
    private float _yaw;
    private Vector3 _up = Vector3.up;
    private bool _rotating = false;

    private Camera _camera;

    public bool in_overview
    {
        get { return _distance_modifier != 1; }
    }

    public float zoom
    {
        get { return _radius * _distance_modifier; }
    }

    // private InputAction _toggle_action;

    // action containing value to rotate
    private InputAction _rotate_action;

    // action containing value to zoom
    private InputAction _zoom_action;

    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponent<Camera>();

        if (input_actions == null)
        {
            Debug.LogError("CameraController missing input_actions!");
            Destroy(this);
            return;
        }

        // save rotation action
        _rotate_action = input_actions.FindActionMap("camera").FindAction("rotate");
        _zoom_action = input_actions.FindActionMap("camera").FindAction("zoom");
        // _toggle_action = input_actions.FindActionMap("camera").FindAction("toggle");

        // assign button to toggle camera movement to OnToggle()
        input_actions.FindActionMap("camera").FindAction("toggle").started += OnToggle;
        input_actions.FindActionMap("camera").FindAction("toggle").canceled += OnToggle;

        // save overview toggle
        input_actions.FindActionMap("camera").FindAction("toggle_overview").started += OnToggleOverview;
        
        // input_actions.FindActionMap("camera").FindAction("zoom").performed += OnZoom;
        // input_actions.FindActionMap("camera").FindAction("rotate").performed += OnRotate;
    }

    void OnEnable()
    {
        input_actions.FindActionMap("camera").Enable();
    }

    void OnDisable()
    {
        input_actions.FindActionMap("camera").Disable();
    }

    // Update is called once per frame
    void Update()
    {
        // if the toggle is held, rotate the camera
        if (_rotating)
        {
            Vector2 rotation = _rotate_action.ReadValue<Vector2>();

            // _up = transform.up;

            // pitch
            float old_pitch = _pitch;
            _pitch += rotation.y * rotation_sensitivity;
            _pitch = Mathf.Clamp(_pitch, -89, 89);
            _direction = Quaternion.AngleAxis(_pitch - old_pitch, -transform.right) * _direction;
            // _direction = Quaternion.AngleAxis(rotation.y * rotation_sensitivity, -transform.right) * _direction;

            // yaw
            float old_yaw = _yaw;
            _yaw += rotation.x * rotation_sensitivity;
            while (_yaw >= 360f)
            {
                _yaw -= 360f;
            }
            while (_yaw < 0)
            {
                _yaw += 360f;
            }
            _yaw = Mathf.Clamp(_yaw, 0, 360);
            _direction = Quaternion.AngleAxis(_yaw - old_yaw, _up) * _direction;
            // _direction = Quaternion.AngleAxis(rotation.x * rotation_sensitivity, _up) * _direction;
        }

        // zoom
        _radius += _zoom_action.ReadValue<float>() * zoom_sensitivity * _distance_modifier;
        _radius = Mathf.Clamp(_radius, min_radius * _distance_modifier, max_radius * _distance_modifier);

        // if (up_reference)
        // {
        //     _up = (target.transform.position - up_reference.transform.position).normalized;

        //     float pitch_angle = Vector3.Angle(-transform.forward, _up);
        //     float min_pitch = 3;
        //     float max_pitch = 177;
        //     if (pitch_angle + _pitch < min_pitch)
        //     {
        //         _pitch = min_pitch - pitch_angle;
        //     }
        //     else if (pitch_angle + _pitch > max_pitch)
        //     {
        //         _pitch = max_pitch - pitch_angle;
        //     }

        //     _direction = Quaternion.AngleAxis(_pitch, -transform.right) * _direction;
        //     _direction = Quaternion.AngleAxis(_yaw, _up) * _direction;
        //     _direction = _direction.normalized;

        //     _pitch = 0;
        //     _yaw = 0;
        // }
        // else
        // {
        //     _direction = target.transform.up;
        //     _direction = Quaternion.AngleAxis(_pitch, -target.transform.right) * _direction;
        //     _direction = Quaternion.AngleAxis(_yaw, target.transform.up) * _direction;

        //     _pitch = 0;
        //     _yaw = 0;
        // }

        transform.position = target.transform.position + _direction * _radius * _distance_modifier;
        transform.LookAt(target.transform.position, _up);
    }

    public void SetUp(Vector3 up)
    {
        _up = up;
    }

    private void OnToggle(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _rotating = true;
        }
        else if (context.canceled)
        {
            _rotating = false;
        }
    }

    private void OnToggleOverview(InputAction.CallbackContext context)
    {
        if (in_overview)
        {
            _distance_modifier = 1;
        }
        else
        {
            _distance_modifier = overview_distance;
        }
        GameManager.Instance.SetInOverview(in_overview);

        if (in_overview)
        {
            _camera.cullingMask = (1 << LayerMask.NameToLayer("CelestialBody")) | (1 << LayerMask.NameToLayer("Overview")) | (1 << LayerMask.NameToLayer("Indicator"));
        }
        else
        {
            _camera.cullingMask = ~(1 << LayerMask.NameToLayer("Overview"));
        }
    }

    private void OnZoom(InputAction.CallbackContext context)
    {
        _radius += context.action.ReadValue<float>() * zoom_sensitivity;
        _radius = Mathf.Clamp(_radius, min_radius, max_radius);
    }

    private void OnRotate(InputAction.CallbackContext context)
    {
        // calculate camera movement
        Vector2 rotation = context.action.ReadValue<Vector2>();

        // if the toggle is held, rotate the camera
        if (_rotating)
        {
            // pitch
            _direction = Quaternion.AngleAxis(rotation.y * rotation_sensitivity, -transform.right) * _direction;

            // yaw
            _direction = Quaternion.AngleAxis(rotation.x * rotation_sensitivity, _up) * _direction;
        }
    }
}
