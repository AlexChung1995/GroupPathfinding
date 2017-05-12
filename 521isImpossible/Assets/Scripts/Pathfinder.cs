using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Pathfinder : MonoBehaviour {//for finding and storing paths 

    Tile[,] Map; //original environment
    Student[] students;//students 
    public int depth;
    public float deltaTime;//time between each time slice;
    private float currentTime;//current time
    Dictionary<Student, List<Tile>> paths;//save student paths here
    List<Tile[,]> TimeSpace;//go to next position in list for next time slice,  current time + (position in list/(1/deltaTime)) is the time at which the map should look like this position in the list
    Dictionary<Student,Dictionary<Tile, int>> BackWardsA;//save dist so far from dest for each path being found, ie there will be # of students positions in this list  

    void Update()
    {
    }

    public IEnumerator timeElapsed()
    {
        while (true)
        {
            TimeSpace.RemoveAt(0);
            currentTime = Time.time;
            foreach (Student student in students)
            {
                if (paths[student].Count > 0)
                {
                    paths[student].RemoveAt(0);
                }
            }
            newTimeSlice();
            yield return new WaitForSecondsRealtime(deltaTime);
        }
    }

    public void newTimeSlice()//adding a new time slice
    {
        TimeSpace.Add((Tile[,])Map.Clone());
    }

    public Tile getNextTile(Student student)
    {
        return paths[student][0];
    }

    public void reachedTile(Student student)
    {
        paths[student].RemoveAt(0);
    }

    public List<Tile> getPath(Student student)
    {
        return paths[student];
    }

    void Start()
    {
        StartCoroutine("timeElapsed");
        
    }

    public float getCurrent()
    {
        return this.currentTime;
    }

    public void init(Tile [,] tiles, Student [] etudiants)
    {
        Map = tiles;
        TimeSpace = new List<Tile[,]>();
        BackWardsA = new Dictionary<Student, Dictionary<Tile, int>>();
        paths = new Dictionary<Student, List<Tile>>();
        currentTime = Time.time;
        setManhattan();
        for (int i = 0; i<depth; i++)
        {
            newTimeSlice();
        }
        students = etudiants;
        foreach (Student student in students)
        {
            paths.Add(student, new List<Tile>());
            BackWardsA.Add(student, new Dictionary<Tile, int>());
            TimeSpace[0][(int)student.transform.position.x / Map[0, 0].getScale(), (int)student.transform.position.z / Map[0, 0].getScale()].setPassable(false);
        }
    }

    public void newTarget(Student student)
    {
        BackWardsA[student] = new Dictionary<Tile, int>(); //wipe true distance heuristic
        paths[student] = new List<Tile>();
    }

    public List<Tile> findPathForMe(Student student,Tile target)//called by student, returns path to that student 
    {
        Tile source;
        List<Tile> path = paths[student];
        Tile[,] starting = TimeSpace[path.Count];
        if (path.Count == 0)// if path is empty, start from tile student is at currently 
        {
            source = starting[(int)Mathf.Round(student.transform.position.x / Map[0, 0].getScale()), (int)Mathf.Round(student.transform.position.z / Map[0, 0].getScale())];
            spaceTimeAStar(source, target, student);
        }
        else
        {
            spaceTimeAStar(path[path.Count], target, student);        
        }
        return new List<Tile>();
    }

    public delegate int heuristic(Tile source, Tile dest);//functional programming! can use different heuristics
    heuristic Admissible;

    public int ManhattanDist(Tile source, Tile dest)//h function
    {
        float xSourcePos = source.pos.x/source.getScale();//mapping to x position in array 
        float ySourcePos = source.pos.z/source.getScale();//mapping to y position in array
        float xDestPos = dest.pos.x / dest.getScale();
        float yDestPos = dest.pos.z / dest.getScale();
        int result = (int)(Mathf.Abs(xDestPos - xSourcePos) + Mathf.Abs((yDestPos - ySourcePos)));
        return result;
    }

    public void setManhattan()
    {
        Admissible = ManhattanDist;
    }

    public int spaceTimeAStar(Tile source, Tile dest, Student student)//call this with source as the last position in the students current path ie list foo = paths[student]; source = foo[foo.Count]
    {
        Dictionary<Tile, int> Open = new Dictionary<Tile, int>(); //map a tile to its timeSlice (int)
        Dictionary<Tile,int> Closed = new Dictionary<Tile,int>();
        SortedList<int,Tile> openF = new SortedList<int,Tile>(new DuplicateKeyComparer<int>());
        SortedList<int,Tile> closeF = new SortedList<int,Tile>(new DuplicateKeyComparer<int>()); //map a F value to a Tile 
        Dictionary<Tile, int> distFromSource = new Dictionary<Tile, int>();
        int timeSlice = paths[student].Count - 1; //the latest time at which the student has a planned action, the timeslice at which source is currently
        Tile current = source;
        Open.Add(current, timeSlice);
        int h;
        Tile optimal = null;
        int optH = 1000000;
        if (!BackWardsA[student].TryGetValue(Map[(int)current.pos.x/current.getScale(),(int)current.pos.z/current.getScale()],out h))//if true distance has already been found
        { 
            SpatialAStar(Map[(int)dest.pos.x/dest.getScale(),(int)dest.pos.z/dest.getScale()], Map[(int)source.pos.x / source.getScale(), (int)source.pos.z / source.getScale()], student);//look for it by casting destination to its position in the map
            h = BackWardsA[student][Map[(int)source.pos.x / source.getScale(), (int)source.pos.z / source.getScale()]];
        }
        openF.Add(0 + h, current);
        distFromSource.Add(current, 0);
        while(openF.Count > 0)
        {
            IList<int> keys = openF.Keys;
            int lowestF = keys[0];
            current = openF.Values[0];//if a tile is in the open list, its complement in Map's true distance from dest must have been saved in BackwardsA[student]
            //because every time we explore a node we find it's true distance
            timeSlice = Open[current];
            if (current.pos.x == dest.pos.x && current.pos.z == dest.pos.z)// if we have found our desired location
            {
                break;
            }
            int xOpenPos = (int)current.pos.x / current.getScale();
            int yOpenPos = (int)current.pos.z / current.getScale();
            Tile[] neighbours = new Tile[5];
            if (timeSlice + 1 < TimeSpace.Count)//if you will be generating neighbours beyond the allowable depth, continue 
            {
                neighbours[0] = TimeSpace[timeSlice + 1][xOpenPos, yOpenPos];//same tile, 1 step into the future
                neighbours[1] = TimeSpace[timeSlice + 1][xOpenPos + 1, yOpenPos];//tile to the right, 1 step into the future
                neighbours[2] = TimeSpace[timeSlice + 1][xOpenPos - 1, yOpenPos];//tile to the left, 1 step into the future
                neighbours[3] = TimeSpace[timeSlice + 1][xOpenPos, yOpenPos + 1];
                neighbours[4] = TimeSpace[timeSlice + 1][xOpenPos, yOpenPos - 1];
                for (int i = 0; i < neighbours.Length; i++)
                {
                    if (!neighbours[i].getPassable())
                    {
                        continue;
                    }

                    int thisIterationCost = distFromSource[current] + Admissible(current, neighbours[i]);//current must be saved in distFromSource, guaranteed by algorithm
                    int currentCost;
                    if (!distFromSource.TryGetValue(neighbours[i], out currentCost))//if neighbours[i] is not explored, set current cost high, if it is, set its current dist from source as the value saved in distFromSource
                    {
                        currentCost = 1000000;
                    }
                    if (Open.ContainsKey(neighbours[i]))
                    {
                        if (currentCost <= thisIterationCost)
                        {
                            continue;
                        }
                    }
                    else if (Closed.ContainsKey(neighbours[i]))
                    {
                        if (currentCost <= thisIterationCost)
                        {
                            continue;
                        }
                        Closed.Remove(neighbours[i]);
                        closeF.RemoveAt(closeF.IndexOfValue(neighbours[i]));
                        Open.Add(neighbours[i], timeSlice + 1);
                        openF.Add(thisIterationCost + h, neighbours[i]);//save f value of neighbours[i]
                    }
                    else
                    {
                        Open.Add(neighbours[i], timeSlice + 1);
                        openF.Add(thisIterationCost + h, neighbours[i]);//save f value of neighbours[i]
                    }
                    if (!BackWardsA[student].TryGetValue(Map[(int)neighbours[i].pos.x / neighbours[i].getScale(), (int)neighbours[i].pos.z / neighbours[i].getScale()], out h))
                    {
                        SpatialAStar(Map[(int)dest.pos.x / dest.getScale(), (int)dest.pos.z / dest.getScale()], Map[(int)neighbours[i].pos.x / neighbours[i].getScale(), (int)neighbours[i].pos.z / neighbours[i].getScale()], student);//look for it by casting destination to its position in the map
                        h = BackWardsA[student][Map[(int)neighbours[i].pos.x / neighbours[i].getScale(), (int)neighbours[i].pos.z / neighbours[i].getScale()]];;
                    }//get true distance heuristic for neighbour
                    
                    distFromSource[neighbours[i]] =  thisIterationCost;
                    neighbours[i].setParent(current);
                }
            }
            else //timeSlice + 1 >= TimeSpace.Count ie it has passed the allowable depth
            {
                int currentH = lowestF - distFromSource[current];//find H value, ie true distance by taking f - g = h
                if (currentH < optH)
                {
                    optimal = current;
                    optH = currentH;
                }
            }
            Closed.Add(current, timeSlice);
            closeF.Add(lowestF, current);
            Open.Remove(current);
            openF.RemoveAt(openF.IndexOfValue(current));
        }
        createPath(optimal,student);
        if (current.pos.x == dest.pos.x && current.pos.z == dest.pos.z)// if we have found our desired location
        {
            return 1;
        }
        return 0;
    }



    //spatialAStar returns the true distance heuristic used in spaceTimeAStar by saving the dist from the destination in the student indexed dictionary
    //input source as the destination, and dest as the tile visited by spaceTimeAStar

    public int SpatialAStar(Tile source, Tile dest, Student student)//working!
    {
        Dictionary<Tile, int> toSave = BackWardsA[student];//toSave is the Dictionary in which we save dist from source ie g value 
        List<Tile> Open = new List<Tile>();
        List<Tile> Closed = new List<Tile>();
        List<int> openF = new List<int>();//f = g + h values in open list
        List<int> closeF = new List<int>();//f = g + h values in closed list
        Open.Add(source);
        openF.Add(0 + Admissible(source,dest));
        toSave[source] = 0;//distance from source is 0
        foreach (Tile key in toSave.Keys)//add all searched tiles, in a manhattan dist. graph with unit cost it is easy to prove that tiles are searched with optimal g value
        {
            int f = toSave[key] + Admissible(key, dest);//toSave[key] is g value of each saved tile, admissible is manhattan dist. heuristic to destination
            int j;
            for (j = 0; j<openF.Count; j++)
            {
                if (openF[j] > f)
                {
                    break;
                }
            }
            Open.Insert(j, key);
            openF.Insert(j, f);
        }
        Tile current = null;
        while (Open.Count > 0)
        {
            current = Open[0];
            if (ReferenceEquals (current,dest))
            {
                break;
            }
            
            float xOpenPos = current.pos.x / current.getScale();//mapping to x position in array 
            float yOpenPos = current.pos.z / current.getScale();//mapping to y position in array
            Tile[] neighbours = new Tile[4];
            neighbours[0] = Map[(int)xOpenPos + 1, (int)yOpenPos];
            neighbours[1] = Map[(int)xOpenPos - 1, (int)yOpenPos];
            neighbours[2] = Map[(int)xOpenPos, (int)yOpenPos + 1];
            neighbours[3] = Map[(int)xOpenPos, (int)yOpenPos - 1];
            for (int k = 0; k < neighbours.Length; k++)
            {
                if (!neighbours[k].getPassable())
                {
                    continue;
                }
 
                int thisIterationCost;
                int currentCost;
                toSave.TryGetValue(current, out thisIterationCost);
                if (!toSave.TryGetValue(neighbours[k], out currentCost))//if neighbours hasnt been explored yet
                {
                    currentCost = 100000000;
                }
                thisIterationCost += Admissible(current, neighbours[k]);//add weight heuristic, this is now g value for the neighbour
                int openFound = -1;
                int closedFound = -1;
                for (int j = 0; j<Open.Count ; j++)
                {
                    if (System.Object.ReferenceEquals(Open[j],neighbours[k]))
                    {
                        openFound = j;
                    }
                }
                for (int j = 0; j < Closed.Count; j++)
                {
                    if (System.Object.ReferenceEquals(Closed[j], neighbours[k]))
                    {
                        closedFound = j;
                    }
                }
                if (openFound >= 0)
                {
                    if (currentCost <= thisIterationCost)
                    {
                        continue;
                    }
                }
                else if (closedFound >= 0)
                {
                    if (currentCost <= thisIterationCost)
                    {
                        continue;
                    }
                    Debug.Log("Searched node before shortest path");
                    int j;
                    for(j = 0; j<Open.Count; j++)//place neighbours[k] into open list as it now has a new f value
                    {
                        if (openF[j]>thisIterationCost)//find the place where the open f value is first greater than this neighbours f value
                        {
                            break;
                        }

                    }
                    openF.Insert(j, thisIterationCost + Admissible(neighbours[k], dest));
                    Open.Insert(j, neighbours[k]);
                    closeF.RemoveAt(closedFound);
                    Closed.RemoveAt(closedFound);

                }
                else
                {
                    int j;
                    for (j = 0; j < Open.Count; j++)//place neighbours[k] into open list as it is finally a frontier node
                    {
                        if (openF[j] > thisIterationCost + Admissible(neighbours[k],dest))//find the place where the open f value is first greater than this neighbours f value
                        {
                            break;
                        }
                    }
                    openF.Insert(j, thisIterationCost + Admissible(neighbours[k], dest));
                    Open.Insert(j, neighbours[k]);
                }
                toSave[neighbours[k]] = thisIterationCost;
                neighbours[k].setParent(current);
            }
            int position = Open.IndexOf(current);
            Closed.Add(Open[position]);
            closeF.Add(openF[position]);//closed doesnt need to be a priority queue
            Open.RemoveAt(position);
            openF.RemoveAt(position);

        }
        if (!ReferenceEquals(current,dest))
        {
            return -1;
        }
        return 1;

    }
    
    public int createPath(Tile dest, Student student)
    {
        Debug.Log("New Student");
        Tile current = dest;
        while (true)
        {
            paths[student].Add(current);
            Debug.Log("x: " + current.pos.x + " z: " + current.pos.z);
            current = current.getParent();
        }
        markResTable(student);
        return 1;
    }
    public int markResTable(Student student)
    {
        List<Tile> path = paths[student];
        for (int i = 0; i<path.Count; i++)
        {
            Tile mark = path[i];
            TimeSpace[i][(int)mark.pos.x / mark.getScale(), (int)mark.pos.z / mark.getScale()].setPassable(false);
        }
        return 1;
    }
}


