using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Indicator : VisualElement
{
    // private Vector2 _center = Vector2.zero;
    // private Transform _target;
    protected float _width = 100;
    protected float _frame_width = 0.7f;
    protected UIDocument _ui;
    public Color color;
    protected Vector3 _world_position = Vector3.zero;
    protected Transform _reference;

    private Vector3 _reference_old_pos;
    // private Color _color = Color.white;
    
    // whether to hide the indicator if an object obstructs the position it tracks
    public bool occlude = false;

    // layermask for occulsion
    public int occlusion_mask = ~2;

    public Indicator(UIDocument ui, Color color, float width = 100, float frame_width = 5)
    {
        // _center = new Vector2(x, y);
        _width = width;
        _frame_width = frame_width;
        _ui = ui;
        // _color = color;
        this.color = color;

        generateVisualContent += DrawCanvas;

        // visible = true;
        _ui.rootVisualElement.Add(this);
    }

    protected virtual void DrawCanvas(MeshGenerationContext context)
    {
        Painter2D painter = context.painter2D;

        painter.lineWidth = _frame_width;
        painter.strokeColor = color;
        painter.lineJoin = LineJoin.Miter;
        painter.lineCap = LineCap.Round;

        float half_width = _width / 2;

        // _center = Vector2.zero;

        painter.BeginPath();
        painter.MoveTo(new Vector2(0, -half_width));
        painter.LineTo(new Vector2(half_width, -half_width));
        painter.LineTo(new Vector2(half_width, half_width));
        painter.LineTo(new Vector2(-half_width, half_width));
        painter.LineTo(new Vector2(-half_width, -half_width));
        painter.LineTo(new Vector2(0, -half_width));
        painter.Stroke();
    }

    public void SetPosition(Vector3 position, Transform reference = null, float lerp_factor = 1)
    {
        Vector3 old_position = _world_position;

        _reference = reference;
        _world_position = position;

        if (_reference)
        {
            _world_position += _reference.position;
            _reference_old_pos = _reference.position;

            _world_position = Vector3.Lerp(old_position, _world_position, lerp_factor);
        }
    }

    // sets indicator to highlight a position in world space
    public virtual void UpdatePosition()
    {
        // if (!visible)
        // {
        //     return;
        // }
        
        // hide if disabled or behind camera
        if (!enabledSelf || Vector3.Dot(GameManager.Instance.main_camera.transform.forward, _world_position - GameManager.Instance.main_camera.transform.position) < 0)
        {
            visible = false;
            return;
        }

        if (occlude)
        {
            Vector3 difference = _world_position - GameManager.Instance.main_camera.transform.position;
            if (Physics.Raycast(GameManager.Instance.main_camera.transform.position, difference, difference.magnitude, occlusion_mask))
            {
                visible = false;
                return;
            }
        }

        // _world_position += OriginShiftController.true_velocity * Time.deltaTime;

        if (_reference)
        {
            _world_position += _reference.position - _reference_old_pos;
            _reference_old_pos = _reference.position;
        }

        Vector3 new_position = RuntimePanelUtils.CameraTransformWorldToPanel(panel, _world_position, GameManager.Instance.main_camera);
        visible = true;
        new_position.z = transform.position.z;
        transform.position = new_position;
    }
}
