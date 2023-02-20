using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReset : MonoBehaviour
{
    private Vector3 _startPosition;
    // Start is called before the first frame update
    void Start()
    {
        _startPosition = transform.position;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (transform.position.y < -30.0f) transform.position = _startPosition;
    }
}
