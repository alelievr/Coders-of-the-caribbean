using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/

// class Gene {
//     public Gene(Ship ship)
//     {
//         this.path = [random.randint(0, ACTIONS_RAND_MAX) for i in range(TOTAL_TURNS)];
//         this.ship = ship
//     }

// class Individual {
//     def __init__(this, myShips):
//         this.genes = [Gene(copy.copy(ship)) for ship in myShips]
//         this.value = 0
//         this.minesCrossed = set()
//         this.barrelsCrossed = set()

//     def SingleActionHeuristic(this, action, ship):
//         if action == Actions.FASTER:
//             if ship.speed < 2:
//                 ship.speed += 1
//             else:
//                 action = Actions.WAIT
//         elif action == Actions.SLOWER:
//             if ship.speed > 0:
//                 ship.speed -= 1
//             else:
//                 action = Actions.WAIT
//         elif action == Actions.PORT:
//             ship.orientation = (ship.orientation + 1) % 6
//         elif action == Actions.STARBOARD:
//             ship.orientation = (ship.orientation - 1) % 6
//         ship.node = ship.node.NNodeFromOrientation(ship.speed, ship.orientation)
    
//     def InitShipStateAtTurn(this, turn, ship):
//         if ship.node.mine != None and ship.node.mine not in this.minesCrossed:
//             ship.rumStock -= 25
//             this.minesCrossed.add(ship.node.mine)
//         if ship.nodeFront.mine != None and ship.nodeFront.mine not in this.minesCrossed:
//             ship.rumStock -= 10
//             this.minesCrossed.add(ship.nodeFront.mine)
//         if ship.nodeBack.mine != None and ship.nodeBack.mine not in this.minesCrossed:
//             ship.rumStock -= 10
//             this.minesCrossed.add(ship.nodeBack.mine)
//         if ship.node.barrel != None and ship.node.barrel not in this.barrelsCrossed:
//             ship.rumStock += ship.node.barrel.rumAmount
//             this.barrelsCrossed.add(ship.node.barrel)
//         if ship.nodeFront.barrel != None and ship.nodeFront.barrel not in this.barrelsCrossed:
//             ship.rumStock += ship.nodeFront.barrel.rumAmount
//             this.barrelsCrossed.add(ship.nodeFront.barrel)
//         if ship.nodeBack.barrel != None and ship.nodeBack.barrel not in this.barrelsCrossed:
//             ship.rumStock += ship.nodeBack.barrel.rumAmount
//             this.barrelsCrossed.add(ship.nodeBack.barrel)
//         for canonBall in ship.node.canonBalls:
//             if canonBall.inTurn == turn:
//                 ship.rumStock -= 50
//         for canonBall in ship.nodeFront.canonBalls:
//             if canonBall.inTurn == turn:
//                 ship.rumStock -= 25
//         for canonBall in ship.nodeBack.canonBalls:
//             if canonBall.inTurn == turn:
//                 ship.rumStock -= 25
//         if ship.rumStock < 0:
//             this.died = True

static class Globals {
    public static int TOTAL_TURNS = 10;
    public static int ACTIONS_RAND_MAX = 4;
    public static int POPULATION_LEN = 25;
}

public enum Actions {
    WAIT,
    PORT,
    STARBOARD,
    FASTER,
    SLOWER,
    FIRE,
    MINE,
    MOVE
};

class Node
{
    public int x {get; private set;}
    public int y {get; private set;}
    public List<Node> links {get; private set;}
    public Mine mine {get; set;}
    public Barrel barrel {get; set;}
    public List<CanonBall> canonBalls {get; set;}
    public int[,] distances {get; set;}

    public Node(int x, int y) {
        this.x = x;
        this.y = y;
        this.links = new List<Node>(6);
        this.mine = null;
        this.barrel = null;
        this.canonBalls = new List<CanonBall>();
    }
    
    public Node NNodeFromOrientation(int n, int orientation) {
        Node node = this;
        for (int i = 0; i < n; i++)
        {
            if (node.links[orientation] == null)
                break ;
            node = node.links[orientation];
        }
        return node;
    }
    public void BFS_distance() {
        Queue<Node> queue = new Queue<Node>();
        this.distances[this.x, this.y] = 0;
        while (queue.Count > 0)
        {
            Node node = queue.Dequeue();
            for (int i = 0; i < node.links.Count; i++)
            {
                Node link = node.links[i];
                if (node.links[i] == null)
                    continue ;
                if (this.distances[link.x, link.y] == -1)
                {
                    this.distances[link.x, link.y] = this.distances[node.x, node.y] + 1;
                    queue.Enqueue(link);
                }
            }
        }
    }

    public override string ToString()
    {
        return ("[" + this.x + ":" + this.y + "]");
    }
}

class Grid {
    public int width {get; private set;}
    public int height {get; private set;}
    public Node[,] nodes{get; private set;}

