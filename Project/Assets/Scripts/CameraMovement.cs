using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour
{

    float cameraSpeed = 20.0f; //Set the camera speed
    float cameraSensitivity = 0.15f; //Set the camera sensitivity
    private Vector3 mousePosition = new Vector3(255, 255, 255); //This will set the mouse in the middle of the screen
    private float timeSpawn = 0;
    void Update()
    {
        //Change the angle of the camera
        mousePosition = Input.mousePosition - mousePosition;
        mousePosition = new Vector3(-mousePosition.y * cameraSensitivity, mousePosition.x * cameraSensitivity, 0);
        mousePosition = new Vector3(transform.eulerAngles.x + mousePosition.x, transform.eulerAngles.y + mousePosition.y, 0);
        if(timeSpawn >= 0.5)
            transform.eulerAngles = mousePosition;
        mousePosition = Input.mousePosition;
        
        //Move the position of the camera
        transform.Translate(GetInput() * cameraSpeed * Time.deltaTime);
        timeSpawn += Time.deltaTime;

    }

    private Vector3 GetInput()
    {   
        // If not button is pressed, return the zero vector, else add to the respective coordinate of the vector
        Vector3 directionChange = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            directionChange += new Vector3(0, 0, 1);
        }
        if (Input.GetKey(KeyCode.S))
        {
            directionChange += new Vector3(0, 0, -1);
        }
        if (Input.GetKey(KeyCode.A))
        {
            directionChange += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            directionChange += new Vector3(1, 0, 0);
        }
        return directionChange;
    }
}