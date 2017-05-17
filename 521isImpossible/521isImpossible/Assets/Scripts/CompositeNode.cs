using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//every node saves a callReturn finished which is set by its parent in initialize(Student etudiant, callReturn finished), this is sent back to its parent upon completion
//every node also sets a callReturn returnToMe which is the analog of finished: a function that it passes to its children to be returned to this upon completion

public class CompositeNode : BehaviourNode
{

    private List<BehaviourNode> children;

    public CompositeNode(Student etudiant, callReturn parent) : base(etudiant, parent)
    {
        children = new List<BehaviourNode>();
    }

    public void AddChild(BehaviourNode child)
    {
        children.Add(child);
    }

    public List<BehaviourNode> getChildren()
    {
        return this.children;
    }

    public override void reset()
    {
        status = -2;
        foreach (BehaviourNode child in children)
        {
            child.reset();
        }
    }

    public void initSequence()
    {
        returnToMe = SequenceReturnToMe;
        run = Sequence;
    }

    public int SequenceReturnToMe(int success)
    {
        int result = Sequence(finished);//everyTime a child completes and returns, run sequence to see if they have all completed
        return result;//finished is saved upon initialization
    }

    public int Sequence(callReturn finished)//all children must return success to return success
    {
        for (int i = 0; i < children.Count; i++)
        {
            if (children[i].getStatus() == -1)//if unsuccessful, return unsuccessful
            {
                status = -1;
                finished(-1);
                return -1;
            }
            else if (children[i].getStatus() == 0)//still running
            {
                status = 0;
                return 0;
            }
            else if (children[i].getStatus() == -2)//-2 denotes an unstarted node
            {
                children[i].run(returnToMe);
                status = 0;
                return 0;
            }
        }//if we have gotten through all children and they have all completed succesfully return a success
        status = 1;
        finished(1);
        return 1;
    }

    public void initSelector()
    {
        returnToMe = SelectorReturnToMe;
        run = Selector;
    }

    public int SelectorReturnToMe(int success)
    {
        int result = Selector(finished);
        return result;
    }

    public int Selector(callReturn finished)//if one child returns success, return success
    {
        for (int i = 0; i < children.Count; i++)
        {
            if (children[i].getStatus() == 1)
            {
                status = 1;
                finished(1);
                return 1;
            }
            else if (children[i].getStatus() == -2)
            {
                status = 0;
                children[i].run(returnToMe);
                return 0;
            }
            else if (children[i].getStatus() == 0)//if we are still waiting for a child to return
            {//this shouldnt happen but is a failsafe regardless
                status = 0;
                return 0;
            }
        }
        //if all children are -1
        status = -1;
        finished(-1);
        return -1;
    }

    public void initParallel()
    {
        run = Parallel;
        returnToMe = ParallelReturnToMe;
    }

    public int ParallelReturnToMe(int position)
    {
        if (position == 0)
        {
            status = 1;
            finished(1);
            return 1;
        }
        children[position - 1].run(returnToMe);
        return 0;
    }

    public int Parallel(callReturn finished)
    {
        children[0].run(returnToMe);
        return 0;
    }

}
