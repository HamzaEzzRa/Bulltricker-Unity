using System;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    #region Members
    public State state;
    public Node parent;
    public List<Node> childArray;
    #endregion

    #region Constructors
    public Node()
    {
        state = new State();
        childArray = new List<Node>();
    }

    public Node(State state)
    {
        this.state = state;
        childArray = new List<Node>();
    }

    public Node(State state, Node parent, List<Node> childArray)
    {
        this.state = state;
        this.parent = parent;
        this.childArray = childArray;
    }

    public Node(Node node)
    {
        this.childArray = new List<Node>();
        state = new State(node.state);
        if (node.parent != null)
            parent = node.parent;
        List<Node> childArray = node.childArray;
        foreach (Node child in childArray)
        {
            childArray.Add(new Node(child));
        }
    }
    #endregion

    #region GetRandomChildNode
    public Node GetRandomChildNode()
    {
        int movesCount = childArray.Count;
        System.Random random = new System.Random();
        int randomIndex = (int)(random.NextDouble() * movesCount);
        return childArray[randomIndex];
    }
    #endregion

    #region GetChildWithMaxScore
    public Node GetChildWithMaxScore()
    {
        Tuple<int, int> scoreIndex = new Tuple<int, int>(0, 0);
        for (int i = 0; i < childArray.Count; i++)
        {
            if (childArray[i].state != null && childArray[i].state.visitCount > scoreIndex.Item1)
            {
                scoreIndex = new Tuple<int, int>(childArray[i].state.visitCount, i);
            }
        }
        return childArray[scoreIndex.Item2];
    }
    #endregion
}

public class Tree
{
    #region Members
    public Node root;
    #endregion

    #region Constructors
    public Tree()
    {
        root = new Node();
    }

    public Tree(Node root)
    {
        this.root = root;
    }
    #endregion

    #region AddChild
    public void AddChild(Node parent, Node child)
    {
        if (parent != null && parent.childArray != null)
        {
            parent.childArray.Add(child);
        }
    }
    #endregion
}

public class State
{
    #region Members
    public MCTSBoard board;
    public int currentPlayer;
    public int visitCount;
    public float winScore;
    #endregion

    #region Constructors
    public State()
    {
        board = new MCTSBoard();
    }

    public State(State state)
    {
        board = new MCTSBoard(state.board);
        currentPlayer = state.currentPlayer;
        visitCount = state.visitCount;
        winScore = state.winScore;
    }

    public State(MCTSBoard board)
    {
        this.board = new MCTSBoard(board);
    }
    #endregion

    #region GetAllPossibleStates
    public List<State> GetAllPossibleStates()
    {
        List<State> possibleStates = new List<State>();
        List<Tuple<int, int>> availableActions = board.GetAllowedActions();
        foreach (Tuple<int, int> action in availableActions)
        {
            int opponent = -currentPlayer;
            State newState = new State(board)
            {
                currentPlayer = opponent
            };
            newState.board.PerformAction(action);
            possibleStates.Add(newState);
        }
        return possibleStates;
    }
    #endregion

    #region IncrementVisit
    public void IncrementVisit()
    {
        visitCount++;
    }
    #endregion

    #region AddScore
    public void AddScore(float score)
    {
        if (winScore != int.MinValue)
        {
            winScore += score;
        }
    }
    #endregion

    #region RandomPlay
    public void RandomPlay()
    {
        System.Random rnd = new System.Random();
        List<Tuple<int, int>> availableActions = board.GetAllowedActions();
        int randomIndex = (int)(rnd.NextDouble() * availableActions.Count);
        board.PerformAction(availableActions[randomIndex]);
    }
    #endregion

    #region TogglePlayer
    public void TogglePlayer()
    {
        currentPlayer = -currentPlayer;
    }
    #endregion
}

public class MCTS
{
    #region Members
    public const int WIN_SCORE = 10;
    public int level;
    public int opponent;
    #endregion

    #region Constructors
    public MCTS()
    {
        level = 3;
    }

    public MCTS(int level)
    {
        this.level = level;
    }
    #endregion

    #region GetMultForCurrentLevel
    private int GetMultForCurrentLevel()
    {
        return 2 * level - 1;
    }
    #endregion

