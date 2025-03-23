using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class Indicator : VisualElement
{
    // private Vector2 _center = Vector2.zero;
    // private Transform _target;
    protected float _width = 100;
    protected float _frame_width = 0.7f;
    protected UIDocument _ui;
    protected Color _color;
    protected Vector3 _world_position = Vector3.zero;
    protected Targetable _reference;
    protected bool _occluded;

    private Vector3 _reference_old_pos = Vector3.zero;
    // private Color _color = Color.white;
    
    // whether to hide the indicator if an object obstructs the position it tracks
    // if true, indicator will still change appearance if occluded
    public bool hide_when_occluded = false;

    public bool realview_only = false;

    // layermask for occulsion
    public int occlusion_mask = ~LayerMask.GetMask("Spacecraft", "Ignore Raycast");

    public Targetable target
    {
        get { return _reference; }
    }

    public Indicator(UIDocument ui, Targetable reference, Color color, float width = 100, float frame_width = 5)
    {
        // _center = new Vector2(x, y);
        _width = width;
        _frame_width = frame_width;
        _ui = ui;
        _reference = reference;
        _color = color;

        generateVisualContent += DrawCanvas;

        // visible = true;
        _ui.rootVisualElement.Add(this);
    }

    ~Indicator()
    {
        _ui.rootVisualElement.Remove(this);
    }

    protected abstract void DrawCanvas(MeshGenerationContext context);

    public void SetPosition(Vector3 position, Targetable reference = null, float lerp_factor = 1)
    {
        Vector3 old_position = _world_position;

        _reference = reference;
        _world_position = position;
        
        _world_position += _reference.transform.position;
        _reference_old_pos = _reference.transform.position;

        _world_position = Vector3.Lerp(old_position, _world_position, lerp_factor);
    }

    public virtual void ChangeColor(Color color)
    {
        _color = color;
        MarkDirtyRepaint();
    }

    // updates indicator's position relative to it's reference
    public void UpdatePosition()
    {
        if (realview_only && GameManager.Instance.in_overview)
        {
            visible = false;
            return;
        }
        else
        {
            visible = true;
        }

        // remove from UI if reference object has been destroyed
        if (!_reference)
        {
            parent.Remove(this);
            return;
        }

        _world_position += _reference.transform.position - _reference_old_pos;
        _reference_old_pos = _reference.transform.position;

        // hide if disabled or behind camera
        // if (!enabledSelf || Vector3.Dot(GameManager.Instance.main_camera.transform.forward, _world_position - GameManager.Instance.main_camera.transform.position) < 0)
        if (!enabledSelf || Vector3.Dot(GameManager.Instance.main_camera.transform.forward, _world_position - GameManager.Instance.main_camera.transform.position) < 0)
        {
            visible = false;
            return;
        }

        Vector3 difference = _world_position - GameManager.Instance.main_camera.transform.position;
        bool is_occluded = Physics.Raycast(GameManager.Instance.main_camera.transform.position, difference, out RaycastHit hit, difference.magnitude, occlusion_mask);

        // prevent indicators inside the planets from being occluded
        if (!hide_when_occluded && is_occluded)
        {
            is_occluded = hit.collider != _reference.GetComponent<Collider>();
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

        Vector3 new_position = RuntimePanelUtils.CameraTransformWorldToPanel(panel, _world_position, GameManager.Instance.main_camera);
        visible = true;
        new_position.z = transform.position.z;
        transform.position = new_position;

        VisualUpdate();
    }

    public virtual void VisualUpdate()
    {}
}
