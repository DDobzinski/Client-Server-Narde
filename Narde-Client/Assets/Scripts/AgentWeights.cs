using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AgentWeights
{
    public double fill = 0.6;
    public double pair = 0.2;
    public double pair_end = 0.05;
    public double chainDist = 0.2;
    public double holes = -0.015;
    public double pass = 0.2;
    public double onenemybase = 0.5;
    public double onenemybase_e = 0.2;
    public double movInHome = -1.0;
    public double danger_start = 0.04;
    public double danger_end = 0.15;
    public double danger_add = 0.4;
    public double canPlace = 2;
    public double opCanPlace = -3;
    public double nearHome = 0.0015;
    public double head_mul = 0.015;
    public double head = 1;
    public double tower = -0.2;
    public double length = -0.1;
    public double length_end = -0.25;
    public double throw_ = 1000; // 'throw' is a reserved keyword in C#, so we use 'throw_'
    public double home = 0.2;
    public double home_middle = 0.3;
    public double home_end = 0.3;
    public double rand = 0;
    public List<double> field_start = new List<double>
    {
        0, 1.0, 1.05, 1.1, 1.15, 1.2, 1.25, 0, 0, 0.1, 0.2, 0.0, 0.4, 1.0, 1.6, 1.7, 2, 2, 2, 0, 0, 0, 0, 0
    };
    public List<double> field_middle = new List<double>
    {
        -0.2, -0.2, -0.1, -0.1, -0.1, -0.1, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
    };
}
