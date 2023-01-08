using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlanetScript : MonoBehaviour
{
    public uint infectionStatus;
    private SpriteRenderer spriteRenderer;
    private uint maxInfectionStatus = 10;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        infectionStatus = Math.Min(infectionStatus, maxInfectionStatus);
    }

    void Update()
    {
        float colorValue = 1.0f - ((float)infectionStatus / (float)maxInfectionStatus);
        spriteRenderer.color = new Color(1.0f, colorValue, colorValue);
    }
}
