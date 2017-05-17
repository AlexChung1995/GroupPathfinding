using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Student : MonoBehaviour
{

    private float speed;
    private Rigidbody rb;
    private Dictionary<Professor, Tile> last4Profs;
    private Professor lru;//least recently used prof 
    private CompositeNode root;//use this for the behaviour tree 
    private Pathfinder silver;
    private Professor assigned;
    private Tile target;
    private Plaque plaque;
    void Start()
    {

    }
    void FixedUpdate()
    {
    }
    void Update()
    {

    }
    public void init()//handle all initiation here
    {
        last4Profs = new Dictionary<Professor, Tile>();
        initBehaviourTree();//instantiate behaviour tree
        root.run(root.finished);
    }

    public void initBehaviourTree()
    {
        root = new CompositeNode(this, null);
        root.setRoot();
        root.initSequence();
        DecoratorNode idle = new DecoratorNode(this, root.returnToMe);
        idle.setIdle();
        root.AddChild(idle);
        CompositeNode idleChild = new CompositeNode(this, idle.returnToMe);
        idleChild.initSequence();
        idle.setChild(idleChild);
        LeafNode setRandom = new LeafNode(this, idleChild.returnToMe, this.setRandom);
        idleChild.AddChild(setRandom);
        LeafNode randomMove = new LeafNode(this, idleChild.returnToMe, this.beginJourney);
        idleChild.AddChild(randomMove);
        //now move on to the second child of root
        CompositeNode getProfLocation = new CompositeNode(this, root.returnToMe);
        getProfLocation.initSelector();
        root.AddChild(getProfLocation);
        LeafNode profKnown = new LeafNode(this, getProfLocation.returnToMe, this.profSaved);
        getProfLocation.AddChild(profKnown);
        CompositeNode explorePlaques = new CompositeNode(this, getProfLocation.returnToMe);
        explorePlaques.initSelector();
        getProfLocation.AddChild(explorePlaques);
        for (int i = 0; i < 6; i++)
        {
            CompositeNode PlaqueFinder = new CompositeNode(this, explorePlaques.returnToMe);
            PlaqueFinder.initSelector();
            explorePlaques.AddChild(PlaqueFinder);
            LeafNode setPlaque = new LeafNode(this, PlaqueFinder.returnToMe, this.setTargetPlaque);
            PlaqueFinder.AddChild(setPlaque);
            LeafNode plaqueMove = new LeafNode(this, PlaqueFinder.returnToMe, this.beginJourney);
            PlaqueFinder.AddChild(plaqueMove);
            LeafNode isProf = new LeafNode(this, PlaqueFinder.returnToMe, this.isAdvisor);
            PlaqueFinder.AddChild(isProf);
        }
        LeafNode finalMove = new LeafNode(this, root.returnToMe, this.beginJourney);
        root.AddChild(finalMove);
    }

    public Professor getAssigned()
    {
        return this.assigned;
    }
    public void setAssigned(Professor p)
    {
        rb = GetComponent<Rigidbody>();
        silver = GameObject.FindObjectOfType<Pathfinder>();
        lru = p;
        this.assigned = p;
    }
    public void setTarget(Tile t)
    {
        target = t;
    }
    public Tile getTarget()
    {
        return this.target;
    }
    public void setSpeed(float s)
    {
        speed = s;
    }
    public float getSpeed()
    {
        return this.speed;
    }
    public bool isAdjacent()
    {
        return false;
    }
    public int isAdvisor(BehaviourNode.callReturn finished)//implement angular sweep here 
    {
        if (ReferenceEquals(assigned, plaque.getProf()))
        {
            target = plaque.getProf().getLocation();
            finished(1);
            return 1;
        }
        else
        {
            finished(-1);
            return -1;
        }
    }

    public int setTargetPlaque(BehaviourNode.callReturn finished)//ugly, fix
    {
        Plaque[] plaques = GameObject.FindObjectsOfType<Plaque>();
        if (target == null)
        {
            target = silver.Map[(int)plaques[0].transform.position.x, (int)plaques[0].transform.position.z];
        }
        for (int i = 0; i < plaques.Length; i++)
        {
            if (plaques[i].transform.position.x == target.pos.x && plaques[i].transform.position.z == target.pos.z)
            {
                if (i != plaques.Length)
                {
                    target = silver.Map[(int)plaques[i + 1].transform.position.x, (int)plaques[i + 1].transform.position.z];
                    plaque = plaques[i + 1];
                }
                else
                {
                    target = silver.Map[(int)plaques[0].transform.position.x, (int)plaques[0].transform.position.z];
                    plaque = plaques[0];
                }
                return 1;
            }
        }
        return 0;

    }

    public int profSaved(BehaviourNode.callReturn finished)
    {
        Tile t;
        if (last4Profs.TryGetValue(assigned, out t))
        {
            Debug.Log("Saved Prof");
            target = t;
            finished(1);
            return 1;
        }
        else
        {
            Debug.Log("Not Saved Prof");
            finished(-1);
            return -1;
        }
    }

    public int setRandom(BehaviourNode.callReturn finished)
    {
        Debug.Log("Going to Random Location");
        target = silver.Map[Random.Range(0, silver.Map.GetLength(0)), Random.Range(0, silver.Map.GetLength(1))];
        while (!target.getPassable())
        {
            target = silver.Map[Random.Range(0, silver.Map.GetLength(0)), Random.Range(0, silver.Map.GetLength(1))];
        }
        finished(1);
        return 1;
    }

    //find path is now done in pathfinder constantly, should always have a path to move along 

    public int beginJourney(BehaviourNode.callReturn finished)//call this from a leafnode
    {
        StartCoroutine(moving(finished));
        Debug.Log("Starting movement: " + target.pos.x);
        return 0;
    }

    public IEnumerator moving(BehaviourNode.callReturn finished)//call from function above, instantiate in leafnode with LeafNode.DoSomething = student.beginJourney;
    {
        yield return new WaitUntil(() => gameObject.transform.position.x == target.pos.x && gameObject.transform.position.z == target.pos.z);
        Debug.Log("Finished Moving");
        finished(1);

    }
}

