using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuAudioManager : MonoBehaviour
{
    private static MenuAudioManager _instance;

    public static MenuAudioManager Instance { get => _instance; set => _instance = value; }

    void Awake()
    {
        _instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayClick()
    {
        GetComponent<AudioSource>().Play();
    }
}
