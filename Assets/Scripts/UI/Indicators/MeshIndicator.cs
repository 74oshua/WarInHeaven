using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MeshIndicator : Indicator
{
    protected OverviewIcon _mesh;
    protected MeshRenderer[] _renderers;

    public MeshIndicator(UIDocument ui, OverviewIcon mesh, Targetable reference, Color color, bool overview_only = false, float width = 100, float frame_width = 5) : base(ui, reference, color, width, frame_width)
    {
        mesh.gameObject.SetActive(false);
        mesh.overview_only = overview_only;
        _mesh = GameObject.Instantiate(mesh, target.transform);
        _renderers = _mesh.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in _renderers)
        {
            renderer.material.color = color;
        }
        _mesh.scale = width / Screen.width;
        _mesh.gameObject.SetActive(true);
    }

    ~MeshIndicator()
    {
        GameObject.Destroy(_mesh);
        _ui.rootVisualElement.Remove(this);
    }

    protected override void DrawCanvas(MeshGenerationContext context)
    {
        return;
    }

    public override void ChangeColor(Color color)
    {
        foreach (MeshRenderer renderer in _renderers)
        {
            renderer.material.color = color;
        }
    }
}
