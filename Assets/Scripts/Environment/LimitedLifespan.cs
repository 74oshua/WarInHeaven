using UnityEngine;

public class LimitedLifespan : MonoBehaviour
{
    // time in seconds before object is destroyed
    public float ttl = 1f;

    // time remaining in lifespan
    private float _remaining = 0;

    void Start()
    {
        _remaining = ttl;
    }

    void FixedUpdate()
    {
        _remaining -= GameManager.Instance.fixedTimestep;

        if (_remaining <= 0)
        {
            Destroy(gameObject);
        }
    }
}
