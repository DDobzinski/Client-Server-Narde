using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
public enum PointType
    {
        Top,
        Bottom
    }

public class Point : MonoBehaviour
{
    // Assuming you have a method to keep track of the checkers in the stack
    // Define the PointType enumeration here
    public int id;
    public bool isSelected = false;
    public bool isHighlighted = false;
    public Color defaultColor;
    public Color highlightColor;
    public Color selectedColor;
    private Image pointImage;
    public PointType pointType;
    public List<GameObject> checkersStack = new List<GameObject>();
    
    public List<MoveOption> possibleMoves = new List<MoveOption>();
    void Start()
    {
        pointImage = GetComponent<Image>();
        defaultColor = pointImage.color; // Store the original color
    }
    
    void OnMouseDown()
    {
        // Handle triangle selection logic
        GameManager.Instance.SelectTriangle(this);
    }

    public bool HasCheckers()
    {
        return checkersStack.Count > 0;
    }

    public void SetHighlight(bool highlight, bool isSelected = false) 
    {
        isHighlighted = highlight;
        if (isSelected)
        {
            // Highlight as selected
            pointImage.color = selectedColor;
        }
        else
        {
            // Regular highlight or default color
            pointImage.color = highlight ? highlightColor : defaultColor;
        }
    }
    

    public void AddChecker(GameObject checker)
    {
        checkersStack.Add(checker);
        // Optionally, position the checker on the Point here
        PositionCheckerInStack(checker, checkersStack.Count - 1);
    }

    public void SimulateAddChecker(GameObject checker)
    {
        checkersStack.Add(checker);
    }

    private void PositionCheckerInStack(GameObject checker, int index)
    {
        Checker checkerScript = checker.GetComponent<Checker>();
        if (checkerScript != null)
        {
            checkerScript.PositionCheckerInStack(index, pointType);
        }
    }

   public GameObject RemoveTopChecker()
    {
        if (checkersStack.Count > 0)
        {
            // Remove the top checker from the stack
            int lastIndex = checkersStack.Count - 1;
            GameObject checkerToRemove = checkersStack[lastIndex];
            checkersStack.RemoveAt(lastIndex);

            // Optionally, update the positions of the remaining checkers
            SortCheckers();

            return checkerToRemove;
        }
        return null;
    }

    public GameObject SimulateRemoveTopChecker()
    {
        if (checkersStack.Count > 0)
        {
            // Remove the top checker from the stack
            int lastIndex = checkersStack.Count - 1;
            GameObject checkerToRemove = checkersStack[lastIndex];
            checkersStack.RemoveAt(lastIndex);

            // Optionally, update the positions of the remaining checkers

            return checkerToRemove;
        }
        return null;
    }

    // Update the positions of the checkers in the stack
    private void UpdateCheckerPositions()
    {
        for (int i = 0; i < checkersStack.Count; i++)
        {
            Checker checkerScript = checkersStack[i].GetComponent<Checker>();
            if (checkerScript != null)
            {
                checkerScript.PositionCheckerInStack(i, pointType);
            }
        }
    }

    public bool ContainsCheckerColor(Checker.CheckerColor color)
    {
        if (checkersStack.Count > 0)
        {
            Checker topChecker = checkersStack[checkersStack.Count - 1].GetComponent<Checker>();
            return topChecker != null && topChecker.checkerColor == color;
        }
        return false;
    }

    public void SortCheckers()
    {
        // Sort the checkers list based on a defined criteria.
        // For example, you might sort them based on their instance ID or any other unique identifier
        checkersStack.Sort((a, b) => b.GetInstanceID().CompareTo(a.GetInstanceID()));

        // Update positions after sorting
        UpdateCheckerPositions();
    }

    public bool CanRemoveChecker()
    {
        // Logic to determine if a checker can be removed from this triangle
        // This might check if the triangle is one of the last six and other game-specific conditions
        return gameObject.tag == "RemovePoint"; // Replace with actual logic
    }

    public GameObject RemoveChecker()
    {
        if (CanRemoveChecker() && HasCheckers())
        {
            GameObject checkerToRemove = checkersStack[checkersStack.Count - 1];
            checkersStack.RemoveAt(checkersStack.Count - 1);
            return checkerToRemove;
        }
        return null;
    }

    public void Copy(Point otherpoint)
    {
       
        this.pointType = otherpoint.pointType;
        this.checkersStack = otherpoint.checkersStack;
    }
}