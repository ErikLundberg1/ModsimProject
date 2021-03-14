using System.Collections;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;

public class Boid : MonoBehaviour
{
    public BoidSpawner BoidSpawner { get; set; }
    public TrafficLight TrafficLight { get; set; }
    public int BoidIndex { get; set; }

    public Vector3 Velocity;
    public Vector3 Position;
    public Vector3 Acceleration;
    private float ComfortableAcceleration;
    public float DesirableVelocity;
    private bool laneChangeLeft = false;
    private bool laneChangeRight = false;
    private float timeLaneLeft;
    private float timeLaneRight;
    private Rigidbody rb;
    private bool trafficLight = false;
    private float trafficLightXValue = 80f;
    private float deceleration = 0f;
    private int carCount;
 
    void Start()
    {
        Position = this.transform.position;
        Velocity = new Vector3(5f, 0, 0);
        DesirableVelocity = Random.Range(10f, 15f);
        ComfortableAcceleration = Random.Range(0.45f, 0.90f);
        Time.timeScale = 2f;
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    public void Update()
    {
        if (laneChangeLeft == false && checkOtherLane() && this.Position.z == -2.0f)
        {
            laneChangeLeft = true;
            timeLaneLeft = 0;
        }

        if (laneChangeRight == false && checkOtherLaneSlower() && this.Position.z == 0.0f)
        {
            //Debug.Log("Changing to right");
            laneChangeRight = true;
            timeLaneRight = 0;
        }

        if (laneChangeLeft)
        {
            if (timeLaneLeft > 1.0f)
            {
                laneChangeLeft = false;
            }

            Position = new Vector3(Position.x, Position.y, Mathf.Lerp(-2.0f, 0, timeLaneLeft / 0.95f));
            timeLaneLeft += Time.deltaTime;
        }

        if (laneChangeRight)
        {
            if (timeLaneRight > 1.0f)
            {
                laneChangeRight = false;
            }

            Position = new Vector3(Position.x, Position.y, Mathf.Lerp(0, -2.0f, timeLaneRight / 0.95f));
            timeLaneRight += Time.deltaTime;
        }
        //Clear acceleration from last frame
        Acceleration = Vector3.zero;

        //Apply forces
        Acceleration.x += getBoidForces().x;
        Acceleration.x += GetConstraintSpeedForce().x;

        trafficLightM();

        if (Velocity.x < DesirableVelocity && !trafficLight)
        {
            Acceleration.x += 0.15f * (DesirableVelocity - Velocity.x);
        }

        if (Acceleration.x > ComfortableAcceleration && !trafficLight)
            Acceleration.x = ComfortableAcceleration;
        if (float.IsNaN(Acceleration.x))
            Acceleration.x = 0.005f;

        Velocity += Time.deltaTime * new Vector3(Acceleration.x, 0 , 0); 
        Position += Time.deltaTime * Velocity + 0.5f * Time.deltaTime * Time.deltaTime * Acceleration;

        deleteCar(200f);
    }

    public void FixedUpdate()
    {
        rb.MovePosition(Position);
    }

    private void deleteCar(float endPoint)
    {
        if (this.Position.x > endPoint)
        {
            Destroy(gameObject);
            BoidSpawner.clearList(BoidIndex);
            BoidSpawner.spawnNewCar(BoidIndex);
        }
    }

    private Vector3 getBoidForces()
    {
        Vector3 cohesionForce = Vector3.zero;
        Vector3 alignmentForce = Vector3.zero;
        Vector3 separationForce = Vector3.zero;
        // Code for the avg velocity calculation
        int amountOfNeighboursAlignment = 0;
        int amountOfNeighboursCohesion = 0;
        Vector3 avgVelocity = Vector3.zero;
        Vector3 avgPosition = Vector3.zero;
        float springForce = 0;
        int firstTime = 0;
        Boid leastDistanceCar = null;
        foreach (Boid b in BoidSpawner.GetNeighbors(this, 20f))
        {
            float distance = (b.Position - Position).magnitude;

            if (firstTime == 0)
            {
                firstTime++;
                leastDistanceCar = b;
            }

            //Separation force
            if (distance < BoidSpawner.SeparationRadius && b.Position.z == this.Position.z)
            {
                separationForce += BoidSpawner.SeparationForceFactor * ((BoidSpawner.SeparationRadius - distance) / distance) * (Position - b.Position);
            }

            //Calculate average position/velocity here
            if (distance < BoidSpawner.AlignmentRadius && b.Position.z == this.Position.z)
            {
                avgVelocity = avgVelocity + b.Velocity;
                amountOfNeighboursAlignment++;
            }

            if (distance < BoidSpawner.CohesionRadius && b.Position.z == this.Position.z)
            {
                avgPosition = avgPosition + b.Position;
                amountOfNeighboursCohesion++;
            }

            leastDistanceCar = findCarInFront(leastDistanceCar, b);
        }

        if (leastDistanceCar != null && Velocity.x > leastDistanceCar.Velocity.x && Position.x - leastDistanceCar.Position.x < 25 && leastDistanceCar.Position.z == this.Position.z)
        {
            springForce = -0.85f * (Velocity.x - leastDistanceCar.Velocity.x);
        }

        //Set cohesion/alignment forces here
        if (amountOfNeighboursAlignment != 0)
        {
            avgVelocity = avgVelocity / amountOfNeighboursAlignment;
            alignmentForce += BoidSpawner.AlignmentForceFactor * (avgVelocity - Velocity);
        }

        if (amountOfNeighboursCohesion != 0)
        {
            avgPosition = avgPosition / amountOfNeighboursCohesion;
            cohesionForce += BoidSpawner.CohesionForceFactor * (avgPosition - Position);
        }

        return alignmentForce + separationForce + new Vector3(springForce, 0, 0);
    }

    private bool checkOtherLane()
    {
        Boid leastDistanceCar = null;
        Boid leastDistanceCarLane = null;
        Boid leastDistanceCareLaneBehind = null;

        int firstTime = 0;
        int firstTimeLane = 0;
        int firstTimeBehindLane = 0;

        foreach (Boid b in BoidSpawner.GetNeighbors(this, 15f))
        {

            if (firstTime == 0 && b.Position.z == this.Position.z && b.Position.x > this.Position.x)
            {
                firstTime++;
                leastDistanceCar = b;
            }

            if (firstTimeLane == 0 && b.Position.z != this.Position.z && b.Position.x > this.Position.x)
            {
                firstTimeLane++;
                leastDistanceCarLane = b;
            }

            if (firstTimeBehindLane == 0 && b.Position.z != this.Position.z && b.Position.x < this.Position.x)
            {
                firstTimeBehindLane++;
                leastDistanceCareLaneBehind = b;
            }

            if ( this.Position.x > b.Position.x && this.Position.x - b.Position.x < 12.5f && this.Position.z != b.Position.z)
            {
                return false;
            }

            if (this.Position.x < b.Position.x && b.Position.x - this.Position.x  < 12.5f && this.Position.z != b.Position.z)
            {
                return false;
            }

            if(leastDistanceCar != null)
            {
                leastDistanceCar = findCarInFront(leastDistanceCar, b);
            }

            if (leastDistanceCarLane != null)
            {
                leastDistanceCarLane = findCarInFrontLane(leastDistanceCarLane, b);
            }

            if(leastDistanceCareLaneBehind != null)
            {
                leastDistanceCareLaneBehind = findCarBehindLane(leastDistanceCareLaneBehind, b);
            }
        }
        
        if(leastDistanceCar != null)
        {
            if (leastDistanceCar.Acceleration.x + 0.0375 >= Acceleration.x)
                return false;
        }

        if (leastDistanceCar == null)
        {
            return false;
        }

        if(leastDistanceCarLane != null)
        {
            if (leastDistanceCarLane.Acceleration.x <= Acceleration.x + 0.0365f)
                return false;
        }

        if(leastDistanceCareLaneBehind != null)
        {
            if(leastDistanceCareLaneBehind.Acceleration.x - 0.025 >= Acceleration.x)
            {
                return false;
            }
        }

        if (Velocity.x < 6.75f)
            return false;

        return true;

    }

    private Vector3 GetConstraintSpeedForce()
    {
        Vector3 force = Vector3.zero;

        //Apply drag
        force -= 0.000005f * Velocity;

        if (Velocity.x > DesirableVelocity)
        {
            //If speed is above the maximum allowed speed, apply extra friction force
            force -= (5.0f * (Velocity.x - DesirableVelocity) / Velocity.x) * Velocity;
        }

        return force;
    }

    private Boid findCarInFront(Boid leastDistanceCar, Boid b)
    {
        if(b.Position.x - this.Position.x < leastDistanceCar.Position.x - this.Position.x && b.Position.x > this.Position.x && this.Position.z == b.Position.z)
        {
            return b;
        }
        else
        {
            return leastDistanceCar;
        }
    }

    private Boid findCarInFrontLane(Boid leastDistanceCarLane, Boid b)
    {
        if (b.Position.x - this.Position.x < leastDistanceCarLane.Position.x - this.Position.x && b.Position.x > this.Position.x && this.Position.z != b.Position.z)
        {
            return b;
        }
        else
        {
            return leastDistanceCarLane;
        }
    }

    private Boid findCarBehindLane(Boid leastDistanceCarLaneBehind, Boid b)
    {
        if (this.Position.x - b.Position.x > this.Position.x - leastDistanceCarLaneBehind.Position.x && b.Position.x < this.Position.x && this.Position.z != b.Position.z)
        {
            return b;
        }
        else
        {
            return leastDistanceCarLaneBehind;
        }
    }

    private bool checkOtherLaneSlower()
    {
        Boid leastDistanceCar = null;
        Boid leastDistanceCar1 = null;

        int firstTimeSlower = 0;
        int firstTime = 0;

        foreach (Boid b in BoidSpawner.GetNeighbors(this, 15f))
        {

            if (firstTimeSlower == 0 && b.Position.z == this.Position.z && b.Position.x < this.Position.x)
            {
                firstTimeSlower++;
                leastDistanceCar = b;
            }

            if (firstTime == 0 && b.Position.z != this.Position.z && b.Position.x < this.Position.x)
            {
                firstTime++;
                leastDistanceCar1 = b;
            }

            if (this.Position.x > b.Position.x && this.Position.x - b.Position.x < 12.5f && this.Position.z != b.Position.z)
            {
                return false;
            }

            if (this.Position.x < b.Position.x && b.Position.x - this.Position.x < 12.5f && this.Position.z != b.Position.z)
            {
                return false;
            }

            if (leastDistanceCar != null)
            {
                leastDistanceCar = findCarInFrontSlower(leastDistanceCar, b);
            }

            if(leastDistanceCar1 != null)
            {
                leastDistanceCar1 = findCarInFrontSlower1(leastDistanceCar1, b);
            }

        }

        if (leastDistanceCar1 == null)
        {
            return false;
        }

        else if (leastDistanceCar1 != null)
        {
            if (leastDistanceCar1.Acceleration.x > this.Acceleration.x)
            {
                Debug.Log("Working!");
                return true;
            }
        }


        if (leastDistanceCar == null)
        {   
            return false;
        }

        else if(leastDistanceCar != null)
        {
            if (leastDistanceCar.Acceleration.x <= this.Acceleration.x + 0.035f)
                return false;
        }

        return true;

    }

    private Boid findCarInFrontSlower(Boid leastDistanceCar, Boid b)
    {
        if (this.Position.x - b.Position.x < this.Position.x - leastDistanceCar.Position.x && b.Position.x < this.Position.x && this.Position.z == b.Position.z)
        {
            return b;
        }
        else
        {
            return leastDistanceCar;
        }
    }

    private Boid findCarInFrontSlower1(Boid leastDistanceCar, Boid b)
    {
        if (this.Position.x - b.Position.x < this.Position.x - leastDistanceCar.Position.x && b.Position.x < this.Position.x && this.Position.z != b.Position.z)
        {
            return b;
        }
        else
        {
            return leastDistanceCar;
        }
    }

    private void trafficLightM()
    {
        if (trafficLight)
        {
            carsInFront();
            deceleration = -(float)System.Math.Pow(Velocity.x, 2) / (2 * (trafficLightXValue - Position.x - 5 * carCount)) + getSeparationForce().x;
        }


        if (trafficLight && Position.x >= trafficLightXValue - 1 - 5 * carCount)
        {
            Acceleration.x = 0;
            Velocity.x = 0;

            if (!TrafficLight.firstCar)
                TrafficLight.firstCar = true;
            
            if (TrafficLight.getTime() >= 3.0f)
            {
                Velocity.x = 1;
                trafficLight = false;
                deceleration = 0;
            }
        }

        else if(trafficLight)
            Acceleration.x = deceleration;
    }

    private void carsInFront()
    {
        carCount = 0;
        foreach (Boid b in BoidSpawner.GetNeighbors(this, 50f))
        {
            if (b.Position.x > this.Position.x && b.Position.z == this.Position.z)
                carCount++;
        }
    }

    public void setTrafficLight(bool value)
    {
        trafficLight = value;
    }

    private Vector3 getSeparationForce()
    {
        Vector3 separationForce = Vector3.zero;

        foreach (Boid b in BoidSpawner.GetNeighbors(this, 20f))
        {
            float distance = (b.Position - Position).magnitude;

            //Separation force
            if (distance < BoidSpawner.SeparationRadius && b.Position.z == this.Position.z)
            {
                separationForce += BoidSpawner.SeparationForceFactor * ((BoidSpawner.SeparationRadius - distance) / distance) * (Position - b.Position);
            }
        }

        return separationForce;
    }
}
