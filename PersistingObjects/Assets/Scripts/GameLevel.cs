using UnityEngine;

public class GameLevel : MonoBehaviour
{
    [SerializeField]
    SpawnZone spawnZone;

    void Start()
    {
        Game.Instance.spawnZoneOfLevel = spawnZone;
    }
}
