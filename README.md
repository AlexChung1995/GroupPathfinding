# GroupPathfinding

The theoretical basis for this project was created by Prof. David Silver at the University of Alberta and can be reviewed at: 
http://www0.cs.ucl.ac.uk/staff/D.Silver/web/Applications_files/coop-path-AIWisdom.pdf .

The project is developed in Unity with C# and implements a variation of the A* algorithm to find paths for multiple agents to multiple destinations (sometimes the same)
without collisions. Upon opening the executable you will notice an arena. Burghundy tiles denote moveable spaces and black tiles denote impassable terrain.
The grey spheres denote possible destination locations and are located within "rooms". The spheres of varying shades of green are the artificially intelligent agents. 
Finally 2 dimensional squares represent "plaques". The agents pathfind to each of these plaques which tell the agent the identity of the grey sphere within the room.
If the identity of the grey sphere is equivalent to the agents' desired destination, the agent then pathfinds to the grey sphere as opposed to moving on to the next plaque.
The architecture of the arena creates numerous "choke points" where interesting behaviour can be observed. Agents will wait and move out of the way so that other agents
can make their way through choke points. Ocassionally, agents enter an "idle" state in which they pathfind to a random location.

The cooperative pathfinding is handled by Silver's algorithm, however, the complex behaviour of the agents (deciding which plaque to go to, whether they are idle etc.)
is handled by another popular AI solution: decision trees. The tree system implemented in my project is robust and dynamic, supporting on-the-fly altering or creation
of a tree. This dynamic attribute is achieved through making each node of the tree an object, rather than a series of switch or if statements, and through callback
functions when a node completes.

I hope you like this project.

Alex