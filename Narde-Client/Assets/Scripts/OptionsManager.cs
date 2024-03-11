using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    public Image point1;
    public Image point2;
    public Image checkerPlayer;
    public Image checkerEnemy;
    public Image board;
    public Image side1;
    public Image side2;
    public TMP_Dropdown point1Dropdown;
    public TMP_Dropdown point2Dropdown;
    public TMP_Dropdown checkerPlayerDropdown;
    public TMP_Dropdown checkerEnemyDropdown;
    public TMP_Dropdown boardDropdown;
    public TMP_Dropdown sideDropdown;

    private int point1Value = 0;
    private int point2Value = 1;
    private int checkerPlayerValue = 2;
    private int checkerEnemyValue = 3;
    private int boardValue = 4;
    private int sideValue = 5;
    private List<Color32> colors = new();

    void Start()
    {
        colors.Add(new Color32(182, 127, 60, 255));
        colors.Add(new Color32(253, 236, 213, 255));
        colors.Add(new Color32(255, 255, 255, 255));
        colors.Add(new Color32(86, 85, 78, 255));
        colors.Add(new Color32(242, 193, 100, 255));
        colors.Add(new Color32(216, 165, 106, 255));
        colors.Add(new Color32(229, 92, 90, 255));
        colors.Add(new Color32(250, 115, 188, 255));
        colors.Add(new Color32(193, 101, 254, 255));
        colors.Add(new Color32(98, 84, 238, 255));
        colors.Add(new Color32(58, 153, 248, 255));
        colors.Add(new Color32(80, 242, 236, 255));
        colors.Add(new Color32(73, 254, 152, 255));
        colors.Add(new Color32(100, 227, 62, 255));
        colors.Add(new Color32(151, 223, 57, 255));
        colors.Add(new Color32(238, 236, 81, 255));
        colors.Add(new Color32(195, 195, 195, 255));
        colors.Add(new Color32(117, 117, 117, 255));

        point1Dropdown.onValueChanged.AddListener(delegate { OnPoint1Change(point1Dropdown.value); });
        point2Dropdown.onValueChanged.AddListener(delegate { OnPoint2Change(point2Dropdown.value); });
        checkerPlayerDropdown.onValueChanged.AddListener(delegate { OnPlayerCheckerChange(checkerPlayerDropdown.value); });
        checkerEnemyDropdown.onValueChanged.AddListener(delegate { OnEnemyCheckerChange(checkerEnemyDropdown.value); });
        boardDropdown.onValueChanged.AddListener(delegate { OnBoardChange(boardDropdown.value); });
        sideDropdown.onValueChanged.AddListener(delegate { OnSideChange(sideDropdown.value); });
    }
    
    public void OnPoint1Change(int dropDownValue)
    {
        
        point1Value = CalculateColor(0, dropDownValue);
        point1.color = colors[point1Value];
    }
    public void OnPoint2Change(int dropDownValue)
    {
        point2Value = CalculateColor(1, dropDownValue); 
        point2.color = colors[point2Value];
    }
    public void OnPlayerCheckerChange(int dropDownValue)
    {
        checkerPlayerValue = CalculateColor(2, dropDownValue); 
        checkerPlayer.color = colors[checkerPlayerValue];
    }
    public void OnEnemyCheckerChange(int dropDownValue)
    {
        checkerEnemyValue = CalculateColor(3, dropDownValue); 
        checkerEnemy.color = colors[checkerEnemyValue];
    }
    public void OnBoardChange(int dropDownValue)
    {
        boardValue = CalculateColor(4, dropDownValue); 
        board.color = colors[boardValue];
    }
    public void OnSideChange(int dropDownValue)
    {
        sideValue = CalculateColor(5, dropDownValue); 
        side1.color = colors[sideValue];
        side2.color = colors[sideValue];
    }
    public void SaveOptions()
    {
        Client.instance.player.SaveSettings(point1Value, point2Value, checkerPlayerValue, checkerEnemyValue, boardValue, sideValue);
    }
    private int CalculateColor(int defaultColor, int dropDownValue)
    {
        if(dropDownValue == 0) return defaultColor;
        else if(dropDownValue <= defaultColor)
        {
            return dropDownValue - 1;
        }
        return dropDownValue;
    }

    private int CalculateColorRevers(int defaultColor, int colorValue)
    {
        if(colorValue == defaultColor) return 0;
        else if(colorValue < defaultColor)
        {
            return colorValue + 1;
        }
        return colorValue;
    }

    public void SetValues()
    {
        if(colors.Count > 0)
        {
        point1Value = Client.instance.player.point1Colour;
        point1.color = colors[point1Value];
        point1Dropdown.value = CalculateColorRevers(0, point1Value);

        point2Value = Client.instance.player.point2Colour;
        point2.color = colors[point2Value];
        point2Dropdown.value = CalculateColorRevers(1, point2Value);

        checkerPlayerValue = Client.instance.player.playerCheckerColour;
        checkerPlayer.color = colors[checkerPlayerValue];
        checkerPlayerDropdown.value = CalculateColorRevers(2, checkerPlayerValue);

        checkerEnemyValue = Client.instance.player.enemyCheckerColour;
        checkerEnemy.color = colors[checkerEnemyValue];
        checkerEnemyDropdown.value = CalculateColorRevers(3, checkerEnemyValue);

        boardValue = Client.instance.player.boardColour;
        board.color = colors[boardValue];
        boardDropdown.value = CalculateColorRevers(4, boardValue);

        sideValue = Client.instance.player.sideColour;
        side1.color = colors[sideValue];
        side2.color = colors[sideValue];
        sideDropdown.value = CalculateColorRevers(5, sideValue);
        }
        
    }
}
