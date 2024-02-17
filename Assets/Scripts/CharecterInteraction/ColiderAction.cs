using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColiderAction : MonoBehaviour
{
    // Start is called before the first frame update

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger On Enter {other.GetComponent<Stats>().Health}");
    }
}