    public Grid(int width, int height)
    {
        this.width = width;
        this.height = height;
        this.nodes = new Node[width,height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                this.nodes[x,y] = new Node(x, y);
        this.Init_links();
        foreach (Node node in this.nodes)
        {
            node.distances = new int[width,height];
            for (int y = 0; y < this.height; y++)
                for (int x = 0; x < this.width; x++)
                    node.distances[x, y] = -1;
            node.BFS_distance();
        }
    }

    public void TrySetLinks(Node node, int x, int y) {
        if (x >= 0 && x < this.width && y >= 0 && y < this.height)
            node.links.Add(this.nodes[x,y]);
        else
            node.links.Add(null);
    }

    public void Init_links() {
        for (int y = 0; y < this.height; y++)
            for (int x = 0; x < this.width; x++)
                if ((y & 1) == 1)
                {
                    this.TrySetLinks(this.nodes[x, y], x + 1, y);
                    this.TrySetLinks(this.nodes[x, y], x + 1, y - 1);
                    this.TrySetLinks(this.nodes[x, y], x, y - 1);
                    this.TrySetLinks(this.nodes[x, y], x - 1, y);
                    this.TrySetLinks(this.nodes[x, y], x, y + 1);
                    this.TrySetLinks(this.nodes[x, y], x + 1, y + 1);
                }
                else
                {
                    this.TrySetLinks(this.nodes[x, y], x + 1, y);
                    this.TrySetLinks(this.nodes[x, y], x, y - 1);
                    this.TrySetLinks(this.nodes[x, y], x - 1, y - 1);
                    this.TrySetLinks(this.nodes[x, y], x - 1, y);
                    this.TrySetLinks(this.nodes[x, y], x - 1, y + 1);
                    this.TrySetLinks(this.nodes[x, y], x, y + 1);
                }
    }

    public int DistanceBetween(Node node1, Node node2) {
        return node1.distances[node2.x, node2.y];
    }
}

class Barrel
{
    public Node node {get; private set;} 
    public int rumAmount {get; private set;}

    public Barrel(Node node, int rumAmount)
    {
        this.node = node;
        this.rumAmount = rumAmount;
    }
}

class CanonBall
{
    public Node node {get; private set;} 
    public int inTurn {get; private set;}

    public CanonBall(Node node, int inTurn)
    {
        this.node = node;
        this.inTurn = inTurn;
    }
}

class Mine
{
    public Node node {get; private set;} 
    public int DisapearInTurn {get; private set;}

    public Mine(Node node) {
        GameManager.SetCellColor(node.x, node.y, Color.green);
        this.node = node;
        this.DisapearInTurn = -1;
    }
}


class Ship {

    public int id {get; private set;}
    public Node nodeFront {get; private set;}
    public Node nodeBack {get; private set;}
    public int orientation {get; private set;}
    public int speed {get; private set;}
    public int rumStock {get; private set;}
    public int owner {get; private set;}
    public bool died {get; set;}

    private Node node;

    public Node Node {
        get
        {
            return this.node;
        }
        set
        {
            this.node = value;
            this.nodeFront = this.node.links[this.orientation];
            this.nodeBack = this.node.links[(this.orientation + 3) % 6];
        }
    }
    public Ship(Node node, int orientation, int speed, int rumStock) {
        this.orientation = orientation;
        this.speed = speed;
        this.rumStock = rumStock;
        this.died = false;
        this.Node = node;
    }
}

class test : PlayerAI
{
    Grid grid = new Grid(23, 21);
    List<Ship> myShips = new List<Ship>();
    List<Ship> enemyShips = new List<Ship>();

	public override string PlayTurn(int myShipCount, int entityCount, string[] consoleInputs)
    {
        // while (true)
        // {
            myShips.Clear();
            enemyShips.Clear();
            foreach (Node node in grid.nodes)
            {
                node.mine = null;
                node.barrel = null;
                node.canonBalls.Clear();
            }
            // int myShipCount = int.Parse(Console.ReadLine()); // the number of remaining ships
            // int entityCount = int.Parse(Console.ReadLine()); // the number of entities (e.g. ships, mines or cannonballs)
            for (int i = 0; i < entityCount; i++)
            {
                string[] inputs = consoleInputs[i].Split(' '); // replace with console_readline
                Node node = grid.nodes[int.Parse(inputs[2]), int.Parse(inputs[3])];
                switch (inputs[1])
                {
                    case "MINE":
                        node.mine = new Mine(node);
                        break ;
                    case "BARREL":
                        node.barrel = new Barrel(node, int.Parse(inputs[4]));
                        break ;
                    case "CANONBALL":
                        node.canonBalls.Add(new CanonBall(node, int.Parse(inputs[5])));
                        break ;
                    case "SHIP":
                        if (int.Parse(inputs[7]) == 1)
                            myShips.Add(new Ship(node, int.Parse(inputs[4]), int.Parse(inputs[5]), int.Parse(inputs[6])));
                        else
                            enemyShips.Add(new Ship(node, int.Parse(inputs[4]), int.Parse(inputs[5]), int.Parse(inputs[6])));
                        break ;
                    default:
                        break ;
                }
            }
            string outputs = ""; 
            for (int i = 0; i < myShipCount; i++)
            {
                GameManager.SetShipDebugText(i, "ship " + i);
                outputs += "WAIT\n";
                // Console.WriteLine("WAIT");
            }
            return outputs;
        // }
    }
}