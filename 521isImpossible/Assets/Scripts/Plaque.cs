﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plaque : MonoBehaviour
{

    private Professor prof;
    private Tile profLocation;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public Professor getProf()
    {
        return this.prof;
    }

    public Tile getProfLocation()
    {
        return profLocation;
    }

    public void setProf(Professor p)
    {
        this.prof = p;
    }

    public void setProfLocation(Tile t)
    {
        this.profLocation = t;
    }
}
