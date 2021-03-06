﻿using UnityEngine;

public class SphereSpawnZone : SpawnZone
{
    [SerializeField]
    bool surfaceOnly;

    public override Vector3 SpawnPoint
    {
        get
        {
            return transform.TransformPoint(surfaceOnly ? Random.onUnitSphere : Random.insideUnitSphere);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(Vector3.zero, 1f);
    }
}
