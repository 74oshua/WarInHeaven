using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gimbal : MonoBehaviour
{
    public Transform x_axis = null;
    public Transform y_axis = null;
    public Transform emitter = null;

    // public Targetable target;
    protected Vector3 _target_point;
    public float tracking_speed = 1;
    public float tracking_smooth_factor = 40;

    public float x_min_angle = 0;
    public float x_max_angle = 0;
    public float y_min_angle = -180;
    public float y_max_angle = 180;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // takes an angle in degrees, outputs the same angle between -180 and 180
    private float RestrictAngle(float angle)
    {
        float r = angle % 360;
        r = r > 180 ? r - 360 : r;
        return r;
    }

    void FixedUpdate()
    {
        // if (!target)
        // {
        //     return;
        // }

        // _target_position = CalculateIntercept(target.transform.position, target.ob.state.velocity, emitter.transform.position, 50);

        for (int i = 0; i < tracking_smooth_factor; i++)
        {
            float x_difference = Vector3.SignedAngle(emitter.transform.up, _target_point, x_axis.transform.up);
            float y_difference = Vector3.SignedAngle(emitter.transform.up, _target_point, y_axis.transform.right);

            // float x_factor = Mathf.Abs(x_difference) / tracking_smooth_factor * tracking_speed;
            // float y_factor = Mathf.Abs(y_difference) / tracking_smooth_factor * tracking_speed;
            float x_factor = tracking_speed / tracking_smooth_factor;
            float y_factor = tracking_speed / tracking_smooth_factor;
            // x_axis.Rotate(new Vector3(0, Mathf.Clamp(x_difference, -x_factor, x_factor), 0), Space.Self);
            // y_axis.Rotate(new Vector3(Mathf.Clamp(y_difference, -y_factor, y_factor), 0, 0), Space.Self);

            float x_angle = x_axis.localEulerAngles.y + Mathf.Clamp(x_difference, -x_factor, x_factor);
            float y_angle = y_axis.localEulerAngles.x + Mathf.Clamp(y_difference, -y_factor, y_factor);

            // clamp rotation
            x_axis.localRotation = Quaternion.Euler(0, Mathf.Clamp(RestrictAngle(x_angle), x_min_angle, x_max_angle), 0);
            y_axis.localRotation = Quaternion.Euler(Mathf.Clamp(RestrictAngle(y_angle), y_min_angle, y_max_angle), 0, 0);
        }
    }

    public bool IsAligned(float accuracy)
    {
        return Vector3.Dot(emitter.up, _target_point.normalized) >= accuracy;
    }

    public void SetTargetPoint(Vector3 point)
    {
        _target_point = point;
    }
}
