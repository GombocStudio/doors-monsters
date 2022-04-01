using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class UIRadialIndicator : MonoBehaviour
{
    private Image image;

    private float indicatorTimer = 0.0f;
    private float maxIndicatorTimer = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!image) { return; }

        if (indicatorTimer > 0.0f)
        {
            indicatorTimer -= Time.deltaTime;
            image.fillAmount = indicatorTimer / maxIndicatorTimer;

            if (indicatorTimer <= 0.0f)
                image.fillAmount = 0.0f;
        }
    }
    
    public void ActivateXSeconds(float seconds)
    {
        indicatorTimer = seconds;
        maxIndicatorTimer = seconds;
    }
}
