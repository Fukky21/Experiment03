using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    [Header("Settings")]
    public string fileName;

    public GameObject Sphere;

    private int phase = 0;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (phase == 0)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Sphere.GetComponent<Renderer>().material.color = Color.red;
                if (OutputResult.initialize(fileName) == -1)
                {
                    Application.Quit();
                }
                phase++;
            }
        }
        else if (phase == 1)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Sphere.GetComponent<Renderer>().material.color = Color.blue;
                OutputResult.writeResult(fileName);
                Application.Quit();
            }
        }
    }
}
