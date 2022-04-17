using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GIF : MonoBehaviour
{
    [Header("Play GIF variables")]
    public Sprite[] frames; 
    public int framesPerSecond = 10;

    // Reference to gif image component
    private Image image;

    private void Start()
    {
        // Initialise image component
        image = GetComponent<Image>();
    }

    private void Update() 
    { 
        if (!image) { return; }

        // Compute gif frame index
        int index = (int)(Time.time * framesPerSecond) % frames.Length; 

        if (!frames[index]) { return; }

        // Set gif frame as image sprite
        image.sprite = frames[index]; 
    }
}
