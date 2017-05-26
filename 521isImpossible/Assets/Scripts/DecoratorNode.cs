using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//every node saves a callReturn finished which is set by its parent in initialize(Student etudiant, callReturn finished), this is sent back to its parent upon completion
//every node also sets a callReturn returnToMe which is the analog of finished: a function that it passes to its children to be returned to this upon completion

public class DecoratorNode : BehaviourNode
{

    BehaviourNode child;

    public DecoratorNode(Student etudiant, callReturn parent) : base(etudiant, parent)
    {

    }

    public void setChild(BehaviourNode kid)
    {
        child = kid;
    }

    public void setIdle()
    {
        returnToMe = childCompleted;
        run = Idle;
    }

    public override void reset()
    {
        child.reset();
        status = -2;
    }

    public int Idle(callReturn finished)
    {
        float prob = Random.Range(0f, 1f);
        if (prob > 0.5)
        {
            status = 0;
            child.run(returnToMe);
            return 0;
        }
        else
        {
            status = 1;
            finished(1);
            return 1;
        }
    }

}
