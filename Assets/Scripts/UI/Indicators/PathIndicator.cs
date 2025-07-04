using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PathIndicator : Indicator
{
    private List<LineSegment> _lineSegments = new();
    private LineSegment _lineSegmentPrefab;
    private Color _far_color;

    public PathIndicator(UIDocument ui, LineSegment lineSegment, List<Vector3> path, Targetable reference, Color near_color, Color far_color, float width = 1, float frame_width = 5) : base(ui, reference, near_color, width, frame_width)
    {
        _far_color = far_color;

        // if (!lineSegment)
        // {
        //     Debug.LogError("No lineSegment!");
        //     return;
        // }

        _lineSegmentPrefab = lineSegment;
        _lineSegmentPrefab.gameObject.SetActive(false);

        for (int i = 0; i < path.Count-1; i++)
        {
            _lineSegments.Add(GameObject.Instantiate(_lineSegmentPrefab));
            _lineSegments[i].gameObject.SetActive(true);
            _lineSegments[i].ChangeColor(Color.Lerp(_color, _far_color, (float)i / path.Count));
            // _renderers.Add(_lineSegments[i].GetComponentsInChildren<MeshRenderer>());
            // foreach (MeshRenderer renderer in _renderers[i])
            // {
            //     renderer.material.color = Color.Lerp(_color, _far_color, (float)i / path.Count);
            // }
            // _lineSegments[i].scale = _width / Screen.width;
            // _lineSegments[i].gameObject.SetActive(true);

            _lineSegments[i].SetLine(path[i], path[i+1], reference);
        }
    }

    public void UpdatePath(List<Vector3> path, Targetable reference, float lerp_factor = 1)
    {
        _reference = reference;
        for (int i = 0; i < path.Count-1; i++)
        {
            if (i >= _lineSegments.Count)
            {
                _lineSegments.Add(GameObject.Instantiate(_lineSegmentPrefab));
                _lineSegments[i].gameObject.SetActive(true);
                _lineSegments[i].ChangeColor(Color.Lerp(_color, _far_color, (float)i / path.Count));
            }

            _lineSegments[i].SetLine(path[i], path[i+1], reference, lerp_factor);
        }
    }

    protected override void DrawCanvas(MeshGenerationContext context)
    {
        return;
    }

    public override void ChangeColor(Color color)
    {
        foreach (LineSegment indicator in _lineSegments)
        {
            indicator.ChangeColor(color);
        }
    }
}
