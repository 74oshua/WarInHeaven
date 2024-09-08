using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TrackingIndicator : Indicator
{
    // private float _width;
    // private float _frame_width = 0.7f;

    protected Targetable _target;
    public Targetable target
    {
        get { return _target; }
    }

    public TrackingIndicator(UIDocument ui, Targetable target, Color color, float width = 100, float frame_width = 5) : base(ui, color, width, frame_width)
    {
        _target = target;
    }
    
    public override void UpdatePosition()
    {
        Vector3 position = _target.transform.position;
        // if (Vector3.Dot(GameManager.Instance.main_camera.transform.forward, position - GameManager.Instance.main_camera.transform.position) < 0)
        // {
        //     position = -position;
        // }

        // Vector3 new_position = RuntimePanelUtils.CameraTransformWorldToPanel(panel, position, GameManager.Instance.main_camera);
        Vector3 new_position = GameManager.Instance.main_camera.WorldToScreenPoint(position);
        float margin = _width;

        // if target is behind camera, lock indicator to edge of viewport
        if (new_position.z < 0)
        {
            new_position.x -= GameManager.Instance.main_camera.pixelWidth / 2;
            new_position.y -= GameManager.Instance.main_camera.pixelHeight / 2;
            new_position *= -100f;
            new_position.x += GameManager.Instance.main_camera.pixelWidth / 2;
            new_position.y += GameManager.Instance.main_camera.pixelHeight / 2;
        }
        
        // clamp indicator to screen region
        new_position.x = Mathf.Clamp(new_position.x, margin, GameManager.Instance.main_camera.pixelWidth - margin);
        new_position.y = GameManager.Instance.main_camera.pixelHeight - Mathf.Clamp(new_position.y, margin, GameManager.Instance.main_camera.pixelHeight - margin);

        visible = true;
        new_position.z = 0;
        transform.position = new_position;
    }
}
