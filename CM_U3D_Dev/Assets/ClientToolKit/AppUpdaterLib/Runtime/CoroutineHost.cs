using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineHost : MonoBehaviour
{
    // Start is called before the first frame update
    void OnEnable()
    {
        GameObject.DontDestroyOnLoad(this);
    }

}
