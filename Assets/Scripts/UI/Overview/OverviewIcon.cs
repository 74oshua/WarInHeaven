using UnityEngine;

public class OverviewIcon : MonoBehaviour
{
    public GameObject icon;
    public float scale = 1;
    public bool overview_only = false;
    public float min_realview_distance = 100f;

    void Start()
    {
        if (!icon)
        {
            return;
        }

        if (overview_only)
        {
            gameObject.layer = LayerMask.NameToLayer("Overview");
            icon.layer = LayerMask.NameToLayer("Overview");
            icon.SetActive(false);
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Indicator");
            icon.layer = LayerMask.NameToLayer("Indicator");
        }
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
        else if (overview_only && icon.activeSelf && !GameManager.Instance.in_overview)
        {
            icon.SetActive(false);
        }
        if (icon.activeSelf && !GameManager.Instance.in_overview && Vector3.Distance(GameManager.Instance.main_camera.transform.position, icon.transform.position) < min_realview_distance)
        {
            icon.SetActive(false);
        }
        else if (!icon.activeSelf && !GameManager.Instance.in_overview && Vector3.Distance(GameManager.Instance.main_camera.transform.position, icon.transform.position) >= min_realview_distance)
        {
            icon.SetActive(true);
        }
        
        if (icon.activeSelf)
        {
            CameraController cam = GameManager.Instance.main_camera.GetComponent<CameraController>();
            icon.transform.localScale = (cam.zoom + (cam.transform.position - icon.transform.position).magnitude) * scale * new Vector3(1, 1, 1);
        }
    }
}
