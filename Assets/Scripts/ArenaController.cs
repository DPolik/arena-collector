using UnityEngine;

public class ArenaController : MonoBehaviour
{
    [SerializeField] private float radius = 1.2f;
    [SerializeField] private Transform[] carSpawnPoints;
    
    public float Radius => radius;
    public Transform[] CarSpawnPoints => carSpawnPoints;

    public Vector3 GetPointOnEdge(Vector3 direction)
    {
        direction.y = 0f;
        return direction.normalized * radius;
    }

    public Vector3 GetRandomPointOnEdge()
    {
        var angle = Random.Range(0f, Mathf.PI * 2f);
        return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
    }
    
    public Vector3 GetRandomPointInside(float minRadius, float maxRadius)
    {
        var r = Mathf.Sqrt(Random.Range(minRadius * minRadius, maxRadius * maxRadius));
        var angle = Random.Range(0f, Mathf.PI * 2f);

        return new Vector3(Mathf.Cos(angle) * r, 0f, Mathf.Sin(angle) * r);
    }
}
