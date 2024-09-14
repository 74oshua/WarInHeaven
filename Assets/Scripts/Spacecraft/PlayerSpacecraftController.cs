using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerSpacecraftController : SpacecraftController
{
    // actions to be used for player control
    public InputActionAsset input_actions;

    // dampening factor
    public float dampening = 1;

    public float roll_factor = 20;

    // rotate action
    private InputAction _rotate_action;

    new void Start()
    {
        base.Start();

        input_actions.FindActionMap("flight").FindAction("burn").started += OnBurn;
        input_actions.FindActionMap("flight").FindAction("burn").canceled += OnBurn;

        input_actions.FindActionMap("flight").FindAction("cycle_target").started += CycleTarget;
        input_actions.FindActionMap("flight").FindAction("forward_target").performed += ForwardTarget;

        // _rotate_action = input_actions.FindActionMap("flight").FindAction("rotate");
    }

    void FixedUpdate()
    {
        Vector3 rotation_axis = input_actions.FindActionMap("flight").FindAction("rotate").ReadValue<Vector3>();
        Vector3 new_rotation_axis = new Vector3(-rotation_axis.z, rotation_axis.y, rotation_axis.x);

        // rotate to axis based on camera direction if main camera is set
        if (GameManager.Instance.main_camera)
        {
            RotateTo(GameManager.Instance.main_camera.transform.TransformDirection(new_rotation_axis));
        }
        // else rotate relative to world coordinates
        else
        {
            RotateTo(new_rotation_axis);
        }
        
        float roll_value = input_actions.FindActionMap("flight").FindAction("roll").ReadValue<float>();
        _sc.RotateRelative(new Vector3(0, roll_value * roll_factor, 0));

        Vector3 translation_axis = input_actions.FindActionMap("flight").FindAction("translate").ReadValue<Vector3>();
        Vector3 new_translation_axis = new Vector3(translation_axis.x, translation_axis.y, translation_axis.z);
        // translate to axis based on camera direction if main camera is set
        if (GameManager.Instance.main_camera)
        {
            _sc.Translate(GameManager.Instance.main_camera.transform.TransformDirection(new_translation_axis));
        }
        // else translate relative to world coordinates
        else
        {
            _sc.Translate(new_translation_axis);
        }

        // _sc.RotateRelative(rotation_axis - _sc.transform.InverseTransformDirection(_sc.GetComponent<Rigidbody>().angularVelocity) * dampening);

        // if (rotation_axis.magnitude == 0)
        // {
        //     RotateTo(_sc.transform.up, 1);
        // }
    }

    void OnEnable()
    {
        input_actions.FindActionMap("flight").Enable();
        _rotate_action = input_actions.FindActionMap("flight").FindAction("rotate");
    }

    void OnDisable()
    {
        input_actions.FindActionMap("flight").Disable();
    }

    void OnBurn(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _sc.SetMainThrottle(1);
        }
        else if (context.canceled)
        {
            _sc.SetMainThrottle(0);
        }
    }

    void CycleTarget(InputAction.CallbackContext context)
    {
        Scanner scanner = GetComponent<Scanner>();

        if (scanner)
        {
            scanner.CycleTarget();
        }
    }

    void ForwardTarget(InputAction.CallbackContext context)
    {
        Scanner scanner = GetComponent<Scanner>();

        if (scanner)
        {
            scanner.SelectForwardTarget();
        }
    }

    // private void OnRotate(InputAction.CallbackContext context)
    // {
    //     _heading = Quaternion.Euler(context.action.ReadValue<Vector3>()) * _sc.transform.up;
    // }
}
