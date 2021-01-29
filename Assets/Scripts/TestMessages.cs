using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMessages : MonoBehaviour
{
    private void OnEnable()
    {
        Debug.Log("OnEnable");
    }
    private void OnDisable()
    {
        Debug.Log("OnDisable");
    }
    private void Awake()
    {
        Debug.Log("Awake");
        if (!isActiveAndEnabled)
            OnDisable();
    }
    private void Start()
    {
        Debug.Log("Start");
    }
}
