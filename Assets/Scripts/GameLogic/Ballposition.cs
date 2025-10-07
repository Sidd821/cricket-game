using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ballposition : MonoBehaviour
{
    public Transform ballPosition;
    public Transform batPosition;

    // Update is called once per frame
    void Update()
    {
        if (!SinglePlayerSetup.bowling)
        {
            ballPosition.gameObject.SetActive(false);
            batPosition.gameObject.SetActive(true);
        }else
        {
            ballPosition.gameObject.SetActive(true);
            batPosition.gameObject.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            ballPosition.position += new Vector3(-0.5f, 0, 0);
            batPosition.position += new Vector3(-2f, 0, 0);

        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            ballPosition.position += new Vector3(0.5f, 0, 0);
            batPosition.position += new Vector3(2f, 0, 0);

        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            ballPosition.position += new Vector3(0, 0, -0.5f);
            batPosition.position += new Vector3(0, 0, -2f);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            ballPosition.position += new Vector3(0, 0, 0.5f);
            batPosition.position += new Vector3(0, 0, 2f);
        }
    }
}
