using UnityEngine;

public class CompositeSpawnZone : SpawnZone
{
    SpawnZone[] spawnZones;

    public override Vector3 SpawnPoint
    {
        get
        {
            int index = Random.Range(0, spawnZones.Length);
            return spawnZones[index].SpawnPoint;
        }
    }
}
