using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManagaer : MonoBehaviour
{

    public Canvas[] canvases; 

    private void Awake()
    {
        foreach(Canvas c in canvases)
        {
            c.gameObject.SetActive(false);
        }
    }
}
