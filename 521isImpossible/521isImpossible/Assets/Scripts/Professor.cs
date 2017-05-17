using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Professor : MonoBehaviour
{

    private Tile location;
    private List<Professor> reccomendations;
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void init(Tile start)
    {
        this.reccomendations = new List<Professor>();
        location = start;
    }

    public List<Professor> getReccomendations()
    {
        return this.reccomendations;
    }
    public Tile getLocation()
    {
        return this.location;
    }
    public void setLocation(Tile t)
    {
        this.location = t;
    }
    public Professor genRandomProf()
    {
        return reccomendations[Random.Range((int)0, (int)reccomendations.Count)];
    }
}
