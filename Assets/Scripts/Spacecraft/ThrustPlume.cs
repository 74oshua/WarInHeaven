using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrustPlume : MonoBehaviour
{
    // gameobject with the plume mesh and shader attached
    public GameObject plumeObject;
    public float max_jitter = 1;
    public float jitter_speed = 1;

    private float jitter = 0;

    // Start is called before the first frame update
    void Start()
    {
        //_material = plumeObject.GetComponent<MeshRenderer>().sharedMaterial;
    }

    // update the plume mesh according to the throttle of the thruster
    public void SimulatePlume(float throttle)
    {
        // don't apply jitter if our thrust is insignificant
        if (throttle > 0.01f)
        {
            jitter += Random.Range(-jitter_speed / GameManager.Instance.fixedTimestep, jitter_speed / GameManager.Instance.fixedTimestep);
            jitter = Mathf.Clamp(jitter, 0, max_jitter);
        }
        plumeObject.GetComponent<MeshRenderer>().material.SetFloat("_Throttle", throttle + jitter * throttle);
        
    }
}
