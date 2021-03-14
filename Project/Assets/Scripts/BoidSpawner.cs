using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class BoidSpawner : MonoBehaviour
{

    private List<Boid> m_boids;

    public Boid prefab;

    private static int number = 15;

    private int indexPosition;

    private IEnumerable<Boid> boidI;

    [Header("Car behaviour data. Experiment with changing these during runtime")]
    [SerializeField]
    private float m_cohesionForceFactor = 1;
    public float CohesionForceFactor
    {
        get { return m_cohesionForceFactor; }
        set { m_cohesionForceFactor = value; }
    }

    [SerializeField]
    private float m_cohesionRadius = 3;
    public float CohesionRadius
    {
        get { return m_cohesionRadius; }
        set { m_cohesionRadius = value; }
    }

    [SerializeField]
    private float m_separationForceFactor = 3.5f;
    public float SeparationForceFactor
    {
        get { return m_separationForceFactor; }
        set { m_separationForceFactor = value; }
    }

    [SerializeField]
    private float m_separationRadius = 3.5f;
    public float SeparationRadius
    {
        get { return m_separationRadius; }
        set { m_separationRadius = value; }
    }

    [SerializeField]
    private float m_alignmentForceFactor = 1;
    public float AlignmentForceFactor
    {
        get { return m_alignmentForceFactor; }
        set { m_alignmentForceFactor = value; }
    }

    [SerializeField]
    private float m_alignmentRadius = 3;
    public float AlignmentRadius
    {
        get { return m_alignmentRadius; }
        set { m_alignmentRadius = value; }
    }

    [SerializeField]
    private float m_maxSpeed = 8;
    public float MaxSpeed
    {
        get { return m_maxSpeed; }
        set { m_maxSpeed = value; }
    }

    [SerializeField]
    private float m_minSpeed;
    public float MinSpeed
    {
        get { return m_minSpeed; }
        set { m_minSpeed = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        m_boids = new List<Boid>();
        transform.eulerAngles = new Vector3(0, -90, 0);

        for (int i = 0; i < number; i++)
        {
            m_boids.AddRange(spawnCar(i));

        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public IEnumerable<Boid> spawnCar(int index)
    {
        Boid boids = Instantiate<Boid>(prefab, new Vector3(0f -7.5f * index, 1.65f, -2.0f), transform.rotation);
        boids.BoidSpawner = this;
        boids.BoidIndex = index;
        yield return boids;
    }

    public IEnumerable<Boid> GetNeighbors(Boid boid, float distance)
    {
        foreach (var other in m_boids)
        {
        if (other != boid && other != null && (other.transform.position.x - boid.transform.position.x) < distance)
            yield return other;
        }
    }

    public List<Boid> GetBoidList()
    {   
        return m_boids;
    }

    public void clearList(int index)
    {
        m_boids.RemoveAt(index);
    }

    public void spawnNewCar(int index)
    {
        m_boids.InsertRange(index, spawnCar(index));
    }
}
