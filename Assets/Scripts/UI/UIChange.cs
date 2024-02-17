using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UIChange : MonoBehaviour
{
    public TextMeshProUGUI counterDisplay;
    int counter;
    // Start is called before the first frame update
    void Start()
    {
        counter = 0;
        counterDisplay.color = Color.red;
    }

    // Update is called once per frame
    void Update()
    {
        counterDisplay.text = counter.ToString();
        counter++;

    }
}