    #region FindNextMove
    public MCTSBoard FindNextMove(MCTSBoard board, int currentPlayer)
    {
        double start = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        double end = start + 1000 * GetMultForCurrentLevel();
        Debug.Log("Start: " + start);
        Debug.Log("End: " + end);

        opponent = -currentPlayer;
        Tree tree = new Tree();
        Node rootNode = tree.root;
        rootNode.state.board = board;
        rootNode.state.currentPlayer = opponent;
        rootNode.state.board.currentPlayer = opponent;

        while (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond < end)
        {
            // Phase 1 - Selection
            Node promisingNode = SelectPromisingNode(rootNode);
            // Phase 2 - Expansion
            if (promisingNode.state.board.CheckState() == GameState.ONGOING)
                ExpandNode(promisingNode);
            // Phase 3 - Simulation
            Node nodeToExplore = promisingNode;
            if (promisingNode.childArray.Count > 0)
            {
                nodeToExplore = promisingNode.GetRandomChildNode();
            }
            Tuple<int, GameState> playoutResult = SimulateRandomPlayout(nodeToExplore);
            // Phase 4 - Update
            BackPropogation(nodeToExplore, playoutResult.Item1);
        }
        Node winnerNode = rootNode.GetChildWithMaxScore();
        tree.root = winnerNode;
        return winnerNode.state.board;
    }
    #endregion

    #region SelectPromisingNode
    private Node SelectPromisingNode(Node rootNode)
    {
        Node node = rootNode;
        while (node.childArray.Count != 0)
        {
            node = UCT.FindBestNodeWithUCT(node);
        }
        return node;
    }
    #endregion

    #region ExpandNode
    private void ExpandNode(Node node)
    {
        List<State> possibleStates = node.state.GetAllPossibleStates();
        foreach (State state in possibleStates)
        {
            Node newNode = new Node(state)
            {
                parent = node
            };
            newNode.state.currentPlayer = -node.state.currentPlayer;
            node.childArray.Add(newNode);
        }
    }
    #endregion

    #region BackPropagation
    private void BackPropogation(Node nodeToExplore, int currentPlayer)
    {
        Node tempNode = nodeToExplore;
        while (tempNode != null)
        {
            tempNode.state.IncrementVisit();
            if (tempNode.state.currentPlayer == currentPlayer)
                tempNode.state.AddScore(WIN_SCORE);
            tempNode = tempNode.parent;
        }
    }
    #endregion

    #region SimulateRandomPlayout
    private Tuple<int, GameState> SimulateRandomPlayout(Node node)
    {
        int currentPlayer = node.state.board.currentPlayer;
        Node tempNode = new Node(node);
        State tempState = tempNode.state;
        GameState boardState = tempState.board.CheckState();

        if ((currentPlayer == 1 && boardState == GameState.BLACK_WIN) || (currentPlayer == -1 && boardState == GameState.WHITE_WIN))
        {
            tempNode.parent.state.winScore = int.MinValue;
            return new Tuple<int, GameState>(currentPlayer, boardState);
        }
        else if ((currentPlayer == 1 && boardState == GameState.WHITE_WIN) || (currentPlayer == -1 && boardState == GameState.BLACK_WIN))
        {
            tempNode.parent.state.winScore = int.MaxValue;
            return new Tuple<int, GameState>(currentPlayer, boardState);
        }
        while (boardState == GameState.ONGOING)
        {
            tempState.TogglePlayer();
            currentPlayer = -currentPlayer;
            tempState.RandomPlay();
            boardState = tempState.board.CheckState();
        }
        return new Tuple<int, GameState>(currentPlayer, boardState);
    }
    #endregion
}

public class UCT
{
    #region GetUCTValue
    public static double GetUCTValue(int totalVisit, double nodeWinScore, int nodeVisit)
    {
        if (nodeVisit == 0)
        {
            return int.MaxValue;
        }
        return (nodeWinScore / nodeVisit) + 1.41 * Math.Sqrt(Math.Log(totalVisit) / nodeVisit);
    }
    #endregion

    #region FindBestNodeWithUCT
    public static Node FindBestNodeWithUCT(Node node)
    {
        int parentVisit = node.state.visitCount;
        double currentMax = int.MinValue;
        Node bestNode = null;
        foreach (Node child in node.childArray)
        {
            double newValue = GetUCTValue(parentVisit, child.state.winScore, child.state.visitCount);
            if (currentMax < newValue)
            {
                currentMax = newValue;
                bestNode = child;
            }
        }
        return bestNode;
    }
    #endregion
}
