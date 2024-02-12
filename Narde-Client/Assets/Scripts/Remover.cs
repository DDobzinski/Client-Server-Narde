using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Remover : MonoBehaviour
{
    public Color defaultColor;
    public Color highlightColor;
    private Image objectImage;
    private bool isRemovalAllowed = false;

    void Start()
    {
        // Initialize the sprite renderer and set the default color
        objectImage = GetComponent<Image>();
        objectImage.color = defaultColor;
    }

    public void SetRemovalAllowed(bool allowed)
    {
        isRemovalAllowed = allowed;
        objectImage.color = allowed ? highlightColor : defaultColor;
    }

    void OnMouseDown()
    {
        // Check if removal is allowed and the object is clicked
        if (isRemovalAllowed)
        {
            // Perform the removal
            GameManager.Instance.RemoveCheckerFromSelectedPoint();
        }
    }
}
