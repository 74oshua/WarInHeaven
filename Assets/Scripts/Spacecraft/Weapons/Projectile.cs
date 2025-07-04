using UnityEditor;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    // projectiles "cheat" high speed collisions
    // if a projectile will pass within a specified distance to an object in the next frame, a collision event will be simulated

    public OrbitalBody orbital_body;
    public float radius;
    private Scanner _scanner;
    private int _collision_layer;

    void Awake()
    {
        _scanner = gameObject.AddComponent<Scanner>();
    }

    void Start()
    {
        _collision_layer = 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("Spacecraft") | 1 << LayerMask.NameToLayer("CelestialBody");
    }

    // copied from HandleUtility.DistancePointLine
    float DistancePointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        return Vector3.Magnitude(ProjectPointLine(point, lineStart, lineEnd) - point);
    }

    Vector3 ProjectPointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 rhs = point - lineStart;
        Vector3 vector = lineEnd - lineStart;
        float magnitude = vector.magnitude;
        Vector3 vector2 = vector;
        if (magnitude > 1E-06f)
        {
            vector2 /= magnitude;
        }

        float value = Vector3.Dot(vector2, rhs);
        value = Mathf.Clamp(value, 0f, magnitude);
        return lineStart + vector2 * value;
    }

    void FixedUpdate()
    {
        _scanner.SetScanRange(orbital_body.state.velocity.magnitude);
        foreach (Targetable target in _scanner.GetVisibleTargets(TargetType.Any))
        {
            // Debug.Log(name + "," + target.name);
            Vector3 relative_velocity = target.ob.state.velocity - orbital_body.state.velocity;
            // Debug.DrawLine(target.transform.position, target.transform.position + relative_velocity * Time.deltaTime, Color.cyan);
            // Debug.Log(HandleUtility.DistancePointLine(orbital_body.transform.position, target.transform.position, target.transform.position + relative_velocity * Time.deltaTime));
            if (DistancePointLine(orbital_body.transform.position, target.transform.position, target.transform.position + relative_velocity * Time.fixedDeltaTime)
                < radius)
            // if (Vector3.Distance(orbital_body.transform.position, target.transform.position + relative_velocity * Time.deltaTime) < radius * relative_velocity.magnitude)
            {
                // at high speeds high enough to cause collision phasing, the projectile will appear to pass through the object
                orbital_body.gameObject.transform.position = target.transform.position;
            }
        }
    }
}
