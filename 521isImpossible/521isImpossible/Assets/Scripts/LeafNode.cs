using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//every node saves a callReturn finished which is set by its parent in initialize(Student etudiant, callReturn finished), this is sent back to its parent upon completion
//every node also sets a callReturn returnToMe which is the analog of finished: a function that it passes to its children to be returned to this upon completion

public class LeafNode : BehaviourNode {

    doSomething action;

    public LeafNode(Student etudiant, callReturn parent, doSomething act) : base(etudiant, parent)
    {
        returnToMe = childCompleted;
        run = runAction;
        action = act;
    }

    public int runAction(callReturn finished)
    {
        int result = action(returnToMe);
        return result;
    }

}
