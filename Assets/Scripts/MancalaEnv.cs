using System.Collections.Generic;
using System.Linq;

public class MancalaEnv
{
    private int[] board;

    public enum Player
    {
        PLAYER,
        AGENT,
    };
    private Player currentPlayer;

    public Player CurrentPlayer => currentPlayer;

    // The model always sees the board from the perspective of the agent, so we need to flip the board state
    public float[] BoardState => Enumerable
        .Range(7, 7).Select(i => board[i])
        .Concat(Enumerable.Range(0, 7).Select(i => board[i]))
        .Select(x => x / 48.0f)
        .ToArray();

    public MancalaEnv()
    {
        Reset();
    }

    public float[] Reset()
    {
        // 6 pits each + 2 mancalas
        board = new int[] {4, 4, 4, 4, 4, 4, 0, 4, 4, 4, 4, 4, 4, 0};
        currentPlayer = Player.PLAYER;
        return BoardState;
    }

    public int[] ValidMoves => (currentPlayer == Player.PLAYER)
        ? Enumerable.Range(0, 6).Where(i => board[i] > 0).ToArray()
        : Enumerable.Range(7, 6).Where(i => board[i] > 0).Select(i => i - 7).ToArray();

    public bool OwnPit(int index)
    {
        return (currentPlayer == Player.PLAYER && 0 <= index && index < 6) ||
               (currentPlayer == Player.AGENT && 7 <= index && index < 13);
    }

    public int OwnMancala => (currentPlayer == Player.PLAYER) ? 6 : 13;

    public int OpponentMancala => (currentPlayer == Player.PLAYER) ? 13 : 6;

    public bool GameOver => board.Take(6).Sum() == 0 || board.Skip(7).Take(6).Sum() == 0;

    public (float[], bool) Step(int pit)
    {
        int stones = board[pit];
        board[pit] = 0;
        int i = pit;

        // distribute stones
        while (stones > 0)
        {
            i = (i + 1) % 14;
            // skip opponent's mancala
            if (i == OpponentMancala)
            {
                continue;
            }
            board[i] += 1;
            stones -= 1;
        }
        int lastPit = i;

        // capture rule
        if (OwnPit(lastPit) && lastPit != OwnMancala && board[lastPit] == 1)
        {
            int oppositeIndex = 12 - lastPit;
            if (board[oppositeIndex] > 0)
            {
                board[OwnMancala] += board[oppositeIndex] + 1;
                board[oppositeIndex] = 0;
                board[lastPit] = 0;
            }
        }

        bool done = false;

        // terminal condition
        if (GameOver)
        {
            // collect remaining stones
            board[6] += board.Take(6).Sum();
            board[13] += board.Skip(7).Take(6).Sum();
            for (int j = 0; j < 6; j++)
            {
                board[j] = 0;
                board[j + 7] = 0;
            }

            done = true;
        }

        // extra turn rule or switch player
        if (lastPit != OwnMancala)
        {
            currentPlayer = 1 - currentPlayer;
        }

        return (BoardState, done);
    }

    public float[] ValidActionMask => (currentPlayer == Player.AGENT)
        ? Enumerable.Range(0, 6).Select(i => board[i + 7] > 0 ? 1.0f : 0.0f).ToArray()
        : Enumerable.Range(7, 6).Select(i => board[i] > 0 ? 1.0f : 0.0f).ToArray();
}