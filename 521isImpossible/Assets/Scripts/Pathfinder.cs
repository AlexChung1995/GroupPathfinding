using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Pathfinder : MonoBehaviour
{//for finding and storing paths 

    public Tile[,] Map; //original environment
    Student[] students;//students 
    public int depth;
    public float deltaTime;//time between each time slice;
    Dictionary<Student, List<Tile>> paths;//save student paths here
    List<Tile[,]> TimeSpace;//go to next position in list for next time slice,  current time + (position in list/(1/deltaTime)) is the time at which the map should look like this position in the list
    Dictionary<Student, Dictionary<Tile, int>> BackWardsA;//save dist so far from dest for each path being found, ie there will be # of students positions in this list  

    void Update()
    {
    }

    public IEnumerator timeElapsed()
    {
        while (true)
        {
            
            for (int i = 0; i < students.Length; i++)
            {
                Student student = students[i];  
                if (paths[student].Count <= 1)
                {
                    findPathForMe(student, student.getTarget());
                }
                if (paths[student].Count > 0)
                {
                    student.transform.position = new Vector3(paths[student][0].pos.x, 0.5f, paths[student][0].pos.z);
                    //Debug.Log("Pos:" + student.transform.position.x + " " + student.transform.position.z + "dest: " + student.getTarget().pos.x + " " + student.getTarget().pos.z);
                    paths[student].RemoveAt(0);
                }
            }
            TimeSpace.RemoveAt(0);
            newTimeSlice();
            yield return new WaitForSecondsRealtime(deltaTime);
        }
    }

    public void newTimeSlice()//adding a new time slice
    {//copy every tile in Map
        Tile[,] newTime = new Tile[Map.GetLength(0), Map.GetLength(1)];
        for (int i = 0; i < Map.GetLength(0); i++)
        {
            for (int j = 0; j < Map.GetLength(1); j++)
            {
                Tile newTile = new Tile(Map[i, j].getScale(), Map[i, j].getPassable(), Map[i, j].pos);
                newTime[i, j] = newTile;
            }
        }
        TimeSpace.Add(newTime);
    }//despite O(m^2) run time, this completes near instantaeneously 

 
    public List<Tile> getPath(Student student)
    {
        return paths[student];
    }

    void Start()
    {

    }

    public void init(Tile[,] tiles, Student[] etudiants)
    {
        Map = tiles;
        TimeSpace = new List<Tile[,]>();
        BackWardsA = new Dictionary<Student, Dictionary<Tile, int>>();
        paths = new Dictionary<Student, List<Tile>>();
        setManhattan();
        for (int i = 0; i < depth; i++)
        {
            newTimeSlice();
        }
        students = etudiants;

        for (int i = 0; i < students.Length; i++)
        {
            Student student = students[i];
            paths.Add(student, new List<Tile>());
            Search root = new Search(TimeSpace[0][(int)student.transform.position.x / Map[0, 0].getScale(), (int)student.transform.position.z / Map[0, 0].getScale()]);
            Search current = root;
            for (int j = 0; j <= ((float)i / ((float)students.Length / (float)depth)); j++)
            {
                Search next = new Search(TimeSpace[(0)][(int)student.transform.position.x / Map[0, 0].getScale(), (int)student.transform.position.z / Map[0, 0].getScale()]);
                next.setParent(current);
                current = next;
            }
            BackWardsA.Add(student, new Dictionary<Tile, int>());
            createPath(current, student);
            student.init();
        }
        StartCoroutine("timeElapsed");
    }

    public void newTarget(Student student)
    {
        BackWardsA[student].Clear(); //wipe true distance heuristic
    }

    public delegate int heuristic(Tile source, Tile dest);//functional programming! can use different heuristics
    heuristic Admissible;

    public int ManhattanDist(Tile source, Tile dest)//h function
    {
        float xSourcePos = source.pos.x / source.getScale();//mapping to x position in array
        float ySourcePos = source.pos.z / source.getScale();//mapping to y position in array
        float xDestPos = dest.pos.x / dest.getScale();
        float yDestPos = dest.pos.z / dest.getScale();
        int result = (int)(Mathf.Abs(xDestPos - xSourcePos) + Mathf.Abs((yDestPos - ySourcePos)));
        return result;
    }

    public void setManhattan()
    {
        Admissible = ManhattanDist;
    }

    public int findPathForMe(Student student, Tile target)//called by student, returns path to that student 
    {
        Tile source;
        List<Tile> path = paths[student];
        Tile[,] starting = TimeSpace[path.Count];
        int result;
        if (path.Count == 0)// if path is empty, start from tile student is at currently 
        {
            Debug.Log("Finding Path no path currently");
            source = starting[(int)Mathf.Round(student.transform.position.x / Map[0, 0].getScale()), (int)Mathf.Round(student.transform.position.z / Map[0, 0].getScale())];
            result = spaceTimeAStar(source, target, student);
        }
        else
        {
            Debug.Log("Finding Path, still has planned path");
            result = spaceTimeAStar(path[path.Count - 1], target, student);
        }
        return result;
    }

    public int spaceTimeAStar(Tile source, Tile dest, Student student)//call this with source as the last position in the students current path ie list foo = paths[student]; source = foo[foo.Count]
    {
        Dictionary<Search, int> Open = new Dictionary<Search, int>(); //map a tile to its timeSlice (int)
        Dictionary<Search, int> Closed = new Dictionary<Search, int>();
        SortedList<int, Search> openF = new SortedList<int, Search>(new DuplicateKeyComparer<int>());
        SortedList<int, Search> closeF = new SortedList<int, Search>(new DuplicateKeyComparer<int>()); //map a F value to a Tile 
        Dictionary<Tile, int> distFromSource = new Dictionary<Tile, int>();//can be a <Tile,int> dictionary because every tile regardless of node has the same dist from source
        int timeSlice = paths[student].Count - 1; //the latest time at which the student has a planned action, the timeslice at which source is currently
        Search current = new Search(source);
        Open.Add(current, timeSlice);
        int h;
        Search optimal = null;
        int optH = 1000000;
        if (!BackWardsA[student].TryGetValue(Map[(int)current.getTile().pos.x / current.getTile().getScale(), (int)current.getTile().pos.z / current.getTile().getScale()], out h))//if true distance has already been found
        {
            SpatialAStar(Map[(int)dest.pos.x / dest.getScale(), (int)dest.pos.z / dest.getScale()], Map[(int)source.pos.x / source.getScale(), (int)source.pos.z / source.getScale()], student);//look for it by casting destination to its position in the map
            h = BackWardsA[student][Map[(int)source.pos.x / source.getScale(), (int)source.pos.z / source.getScale()]];
        }
        openF.Add(0 + h, current);
        distFromSource.Add(current.getTile(), 0);
        while (Open.Count > 0)
        {
            IList<int> keys = openF.Keys;
            int lowestF = keys[0];
            current = openF.Values[0];//if a tile is in the open list, its complement in Map's true distance from dest must have been saved in BackwardsA[student]
            //because every time we explore a node we find it's true distance
            timeSlice = Open[current];//sorted list works as expected
            if (current.getTile().pos.x == dest.pos.x && current.getTile().pos.z == dest.pos.z)// if we have found our desired location
            {
                Debug.Log("Found, timeslice: " + timeSlice);
                for (int k = timeSlice; k<TimeSpace.Count - 1; k++)
                {
                    Search stay = new Search(TimeSpace[k + 1][(int)dest.pos.x, (int)dest.pos.z]);//same tile, 1 step into the future
                    stay.setParent(current);
                    current = stay;
                }
                createPath(current, student);
                return 1;
            }
            int xOpenPos = (int)current.getTile().pos.x / current.getTile().getScale();
            int yOpenPos = (int)current.getTile().pos.z / current.getTile().getScale();
            Search[] neighbours = new Search[5];
            if (timeSlice + 1 < TimeSpace.Count)//if you will be generating neighbours beyond the allowable depth, continue 
            {
                neighbours[0] = new Search(TimeSpace[timeSlice + 1][xOpenPos, yOpenPos]);//same tile, 1 step into the future
                neighbours[1] = new Search(TimeSpace[timeSlice + 1][xOpenPos + 1, yOpenPos]);//tile to the right, 1 step into the future
                neighbours[2] = new Search(TimeSpace[timeSlice + 1][xOpenPos - 1, yOpenPos]);//tile to the left, 1 step into the future
                neighbours[3] = new Search(TimeSpace[timeSlice + 1][xOpenPos, yOpenPos + 1]);
                neighbours[4] = new Search(TimeSpace[timeSlice + 1][xOpenPos, yOpenPos - 1]);
                for (int i = 0; i < neighbours.Length; i++)
                {
                    if (!neighbours[i].getTile().getPassable())
                    {
                        continue;
                    }

                    int thisIterationCost = distFromSource[current.getTile()] + Admissible(current.getTile(), neighbours[i].getTile());//current must be saved in distFromSource, guaranteed by algorithm
                    int currentCost;
                    if (!distFromSource.TryGetValue(neighbours[i].getTile(), out currentCost))//if neighbours[i] is not explored, set current cost high, if it is, set its current dist from source as the value saved in distFromSource
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
                        if (!BackWardsA[student].TryGetValue(Map[(int)neighbours[i].getTile().pos.x / neighbours[i].getTile().getScale(), (int)neighbours[i].getTile().pos.z / neighbours[i].getTile().getScale()], out h))
                        {//get true distance heuristic for neighbour
                            SpatialAStar(Map[(int)dest.pos.x / dest.getScale(), (int)dest.pos.z / dest.getScale()], Map[(int)neighbours[i].getTile().pos.x / neighbours[i].getTile().getScale(), (int)neighbours[i].getTile().pos.z / neighbours[i].getTile().getScale()], student);//look for it by casting destination to its position in the map
                            h = BackWardsA[student][Map[(int)neighbours[i].getTile().pos.x / neighbours[i].getTile().getScale(), (int)neighbours[i].getTile().pos.z / neighbours[i].getTile().getScale()]]; ;
                        }
                        Open.Add(neighbours[i], timeSlice + 1);
                        openF.Add(thisIterationCost + h, neighbours[i]);//save f value of neighbours[i]
                    }
                    else
                    {
                        if (!BackWardsA[student].TryGetValue(Map[(int)neighbours[i].getTile().pos.x / neighbours[i].getTile().getScale(), (int)neighbours[i].getTile().pos.z / neighbours[i].getTile().getScale()], out h))
                        {//get true distance heuristic for neighbour
                            SpatialAStar(Map[(int)dest.pos.x / dest.getScale(), (int)dest.pos.z / dest.getScale()], Map[(int)neighbours[i].getTile().pos.x / neighbours[i].getTile().getScale(), (int)neighbours[i].getTile().pos.z / neighbours[i].getTile().getScale()], student);//look for it by casting destination to its position in the map
                            h = BackWardsA[student][Map[(int)neighbours[i].getTile().pos.x / neighbours[i].getTile().getScale(), (int)neighbours[i].getTile().pos.z / neighbours[i].getTile().getScale()]]; ;
                        }
                        Open.Add(neighbours[i], timeSlice + 1);
                        openF.Add(thisIterationCost + h, neighbours[i]);//save f value of neighbours[i]
                    }


                    distFromSource[neighbours[i].getTile()] = thisIterationCost;
                    neighbours[i].setParent(current);
                }
            }
            else //timeSlice + 1 >= TimeSpace.Count ie it has passed the allowable depth
            {
                int currentH = lowestF - distFromSource[current.getTile()];//find H value, ie true distance by taking f - g = h
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
        createPath(optimal, student);
        return 0;
    }



    //spatialAStar returns the true distance heuristic used in spaceTimeAStar by saving the dist from the destination in the student indexed dictionary
    //input source as the destination, and dest as the tile visited by spaceTimeAStar

    public int SpatialAStar(Tile source, Tile dest, Student student)
    {
        Dictionary<Tile, int> toSave = BackWardsA[student];//toSave is the Dictionary in which we save dist from source ie g value 
        List<Search> Open = new List<Search>();
        List<Search> Closed = new List<Search>();
        List<int> openF = new List<int>();//f = g + h values in open list
        List<int> closeF = new List<int>();//f = g + h values in closed list
        Search current = new Search(source);
        Open.Add(current);
        openF.Add(0 + Admissible(current.getTile(), dest));
        foreach (Tile key in toSave.Keys)//add all searched tiles, in a manhattan dist. graph with unit cost it is easy to prove that tiles are searched with optimal g value
        {
            if (key == source)
            {
                continue;
            }
            int f = toSave[key] + Admissible(key, dest);//toSave[key] is g value of each saved tile, admissible is manhattan dist. heuristic to destination
            int j;
            for (j = 0; j < openF.Count; j++)
            {
                if (openF[j] > f)
                {
                    break;
                }
            }
            Open.Insert(j, new Search(key));
            openF.Insert(j, f);
        }
        toSave[source] = 0;//distance from source is 0
        while (Open.Count > 0)
        {
            current = Open[0];
            float xOpenPos = current.getTile().pos.x / current.getTile().getScale();//mapping to x position in array 
            float yOpenPos = current.getTile().pos.z / current.getTile().getScale();//mapping to y position in array
            if (ReferenceEquals(current.getTile(), dest))
            {
                break;
            }
            Search[] neighbours = new Search[4];
            neighbours[0] = new Search(Map[(int)xOpenPos + 1, (int)yOpenPos]);
            neighbours[1] = new Search(Map[(int)xOpenPos - 1, (int)yOpenPos]);
            neighbours[2] = new Search(Map[(int)xOpenPos, (int)yOpenPos + 1]);
            neighbours[3] = new Search(Map[(int)xOpenPos, (int)yOpenPos - 1]);
            for (int k = 0; k < neighbours.Length; k++)
            {
                if (!neighbours[k].getTile().getPassable())
                {
                    continue;
                }

                int thisIterationCost;
                int currentCost;
                toSave.TryGetValue(current.getTile(), out thisIterationCost);
                if (!toSave.TryGetValue(neighbours[k].getTile(), out currentCost))//if neighbour hasnt been explored yet
                {
                    currentCost = 100000000;
                }
                thisIterationCost += Admissible(current.getTile(), neighbours[k].getTile());//add weight heuristic, this is now g value for the neighbour
                int openFound = -1;
                int closedFound = -1;
                for (int j = 0; j < Open.Count; j++)
                {
                    if (System.Object.ReferenceEquals(Open[j].getTile(), neighbours[k].getTile()))
                    {
                        openFound = j;
                    }
                }
                for (int j = 0; j < Closed.Count; j++)
                {
                    if (System.Object.ReferenceEquals(Closed[j].getTile(), neighbours[k].getTile()))
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
                    int j;
                    for (j = 0; j < Open.Count; j++)//place neighbours[k] into open list as it now has a new f value
                    {
                        if (openF[j] > thisIterationCost + Admissible(neighbours[k].getTile(), dest))//find the place where the open f value is first greater than this neighbours f value
                        {
                            break;
                        }

                    }
                    openF.Insert(j, thisIterationCost + Admissible(neighbours[k].getTile(), dest));
                    Open.Insert(j, neighbours[k]);
                    closeF.RemoveAt(closedFound);
                    Closed.RemoveAt(closedFound);
                }
                else
                {
                    int j;
                    for (j = 0; j < Open.Count; j++)//place neighbours[k] into open list as it is finally a frontier node
                    {
                        if (openF[j] > thisIterationCost + Admissible(neighbours[k].getTile(), dest))//find the place where the open f value is first greater than this neighbours f value
                        {
                            break;
                        }
                    }
                    openF.Insert(j, thisIterationCost + Admissible(neighbours[k].getTile(), dest));
                    Open.Insert(j, neighbours[k]);
                }
                toSave[neighbours[k].getTile()] = thisIterationCost;
                neighbours[k].setParent(current);
            }
            int position = Open.IndexOf(current);
            Closed.Add(Open[position]);
            closeF.Add(openF[position]);
            Open.RemoveAt(position);
            openF.RemoveAt(position);

        }
        if (!ReferenceEquals(current.getTile(), dest))
        {
            return -1;
        }
        return 1;

    }

    public int createPath(Search dest, Student student)
    {
        Search current = dest;
        int insertPos = paths[student].Count;
        while (current.getParent() != null)
        {
            paths[student].Insert(insertPos, current.getTile());
            current = current.getParent();
        }
        Debug.Log("Created path of length: " + paths[student].Count);
        markResTable(student);
        return 1;
    }
    public int markResTable(Student student)//mark reservation table position as impassable
    {// need to add last position reservation functionality 
        List<Tile> path = paths[student];
        for (int i = 0; i < path.Count; i++)
        {
            Tile mark = path[i];
            TimeSpace[i][(int)mark.pos.x / mark.getScale(), (int)mark.pos.z / mark.getScale()].setPassable(false);
        }
        return 1;
    }
}


