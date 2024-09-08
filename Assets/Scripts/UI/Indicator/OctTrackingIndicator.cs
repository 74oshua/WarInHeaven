using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class OctTrackingIndicator : TrackingIndicator
{
    public OctTrackingIndicator(UIDocument ui, Targetable target, Color color, float width = 100, float frame_width = 5) : base(ui, target, color, width, frame_width) {}

    protected override void DrawCanvas(MeshGenerationContext context)
    {
        Painter2D painter = context.painter2D;

        painter.lineWidth = _frame_width;
        painter.strokeColor = color;
        painter.lineJoin = LineJoin.Miter;
        painter.lineCap = LineCap.Round;

        float half_width = _width / 2;
        float sixth_width = _width / 6;

        painter.BeginPath();
        painter.MoveTo(new Vector2(0, -half_width));
        painter.LineTo(new Vector2(sixth_width, -half_width));
        painter.LineTo(new Vector2(half_width, -sixth_width));
        painter.LineTo(new Vector2(half_width, sixth_width));
        painter.LineTo(new Vector2(sixth_width, half_width));
        painter.LineTo(new Vector2(-sixth_width, half_width));
        painter.LineTo(new Vector2(-half_width, sixth_width));
        painter.LineTo(new Vector2(-half_width, -sixth_width));
        painter.LineTo(new Vector2(-sixth_width, -half_width));
        painter.LineTo(new Vector2(0, -half_width));
        painter.Stroke();
    }
}
