namespace TicTacToeOnline.Engine
{
    public class GameTurn
    {
        public GameTurn(string playerId, string gameId, int turn, BoardMark boardMark)
        {
            PlayerId = playerId;
            GameId = gameId;
            Turn = turn;
            BoardMark = boardMark;
        }
        public string PlayerId { get; private set; }
        public string GameId { get; private set; }
        public int Turn { get; private set; }
        public BoardMark BoardMark { get; private set; }
    }
}