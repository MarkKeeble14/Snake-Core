using System.Collections.Generic;
using UnityEngine;

public class SpawnCellOccupantsOnSelect : OnSelectCardAction
{
    [SerializeField] private SerializableDictionary<GridCellOccupantType, SpawnOdds> spawns = new SerializableDictionary<GridCellOccupantType, SpawnOdds>();

    public override void SetCard(SelectionCard card)
    {
        card.Set(label, detailsPrefix + GetSpawnsInfo() + detailsSuffix, delegate
        {
            List<GridCellOccupantType> spawnKeys = spawns.Keys();
            for (int i = 0; i < spawnKeys.Count; i++)
            {
                GridCellOccupantType type = spawnKeys[i];
                SpawnOdds spawnOdds = spawns[type];
                int evaluated = CheckSpawnOdds(spawnOdds);
                switch (type)
                {
                    case GridCellOccupantType.BOMB:
                        GridGenerator._Instance.SpawnBomb(evaluated);
                        break;
                    case GridCellOccupantType.COIN:
                        GridGenerator._Instance.SpawnCoin(evaluated);
                        break;
                    case GridCellOccupantType.FOOD:
                        GridGenerator._Instance.SpawnFood(evaluated);
                        break;
                    case GridCellOccupantType.TELEPORTER:
                        GridGenerator._Instance.SpawnTeleporter(evaluated);
                        break;
                    case GridCellOccupantType.BORDER_WALL:
                        GridGenerator._Instance.SpawnWall(WallType.BORDER, evaluated);
                        break;
                    case GridCellOccupantType.OBSTACLE:
                        GridGenerator._Instance.SpawnObstacle(evaluated);
                        break;
                    case GridCellOccupantType.POWERUP:
                        GridGenerator._Instance.SpawnPowerup(evaluated);
                        break;
                    case GridCellOccupantType.WALL:
                        GridGenerator._Instance.SpawnWall(WallType.NORMAL, evaluated);
                        break;
                    case GridCellOccupantType.VALUABLE_WALL:
                        GridGenerator._Instance.SpawnWall(WallType.VALUABLE, evaluated);
                        break;
                }
            }
        });
    }

    public int CheckSpawnOdds(SpawnOdds odds)
    {
        switch (odds.type)
        {
            case SpawnOddsType.CHANCE_TO:
                if (RandomHelper.EvaluateChanceTo(odds.chances))
                    return 1;
                return 0;
            case SpawnOddsType.NUM_BETWEEN:
                return RandomHelper.RandomIntExclusive(odds.chances);
            default:
                return 0;
        }
    }

    private string GetSpawnsInfo()
    {
        string s = "";
        List<GridCellOccupantType> spawnKeys = spawns.Keys();
        for (int i = 0; i < spawnKeys.Count; i++)
        {
            GridCellOccupantType type = spawnKeys[i];
            SpawnOdds spawnOdds = spawns[type];
            switch (spawnOdds.type)
            {
                case SpawnOddsType.CHANCE_TO:
                    if (spawnOdds.chances.x == spawnOdds.chances.y)
                        s += "Spawn " + GetOccupantString(type);
                    else
                        s += spawnOdds.chances.x + " / " + spawnOdds.chances.y + " Chance to Spawn " + GetOccupantString(type);
                    break;
                case SpawnOddsType.NUM_BETWEEN:
                    if (spawnOdds.chances.x == spawnOdds.chances.y)
                        s += "Spawn " + spawnOdds.chances.x + " " + GetOccupantString(type) + (spawnOdds.chances.x > 1 ? "s" : "");
                    else
                        s += "Spawn Between " + spawnOdds.chances.x + " - " + spawnOdds.chances.y + " " + GetOccupantString(type) + "s";
                    break;
            }

            if (i < spawnKeys.Count)
                s += "\n";
        }
        return s;
    }

    public static string GetOccupantString(GridCellOccupantType type)
    {
        switch (type)
        {
            case GridCellOccupantType.BOMB:
                return "Bomb";
            case GridCellOccupantType.COIN:
                return "Coin";
            case GridCellOccupantType.FOOD:
                return "Food";
            case GridCellOccupantType.TELEPORTER:
                return "Teleporter";
            case GridCellOccupantType.BORDER_WALL:
                return "Death Wall";
            case GridCellOccupantType.OBSTACLE:
                return "Obstacle";
            case GridCellOccupantType.POWERUP:
                return "Powerup";
            case GridCellOccupantType.WALL:
                return "Wall";
            case GridCellOccupantType.VALUABLE_WALL:
                return "Valuable Wall";
            default:
                throw new UnhandledSwitchCaseException();
        }
    }
}
