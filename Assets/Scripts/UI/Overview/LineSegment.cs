using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UIElements;

public class LineSegment : MonoBehaviour
{
    public GameObject line;
    public float scale = 1;
    private float _length;
    private Targetable _reference;
    private Vector3 _reference_old_pos;
    private List<MeshRenderer> _renderers = new();

    void Awake()
    {
        _renderers.AddRange(line.GetComponentsInChildren<MeshRenderer>());
    }

    void Start()
    {
        if (!line)
        {
            return;
        }

        line.transform.SetParent(null);
        line.SetActive(false);
    }

    void LateUpdate()
    {
        if (!line)
        {
            Debug.LogError("No lineSegment!");
            return;
        }

        if (!_reference || (line.activeSelf && !GameManager.Instance.in_overview))
        {
            line.SetActive(false);
            return;
        }
        
        if (!line.activeSelf && GameManager.Instance.in_overview)
        {
            line.SetActive(true);
        }
        
        if (line.activeSelf)
        {
            CameraController cam = GameManager.Instance.main_camera.GetComponent<CameraController>();
            float apparent_size = (cam.zoom + (cam.transform.position - line.transform.position).magnitude) / Screen.width;
            line.transform.localScale = new Vector3(apparent_size, _length * 0.5f / scale, apparent_size);

            if (_reference)
            {
                line.transform.position += _reference.transform.position - _reference_old_pos;
                _reference_old_pos = _reference.transform.position;
            }
        }
    }

    public void SetLine(Vector3 start, Vector3 end, Targetable reference, float lerp_factor = 1)
    {
        _reference = reference;
        line.transform.position = Vector3.Lerp(line.transform.position, start, lerp_factor);
        line.transform.rotation = Quaternion.Lerp(line.transform.rotation, Quaternion.FromToRotation(Vector3.up, end - start), lerp_factor);
        // icon.transform.LookAt(end + reference.transform.position);
        _length = Vector3.Distance(start, end);
        // icon.transform.localScale = new Vector3(cam.zoom * scale, _length * 0.5f, cam.zoom * scale);
        // if (vector.magnitude < float.Epsilon)
        // {
        //     return;
        // }
        // icon.transform.rotation = Quaternion.FromToRotation(Vector3.up, vector);
        // stalk.transform.localScale = new Vector3(1, vector.magnitude / icon.transform.lossyScale.y * length_scale, 1);
        // head.transform.position = icon.transform.position + vector * 2 * length_scale;
    }

    public void ChangeColor(Color color)
    {
        foreach (MeshRenderer renderer in _renderers)
        {
            renderer.material.color = color;
        }
    }
}
