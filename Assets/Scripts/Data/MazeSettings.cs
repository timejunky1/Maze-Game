using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu()]
public class MazeSettings : UpdatableData
{
    public bool newMaze;
    public bool createOuterWall;
    public bool createEntrance = false;
    public int mazeSize = 50;
    public int cubeSize = 10;
    public int backTrackDst = 0;
    public int maxPathLength = 30;
    [Range(0, 40)]
    public int bossDst;
    public int amountOfBosses = 10;
    public int baseSize = 3;
    public int regionSpread = 10;
    public int bossChance = 10;
    public int showVertacies = 10;
}
