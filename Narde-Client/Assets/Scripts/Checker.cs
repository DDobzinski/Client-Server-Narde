using UnityEngine;

public class Checker : MonoBehaviour
{
    public enum CheckerColor
    {
        Player,
        Enemy,
        None
        // Add more colors if needed
    }
    public CheckerColor checkerColor;
    public float yOffset = 5f; // Vertical offset between checkers in the stack
    public void PositionCheckerInStack(int index, PointType pointType)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        RectTransform parentRectTransform = rectTransform.parent.GetComponent<RectTransform>();
        float halfCheckerHeight = rectTransform.rect.height * 87/ 2;
        float startingPointY;

        if (pointType == PointType.Top)
        {
            // Start from the top of the triangle and go downwards
            startingPointY = -halfCheckerHeight- yOffset * index;
        }
        else
        {
            // Start from the bottom of the triangle and go upwards
            startingPointY = -parentRectTransform.rect.height + halfCheckerHeight + yOffset * (index);
        }

        // Set the new position
        rectTransform.anchoredPosition = new Vector2(parentRectTransform.rect.width / 2, startingPointY);
    }

    public void MakeCheckerInvisible()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 0f; // Set alpha to 0 for full transparency
            spriteRenderer.color = color;
        }
    }
}