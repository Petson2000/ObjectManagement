using UnityEngine;

public class RotatingObject : MonoBehaviour
{
    [SerializeField]
    Vector3 angularVelocity;

    void FixedUpdate()
    {
        transform.Rotate(angularVelocity * Time.deltaTime);
    }
}
