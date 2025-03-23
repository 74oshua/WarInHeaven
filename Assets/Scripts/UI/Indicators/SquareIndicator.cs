using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SquareIndicator : Indicator
{   
    public SquareIndicator(UIDocument ui, Targetable reference, Color color, float width = 100, float frame_width = 5) : base(ui, reference, color, width, frame_width) {}

    protected override void DrawCanvas(MeshGenerationContext context)
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
}
