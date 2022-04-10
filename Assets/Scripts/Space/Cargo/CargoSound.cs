using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CargoSound : MonoBehaviour
{
    private static int cargos;

    void Awake()
    {
        cargos = 0;
    }

    void Update()
    {
        if (cargos > 0 && !GetComponent<AudioSource>().isPlaying)
            GetComponent<AudioSource>().Play();
        else if (cargos == 0)
            GetComponent<AudioSource>().Stop();

    }
    public static void StartSound()
    {
        cargos++;
    }
    public static void StopSound()
    {
        cargos--;
    }
}
