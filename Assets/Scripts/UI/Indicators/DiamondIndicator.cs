using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DiamondIndicator : Indicator
{
    public DiamondIndicator(UIDocument ui, Targetable reference, Color color, float width = 100, float frame_width = 5) : base(ui, reference, color, width, frame_width) {}

    protected override void DrawCanvas(MeshGenerationContext context)
    {
        Painter2D painter = context.painter2D;

        painter.lineWidth = _frame_width;
        if (_occluded)
        {
            Color c = _color;
            c.a = 0.25f;
            painter.strokeColor = c;
        }
        else
        {
            painter.strokeColor = _color;
        }
        // painter.strokeColor = color;
        painter.lineJoin = LineJoin.Miter;
        painter.lineCap = LineCap.Round;

        float half_width = _width / 2;

        painter.BeginPath();
        painter.MoveTo(new Vector2(0, -half_width));
        painter.LineTo(new Vector2(half_width, 0));
        painter.LineTo(new Vector2(0, half_width));
        painter.LineTo(new Vector2(-half_width, 0));
        painter.LineTo(new Vector2(0, -half_width));
        painter.Stroke();
    }
}