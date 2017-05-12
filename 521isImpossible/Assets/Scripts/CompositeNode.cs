using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompositeNode : BehaviourNode {

    List<BehaviourNode> children;

    public override void reset()
    {
        status = -2; 
        foreach (BehaviourNode child in children)
        {
            child.reset();
        }
    }

    public void Sequence(callReturn finished)
    {

    }

    public void Selector(callReturn finished)
    {

    }

    public void Parallel(callReturn finished)
    {

    }

}
