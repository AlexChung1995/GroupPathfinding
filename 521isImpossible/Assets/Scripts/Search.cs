using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Search
{//node in search tree

    private Tile tile;
    private Search parent;

    public Search(Tile t)
    {
        tile = t;
        parent = null;
    }

    public Search(Tile t, Search p)
    {
        tile = t;
        parent = p;
    }

    public Search getParent()
    {
        return this.parent;
    }

    public void setParent(Search p)
    {
        this.parent = p;
    }

    public Tile getTile()
    {
        return this.tile;
    }

    public void setTile(Tile t)
    {
        this.tile = t;
    }
}
