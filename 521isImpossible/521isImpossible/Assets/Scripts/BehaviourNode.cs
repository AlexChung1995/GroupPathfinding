using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class BehaviourNode{
    protected Student student;
	protected int status; //1 if success, 0 if running, -1 if failure
    public delegate int callReturn(int success);
    public callReturn finished;//set by parent, return to parent
    public callReturn returnToMe;//send this to children
    public delegate int doSomething(callReturn returnToMe);
    public doSomething run;


    public BehaviourNode(Student etudiant, callReturn parent)
    {
        student = etudiant;
        finished = parent;
        status = -2; 

    }

    //every node saves a callReturn finished which is set by its parent in initialize(Student etudiant, callReturn finished), this is sent back to its parent upon completion
    //every node also sets a callReturn returnToMe which is the analog of finished: a function that it passes to its children to be returned to this upon completion

    public void setRoot()
    {
        finished = rootComplete;
    }

    public int rootComplete(int success)
    {
        reset();
        run(finished);
        return success;
    }

    //use this for passing to children or other intelligent system function calls
    public virtual int childCompleted(int success)//use this for returnToMe
    {
        status = success;
        finished(success);
        return success; 
    }

    public int getStatus()
    {
        return this.status;
    }

    public Student getStudent()
    {
        return this.student;
    }

  

    public virtual void reset()
    {
        status = -2;
    } 

}
