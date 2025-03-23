using UnityEngine;

public class ArrowIcon : OverviewIcon
{
    public GameObject head;
    public GameObject stalk;

    public float length_scale = 1;

    void Start()
    {
        if (!icon)
        {
            return;
        }
        stalk.transform.SetParent(icon.transform);
        head.transform.SetParent(icon.transform);

        stalk.SetActive(true);
        head.SetActive(true);
        icon.SetActive(false);
    }

    void LateUpdate()
    {
        if (!icon)
        {
            return;
        }

        if (!icon.activeSelf && GameManager.Instance.in_overview)
        {
            icon.SetActive(true);
        }
        else if (icon.activeSelf && !GameManager.Instance.in_overview)
        {
            icon.SetActive(false);
        }
        
        if (icon.activeSelf)
        {
            CameraController cam = GameManager.Instance.main_camera.GetComponent<CameraController>();
            icon.transform.localScale = new Vector3(1, 1, 1) * cam.zoom * scale;
        }
    }

    public void SetVector(Vector3 vector)
    {
        if (vector.magnitude < float.Epsilon)
        {
            return;
        }
        icon.transform.rotation = Quaternion.FromToRotation(Vector3.up, vector);
        stalk.transform.localScale = new Vector3(1, vector.magnitude / icon.transform.lossyScale.y * length_scale, 1);
        head.transform.position = icon.transform.position + vector * 2 * length_scale;
    }
}
