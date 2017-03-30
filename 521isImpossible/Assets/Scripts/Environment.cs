using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour {//spawner class

    //public Tile tilePrefab;
    public int numStudents;
    public Student studentPrefab;
    public Professor profPrefab;
    public int numNSRooms; //number of north south rooms
    public int numEWRooms; //number of east west rooms
    public int roomXSize, roomYSize; //each room will be roomXSize tiles large in the x direction
    public int tileSize; //square tiles
    int xVertices;
    int yVertices;

	// Use this for initialization
	void Start () {
        GenerateField();
	}


    void GenerateField()
    {
        //number of room plots 2 on each side, 1 for each NS room, n-1 blocks in between
        xVertices = (4 + 2 * numNSRooms - 1) * roomXSize + 1;
        yVertices = (4 + 2 * numEWRooms - 1) * roomYSize + 1;
        GameObject main = GameObject.FindGameObjectWithTag("MainCamera");
        main.transform.position = new Vector3(xVertices / 2, 40, yVertices / 2);
        Tile [,] tiles = new Tile[xVertices, yVertices];
        Student[] students = new Student[numStudents];
        Professor[] profs = new Professor[2*numNSRooms + 2*numEWRooms];
        int numProfInit = 0;

        int k = 0;
        
        for (int i = 0; i < xVertices; i++)
        {
            int l = 0;
            for (int j = 0; j<yVertices; j++)
            {
                
                Vector3 pos = new Vector3(i * tileSize, 0, j * tileSize);
                Tile newTile = new Tile(tileSize);
                tiles[i, j] = newTile;
                if (i>=2*roomXSize + k*roomXSize && i <= 2*roomXSize +(k+1)*roomXSize && k <= 2*numNSRooms - 1 )
                {
                    if (j== 0 || j == yVertices - 1 || j == roomYSize + 2*tileSize || j == yVertices - roomYSize - 2*tileSize - 1 || ((j == roomYSize || j == yVertices - roomYSize - 1) && (i % roomXSize < roomXSize / 2 - tileSize/2 || i % roomXSize > roomXSize / 2 + tileSize/2)))
                    {
                        newTile.setPassable(false);
                        newTile.spawnObject(PrimitiveType.Cube, pos);
                    }
                    else
                    {
                        if ((j == roomYSize - 1 || j == yVertices - roomYSize ) && i == 2*roomXSize+k*roomXSize + 1)
                        {
                            Professor p = Instantiate(profPrefab, new Vector3(i * tileSize, 0.5f, j * tileSize), Quaternion.identity);
                            profs[numProfInit] = p;
                            p.init(newTile);
                            numProfInit++;
                        }
                        newTile.setPassable(true);
                        newTile.spawnObject(PrimitiveType.Quad, pos);
                    }
                    if (i == 2 * roomXSize + (k + 1) * roomXSize - 1 && j == yVertices - 1)
                    {
                        k += 2;
                    }
                    
                }
                else if (j >= 2 * roomYSize + l * roomYSize && j <= 2 * roomYSize + (l + 1) * roomYSize && l <= 2 * numEWRooms - 1)
                {
                    if (i== 0 || i == xVertices - 1 || i == roomXSize + 2*tileSize || i == xVertices - roomXSize - 2*tileSize - 1 || ((i == roomXSize || i == xVertices - roomXSize - 1) && (j % roomYSize < roomYSize / 2 - tileSize/2 || j % roomYSize > roomYSize/2 + tileSize/2 )))
                    {
                        newTile.setPassable(false);
                        newTile.spawnObject(PrimitiveType.Cube, pos);
                    }
                    else
                    {
                        if ((i == roomXSize - 1 || i == xVertices - roomXSize ) && j == 2 * roomYSize + l * roomYSize + 1)
                        {
                            Professor p = Instantiate(profPrefab, new Vector3(i * tileSize, 0.5f, j * tileSize), Quaternion.identity);
                            profs[numProfInit] = p;
                            numProfInit++;
                            p.init(newTile);
                        }
                        newTile.setPassable(true);
                        newTile.spawnObject(PrimitiveType.Quad, pos);
                    }
                    if (j == 2 * roomYSize + (l + 1) * roomYSize - 1)
                    {
                        l += 2;
                    }
                }
                else if (i > roomXSize && i < xVertices - roomXSize - 1 && j > roomYSize && j < yVertices - roomYSize - 1)
                {
                    newTile.setPassable(true);
                    newTile.spawnObject(PrimitiveType.Quad, pos);

                }
                else
                {
                    newTile.setPassable(false);
                    newTile.spawnObject(PrimitiveType.Cube, pos);
                }
            }
        }
        Pathfinder silver = GameObject.FindObjectOfType<Pathfinder>();
        
        for (int i = 0; i< profs.Length; i++)
        {
            for (int j = 0; j<profs.Length; j++)
            {
                if (i!=j)
                {
                    profs[i].getReccomendations().Add(profs[j]);    
                }
            }
        }
        for (int i = 0; i<numStudents; i++)
        {
            int x = Random.Range((int)roomXSize + 1, (int)xVertices - roomXSize - 1);
            int y = Random.Range((int)roomYSize + 1, (int)yVertices - roomYSize - 1);
            while (!tiles[x,y].getPassable())
            {
                x = Random.Range((int)roomXSize + 1, (int)xVertices - roomXSize - 1);
                y = Random.Range((int)roomYSize + 1, (int)yVertices - roomYSize - 1);
            }
            students[i] = Instantiate(studentPrefab, new Vector3(x*tileSize, 0.5f, y*tileSize), Quaternion.identity);
            students[i].setAssigned(profs[Random.Range(0,profs.Length)]);
            students[i].setTarget(students[i].getAssigned().getLocation());
        }
        silver.init(tiles,students);
        foreach (Student student in students)
        {
            student.findPath();
        }
        Destroy(gameObject);

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
