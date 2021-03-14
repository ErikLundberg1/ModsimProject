using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class TrafficLight : MonoBehaviour
{
    private GameObject lightGameObject;
    private Light lightComp;
    private bool trafficLightOn = true;
    public bool firstCar = false;
    private float timeVar = 0f;


    void Start()
    {
        // Make a game object
        lightGameObject = new GameObject("The Light");

        // Add the light component
        lightComp = lightGameObject.AddComponent<Light>();

        // Set color and position
        if(trafficLightOn)
            lightComp.color = Color.red;
        else
            lightComp.color = Color.green;

        lightComp.range = 45;

        // Set the position (or any transform property)
        lightGameObject.transform.position = new Vector3(88.73945f, 8.16f, 1.025494f);
    }

    void Update()
    {
        if(firstCar)
            timeVar += Time.deltaTime;

        if (timeVar >= 3.0f)
        {
            setColor();
            setTrafficLight();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        other.gameObject.GetComponent<Boid>().setTrafficLight(trafficLightOn);
        other.gameObject.GetComponent<Boid>().TrafficLight = this;

    }
    
    public void setColor()
    {
        lightComp.color = Color.green;
    }

    public void setTrafficLight()
    {
        trafficLightOn = false;
    }

    public float getTime()
    {
        return timeVar;
    }
}
