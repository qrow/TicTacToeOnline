using System;
using System.Linq;

namespace TicTacToeOnline.Engine
{
    /// <summary>
    /// Implement single tic tac toe game session
    /// </summary>
    public class GameSession
    {
        public Player Owner { get; private set; }
        public Player Opponent { get; private set; }
        public bool GameIsOver { get; private set; }
        public bool IsDraw { get; private set; }
        public Player Winner { get; private set; }
        public Player Looser { get; private set; }
        public string Id { get; private set; }
        public Player WhosTurn { get; private set; }

        public GameSession(string id, Player owner)
        {
            Id = id;
            Owner = owner;
            GameIsOver = false;
            IsDraw = false;
            _gameBoard = new BoardMark?[9];
        }

        private int _turnCount;
        private readonly BoardMark?[] _gameBoard;

        /// <summary>
        /// Game board example:
        /// 0  1  2
        /// 3  4  5
        /// 6  7  8
        /// </summary>
        private static readonly int[][] WinningLines = new[]
        {
            new []{0,1,2}, 
            new []{3,4,5}, 
            new []{6,7,8}, 
            new []{0,3,6}, 
            new []{1,4,7}, 
            new []{2,5,8}, 
            new []{0,4,8}, 
            new []{2,4,6} 
        };

        /// <summary>
        /// Opponent trying to joing game
        /// </summary>
        public void Join(Player opponent)
        {
            if (Opponent != null)
            {
                throw new ThisGameInstanceIsFull();
            }
            if (opponent == Owner)
            {
                throw new EngineException("You can not play with your self");
            }
            Opponent = opponent;

            SetStartingPlaer();
        }

        /// <summary>
        /// Tries to make game turn
        /// </summary>
        public BoardMark MakeTurn(Player player, int turn)
        {
            ValidateTurn(player, turn);

            _gameBoard[turn] = player == Owner ? BoardMark.Owner : BoardMark.Opponent;

            WhosTurn = player == Owner ? Opponent : Owner;

            _turnCount++;

            CheckForGameEnd();

            return player == Owner ? BoardMark.Owner : BoardMark.Opponent;
        }

        /// <summary>
        /// End game with declaring looser
        /// </summary>
        public void EndWithLooser(Player looser)
        {
            EndWithWinner(looser == Owner ? Opponent : Owner);
        }

        /// <summary>
        /// End game with draw
        /// </summary>
        private void EndWithDraw()
        {
            GameIsOver = true;
            IsDraw = true;
        }

        /// <summary>
        /// End game with declaring winner
        /// </summary>
        private void EndWithWinner(Player winner)
        {
            Winner = winner;
            Looser = winner == Owner ? Opponent : Owner;
            GameIsOver = true;
        }

        /// <summary>
        /// Randomly choses who starts game
        /// </summary>
        private void SetStartingPlaer()
        {
            WhosTurn = new[] { Opponent, Owner }[new Random().Next(2)];
        }

        /// <summary>
        /// Validates turn in game
        /// </summary>
        private void ValidateTurn(Player player, int turn)
        {
            if (WhosTurn != player)
            {
                throw new NotThisPlayersTurn(string.Format("It is not {0} turn", player.NickName));
            }

            if (turn < 0 || turn > 8)
            {
                throw new EngineException("This is not valid turn");
            }

            if (_gameBoard[turn].HasValue)
            {
                throw new EngineException("This is not valid turn, there is already mark in that cell");
            }
        }

        /// <summary>
        /// Checks if game is finished, if so then calls appropriate game end functions
        /// </summary>
        private void CheckForGameEnd()
        {
            Player winner;
            if (WeHaveWinner(out winner))
            {
                EndWithWinner(winner);
                return;
            }

            if (_turnCount >= 9)
            {
                EndWithDraw();
                return;
            }
        }

        /// <summary>
        /// Desides if we have winner in game or not
        /// </summary>
        private bool WeHaveWinner(out Player winner)
        {
            Player won = null;
            WinningLines.ToList().ForEach(line =>
            {
                if (_gameBoard[line[0]].HasValue &&
                    _gameBoard[line[0]] == _gameBoard[line[1]] &&
                    _gameBoard[line[1]] == _gameBoard[line[2]])
                {
                    won = _gameBoard[line[0]] == BoardMark.Owner ? Owner : Opponent;
                }
            });
            winner = won;
            if (winner != null)
            {
                return true;
            }
            return false;
        }
    }
}