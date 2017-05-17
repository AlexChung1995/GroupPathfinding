using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{

    private bool isPassable;
    private int scale;
    public Vector3 pos;

    public Tile(int s)
    {
        this.scale = s;
    }

    public Tile(int s, bool p, Vector3 position)
    {
        scale = s;
        isPassable = p;
        pos = position;
    }

    public bool getPassable()
    {
        return this.isPassable;
    }

    public void setPassable(bool pass)
    {
        this.isPassable = pass;
    }

    public void setPosition(Vector3 position)
    {
        pos = position;
    }
    public void spawnObject(PrimitiveType type, Vector3 position)
    {
        pos = position;
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.transform.position = position;
        obj.transform.localScale = new Vector3(this.scale, this.scale, this.scale);
        obj.transform.eulerAngles = new Vector3(90, 0, 0);
        Renderer renderer = obj.GetComponent<Renderer>();
        Material mat = renderer.material;
        if (this.isPassable == true)
        {
            mat.SetColor("_Color", Color.red);
            obj.GetComponent<MeshCollider>().convex = true;
        }
        else
        {
            mat.SetColor("_Color", Color.black);
        }
    }

    public int getScale()
    {
        return this.scale;
    }

    public void setScale(int S)
    {
        this.scale = S;
    }

}

