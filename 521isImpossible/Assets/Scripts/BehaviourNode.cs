using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class BehaviourNode : MonoBehaviour{
    public Student student;
	public int status; //1 if success, 0 if running, -1 if failure
    public delegate int callReturn(int success);
    callReturn finish;
    public delegate int doSomething(callReturn finished);
    doSomething done;

    public virtual int onComplete(int success)
    {
        status = success;
        return status;
    }

    void Start()
    {
        finish = onComplete;
        status = -2;
    }

    void Update()
    { 
    }

    public void initialize(Student etudiant)
    {
        student = etudiant;
    }

    public virtual void reset()
    {
        status = -2;
    } 

}
