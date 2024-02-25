using System.Collections;
using System.Collections.Generic;

public class Move
{
    public int StartPointId { get; set; }
    public int EndPointId { get; set; }
    public List<int> DiceValues { get; set; }

    public Move(int startPointId, int endPointId, List<int> diceValues)
    {
        StartPointId = startPointId;
        EndPointId = endPointId;
        DiceValues = diceValues;
    }
}
