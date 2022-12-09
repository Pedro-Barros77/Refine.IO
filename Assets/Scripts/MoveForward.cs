using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveForward : MonoBehaviour
{
    float _travelingSpeed;
    public void SetSpeed(float travelingSpeed)
    {
        this._travelingSpeed = travelingSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.up * _travelingSpeed * Time.deltaTime);
    }
}
