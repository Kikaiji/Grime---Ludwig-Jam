using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderValue : MonoBehaviour
{
    public float value;
    Text valueText;
    void Start()
    {
        valueText = transform.GetChild(5).GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        value = GetComponent<Slider>().value;
        valueText.text = value.ToString("F2");
    }
}
