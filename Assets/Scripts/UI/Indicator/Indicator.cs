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
    protected Color _color;
    protected Vector3 _world_position = Vector3.zero;
    protected Targetable _target;
    protected bool _occluded;

    private Vector3 _reference_old_pos = Vector3.zero;
    // private Color _color = Color.white;
    
    // whether to hide the indicator if an object obstructs the position it tracks
    // if true, indicator will still change appearance if occluded
    public bool hide_when_occluded = false;

    // layermask for occulsion
    public int occlusion_mask = ~LayerMask.GetMask("Spacecraft", "Ignore Raycast");

    public Targetable target
    {
        get { return _target; }
    }

    public Indicator(UIDocument ui, Targetable reference, Color color, float width = 100, float frame_width = 5)
    {
        // _center = new Vector2(x, y);
        _width = width;
        _frame_width = frame_width;
        _ui = ui;
        _target = reference;
        this._color = color;

        generateVisualContent += DrawCanvas;

        // visible = true;
        _ui.rootVisualElement.Add(this);
    }

    ~Indicator()
    {
        _ui.rootVisualElement.Remove(this);
    }

    protected virtual void DrawCanvas(MeshGenerationContext context)
    {
        Painter2D painter = context.painter2D;

        painter.lineWidth = _frame_width;
        if (_occluded)
        {
            Color c = _color;
            c.a = 0.25f;
            painter.strokeColor = Color.black;
        }
        else
        {
            painter.strokeColor = _color;
        }
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

    public void SetPosition(Vector3 position, Targetable reference = null, float lerp_factor = 1)
    {
        Vector3 old_position = _world_position;

        _target = reference;
        _world_position = position;

        if (_target)
        {
            _world_position += _target.transform.position;
            _reference_old_pos = _target.transform.position;

            _world_position = Vector3.Lerp(old_position, _world_position, lerp_factor);
        }
    }

    public void ChangeColor(Color color)
    {
        _color = color;
        MarkDirtyRepaint();
    }

    // updates indicator's position relative to it's reference
    public void UpdatePosition()
    {
        // hide if disabled or behind camera
        // if (!enabledSelf || Vector3.Dot(GameManager.Instance.main_camera.transform.forward, _world_position - GameManager.Instance.main_camera.transform.position) < 0)
        if (!enabledSelf || Vector3.Dot(GameManager.Instance.main_camera.transform.forward, _world_position - GameManager.Instance.main_camera.transform.position) < 0)
        {
            visible = false;
            return;
        }

        Vector3 difference = _world_position - GameManager.Instance.main_camera.transform.position;
        RaycastHit hit;
        bool is_occluded = Physics.Raycast(GameManager.Instance.main_camera.transform.position, difference, out hit, difference.magnitude, occlusion_mask);

        // prevent indicators inside the planets from being occluded
        if (!hide_when_occluded && is_occluded)
        {
            is_occluded = hit.collider != _target.GetComponent<Collider>();
        }

        // Debug.Log(_target.name + ": " + is_occluded);
        // repaint if just occluded
        if (_occluded != is_occluded)
        {
            _occluded = is_occluded;
            MarkDirtyRepaint();
        }

        if (hide_when_occluded && _occluded)
        {
            visible = false;
            return;
        }

        if (_target)
        {
            _world_position += _target.transform.position - _reference_old_pos;
            _reference_old_pos = _target.transform.position;
        }

        Vector3 new_position = RuntimePanelUtils.CameraTransformWorldToPanel(panel, _world_position, GameManager.Instance.main_camera);
        visible = true;
        new_position.z = transform.position.z;
        transform.position = new_position;
    }
}
