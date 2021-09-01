using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class MRDemoDynamicCube : NetworkBehaviour
{

    float xAngle = 0f, yAngle = 0f, zAngle = 0f;
    void Update()
    {
        xAngle += 100 * Time.deltaTime;
        yAngle += 100 * Time.deltaTime;
        zAngle += 100 * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(xAngle, yAngle, zAngle);
    }

    public void OnTriggerEnter(Collider other) {
        float x = Random.Range(-7f, 7f);
        float z = Random.Range(-7f, 7f);
        transform.localPosition = new Vector3(x, 1.5f, z);
    }
}
