using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class VectorIndicator : Indicator
{
    private ArrowIcon _arrow;
    private MeshRenderer[] _renderers;

    public VectorIndicator(UIDocument ui, ArrowIcon arrow, Targetable reference, Color color, float width = 1, float frame_width = 5) : base(ui, reference, color, width, frame_width)
    {
        arrow.gameObject.SetActive(false);
        _arrow = GameObject.Instantiate(arrow, target.transform);
        _renderers = _arrow.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in _renderers)
        {
            renderer.material.color = color;
        }
        _arrow.gameObject.layer = LayerMask.NameToLayer("Overview");
        _arrow.scale = width / Screen.width;
        _arrow.gameObject.SetActive(true);
    }

    public void UpdateVector(Vector3 vector)
    {
        _arrow.SetVector(vector);
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
