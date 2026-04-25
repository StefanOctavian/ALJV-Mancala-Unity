using UnityEngine;
using System.Collections.Generic;

public class Board : MonoBehaviour
{
    [SerializeField] 
    private GameObject marblePrefab;
    [SerializeField]
    private Transform ownMancala;
    [SerializeField]
    private Transform opponentMancala;

    private readonly List<Transform> pits = new ();

    private readonly MancalaEnv env = new ();

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

    public void PerformMove(int pitIndex)
    {
        env.Step(pitIndex);
        int i = pitIndex;
        List<Transform> stonesToMove = new ();
        foreach (Transform stone in pits[i])
        {
            stonesToMove.Add(stone);
        }
        foreach (Transform stone in stonesToMove)
        {
            i = (i + 1) % 14;
            if (i == env.OpponentMancala())
            {
                i = (i + 1) % 14;
            }
            stone.SetParent(pits[i]);
            // Stone position random offset from center of pit
            stone.localPosition = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0);
        }
        if (pits[i].childCount == 1 && env.OwnPit(i))
        {
            int oppositeIndex = 12 - i;
            Transform mancala = pits[env.OwnMancala()];
            List<Transform> oppositeStones = new ();
            foreach (Transform stone in pits[oppositeIndex])
            {
                oppositeStones.Add(stone);
            }
            foreach (Transform stone in oppositeStones)
            {
                stone.SetParent(mancala);
                stone.localPosition = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0);
            }
        }
    }
}
