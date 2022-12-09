using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
    float _delayInSeconds;
    //Qualquer objeto com esse script se auto destruirá após o tempo informado
    public void Begin(float delayInSeconds)
    {
        _delayInSeconds = delayInSeconds;
        StartCoroutine("SelfDestroyTimer");
    }

    IEnumerator SelfDestroyTimer()
    {
        yield return new WaitForSeconds(_delayInSeconds);
        Destroy(gameObject);
    }
    private void Start()
    {
        
    }
   
}
