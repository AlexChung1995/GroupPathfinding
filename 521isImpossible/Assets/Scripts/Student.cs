using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Student : MonoBehaviour {

    private float speed;
    private Rigidbody rb;
    Dictionary<Professor, Tile> last4Profs;
    Professor lru;//least recently used prof 
    BehaviourNode root;//use this for the behaviour tree 
    Pathfinder silver;
    Professor assigned;
    Tile target;
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

        //instantiate behaviour tree
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
    public List<GameObject> canSee()//implement angular sweep here 
    {
        return new List<GameObject>();
    }

    public void findPath()//BehaviourNode.callReturn finished)//call this when theres a new target location
    {
        Debug.Log("New Student");
        silver.findPathForMe(this, target);
    }

    public void beginJourney()//BehaviourNode.callReturn finished)//call this from a leafnode
    {
        StartCoroutine("moving");
    }

    public IEnumerator moving()//BehaviourNode.callReturn finished)//call from function above, instantiate in leafnode with LeafNode.DoSomething = student.beginJourney;
    {
        Tile tile = silver.getNextTile(this);
        while (tile != null)
        {
            tile = silver.getNextTile(this);
            /*if (this.transform.position.x == tile.pos.x && this.transform.position.z == tile.pos.z)
            {
                silver.reachedTile(this);
            }
            Vector3 movement = new Vector3(tile.pos.x - this.transform.position.x, 0.0f, tile.pos.z - this.transform.position.z) / Mathf.Sqrt(Mathf.Pow(tile.pos.x - this.transform.position.x, 2) + Mathf.Pow(tile.pos.z - this.transform.position.z, 2));
            rb.AddForce(movement * speed);*/
            this.transform.position = new Vector3(tile.pos.x, 0.5f, tile.pos.z);
            yield return new WaitForSecondsRealtime(silver.getCurrent () + silver.deltaTime - Time.time);
        }
        //finished(1);    
    }
    public void wait(BehaviourNode.callReturn finished)
    {

    }
}
