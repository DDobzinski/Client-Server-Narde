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

    public void MakeCheckerInvisible(bool invisible)
    {
        if (TryGetComponent<SpriteRenderer>(out var spriteRenderer))
        {
            Color color = spriteRenderer.color;
            if(invisible)
            {
                color.a = 0f;
            }
            else
            {
                color.a = 1f;
            }
            
            spriteRenderer.color = color;
        }
    }

    public void ChangeOpacity(bool change)
    {
        if (TryGetComponent<SpriteRenderer>(out var spriteRenderer))
        {
            Color color = spriteRenderer.color;
            if(change)
            {
                color.a = 0.8f;
            }
            else
            {
                color.a = 1f;
            }
            
            spriteRenderer.color = color;
        }
    }
}