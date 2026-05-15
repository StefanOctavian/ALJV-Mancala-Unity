using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Board : MonoBehaviour
{
    [SerializeField] 
    private GameObject marblePrefab;
    [SerializeField]
    private Transform ownMancala;
    [SerializeField]
    private Transform opponentMancala;
    [SerializeField]
    private MancalaAI ai;

    private readonly List<Transform> pits = new ();
    private readonly MancalaEnv env = new ();
    private bool isAITurn = false;

    private WaitForSeconds captureMoveDelay = new (0.05f);

    void Start()
    {
        for (int i = 0; i < 6; ++i)
        {
            pits.Add(transform.Find("Pits").GetChild(i));
        }
        pits.Add(ownMancala);
        for (int i = 11; i > 5; --i)
        {
            pits.Add(transform.Find("Pits").GetChild(i));
        }
        pits.Add(opponentMancala);
    }

    IEnumerator MoveStone(Transform stone, int toPit)
    {
        Vector3 startPos = stone.position;
        Vector3 localEndPos = new (Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0);
        Vector3 endPos = pits[toPit].position + localEndPos;

        float speed = 3.0f;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * speed;
            stone.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        stone.position = endPos;
        stone.SetParent(pits[toPit]);
    }

    public IEnumerator PerformMove(int pitIndex)
    {
        int i = pitIndex;
        List<Transform> stonesToMove = new ();
        foreach (Transform stone in pits[i])
        {
            stonesToMove.Add(stone);
        }
        foreach (Transform stone in stonesToMove)
        {
            i = (i + 1) % 14;
            if (i == env.OpponentMancala)
            {
                i = (i + 1) % 14;
            }
            yield return MoveStone(stone, i);
        }
        int oppositeIndex = 12 - i;
        if (pits[i].childCount == 1 && env.OwnPit(i) && pits[oppositeIndex].childCount > 0)
        {
            List<Transform> oppositeStones = new ();
            List<Coroutine> captureMoves = new ();
            foreach (Transform stone in pits[oppositeIndex])
                oppositeStones.Add(stone);

            foreach (Transform stone in oppositeStones)
            {
                captureMoves.Add(StartCoroutine(MoveStone(stone, env.OwnMancala)));
                yield return captureMoveDelay;
            }
            captureMoves.Add(StartCoroutine(MoveStone(pits[i].GetChild(0), env.OwnMancala)));
            foreach (var move in captureMoves)
                yield return move;
        }
        env.Step(pitIndex);
    }

    public IEnumerator PerformAIMove()
    {
        int aiMove = ai.GetAIMove(env.BoardState, env.ValidActionMask);
        int pitIndex = aiMove + 7;
        Debug.Log($"AI move: {aiMove}");
        yield return PerformMove(pitIndex);
        Debug.Log($"Turn: {env.CurrentPlayer}");
        isAITurn = false;
    }

    private IEnumerator PerformAIMoveWithDelay(float delay)
    {
        isAITurn = true;
        yield return new WaitForSeconds(delay);
        yield return PerformAIMove();
    }

    public IEnumerator TryPlayerMove(int pitIndex)
    {
        if (isAITurn || env.CurrentPlayer != MancalaEnv.Player.PLAYER)
            yield break;

        Debug.Log($"Player move: {pitIndex}");
        yield return PerformMove(pitIndex);
        Debug.Log($"Turn: {env.CurrentPlayer}");
    }

    IEnumerator EndGame()
    {
        // Move remaining stones to respective mancalas
        for (int i = 0; i < 6; ++i)
        {
            foreach (Transform stone in pits[i])
            {
                yield return MoveStone(stone, 6);
            }
        }
        for (int i = 7; i < 13; ++i)
        {
            foreach (Transform stone in pits[i])
            {
                yield return MoveStone(stone, 13);
            }
        }
        if (pits[6].childCount > pits[13].childCount)
        {
            Debug.Log("Player wins!");
        }
        else if (pits[6].childCount < pits[13].childCount)
        {
            Debug.Log("AI wins!");
        }
        else
        {
            Debug.Log("It's a tie!");
        }
    }

    void Update()
    {
        if (!isAITurn && env.CurrentPlayer == MancalaEnv.Player.AGENT)
        {
            StartCoroutine(PerformAIMoveWithDelay(1.0f));
        }

        if (env.GameOver)
        {
            StartCoroutine(EndGame());
        }
    }
}
