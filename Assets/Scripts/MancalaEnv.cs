using System.Collections.Generic;
using System.Linq;

public class MancalaEnv
{
    private int[] board;

    public enum Player
    {
        AGENT,
        OPPONENT
    };
    private Player currentPlayer;

    public MancalaEnv()
    {
        Reset();
    }

    List<float> Reset()
    {
        // 6 pits each + 2 mancalas
        board = new int[] {4, 4, 4, 4, 4, 4, 0, 4, 4, 4, 4, 4, 4, 0};
        currentPlayer = Player.AGENT;
        return GetState();
    }

    List<float> GetState()
    {
        return board.Select(x => x / 48.0f).ToList();
    }

    List<int> ValidMoves()
    {
        if (currentPlayer == Player.AGENT)
        {
            return Enumerable.Range(0, 6).Where(i => board[i] > 0).ToList();
        }
        else
        {
            return Enumerable.Range(7, 6).Where(i => board[i] > 0).Select(i => i - 7).ToList();
        }
    }

    public bool OwnPit(int index)
    {
        return (currentPlayer == Player.AGENT && 0 <= index && index < 6) ||
               (currentPlayer == Player.OPPONENT && 7 <= index && index < 13);
    }

    public int OwnMancala()
    {
        return currentPlayer == Player.AGENT ? 6 : 13;
    }

    public int OpponentMancala()
    {
        return currentPlayer == Player.AGENT ? 13 : 6;
    }

    public (List<float>, float, bool) Step(int pit)
    {
        int stones = board[pit];
        board[pit] = 0;
        int i = pit;

        // distribute stones
        while (stones > 0)
        {
            i = (i + 1) % 14;
            // skip opponent's mancala
            if (i == OpponentMancala())
            {
                continue;
            }
            board[i] += 1;
            stones -= 1;
        }
        int lastPit = i;

        // capture rule
        if (OwnPit(lastPit) && lastPit != OwnMancala() && board[lastPit] == 1)
        {
            int oppositeIndex = 12 - lastPit;
            board[OwnMancala()] += board[oppositeIndex];
            board[oppositeIndex] = 0;
        }

        float reward = 0;
        bool done = false;

        // terminal condition
        if (board.Take(6).Sum() == 0 || board.Skip(7).Take(6).Sum() == 0)
        {
            done = true;
            if (board[6] > board[13])
            {
                reward = 1;
            }
            else if (board[6] < board[13])
            {
                reward = -1;
            }
        }

        // extra turn rule or switch player
        if (lastPit != OwnMancala())
        {
            currentPlayer = currentPlayer == Player.AGENT ? Player.OPPONENT : Player.AGENT;
        }

        return (GetState(), reward, done);
    }

    List<float> ValidActionMask()
    {
        List<float> mask = new (new float[6]);

        if (currentPlayer == Player.AGENT)
        {
            for (int i = 0; i < 6; ++i)
            {
                mask[i] = board[i] > 0 ? 1.0f : 0.0f;
            }
        }
        else
        {
            for (int i = 7; i < 13; ++i)
            {
                mask[i - 7] = board[i] > 0 ? 1.0f : 0.0f;
            }
        }

        return mask;
    }
}