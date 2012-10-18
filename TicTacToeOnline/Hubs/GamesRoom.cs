using System;
using System.Threading.Tasks;
using SignalR.Hubs;
using TicTacToeOnline.Engine;
using log4net;

namespace TicTacToeOnline.Hubs
{
    /// <summary>
    /// Responsible for getting commands from plaers and routing them to game engine
    /// </summary>
    public class GamesRoom : Hub, IDisconnect
    {
        private readonly IGameEngine _gameEngine;
        private readonly ILog _log = LogManager.GetLogger(typeof(GamesRoom));

        public GamesRoom(IGameEngine gameEngine)
        {
            _gameEngine = gameEngine;
        }

        public void Login(string email, string nickName)
        {
            WithErrorHandling(() => 
                _gameEngine.Login(Context.ConnectionId, email, nickName));
        }

        public void CreateGame()
        {
            WithErrorHandling(() => 
                _gameEngine.CreateGame(Context.ConnectionId));
        }

        public void JoinGame(string gameId)
        {
            WithErrorHandling(() => 
                _gameEngine.JoinGame(gameId, Context.ConnectionId));
        }

        public void MakeTurn(string gameId, int turn)
        {
            WithErrorHandling(() => 
                _gameEngine.MakeTurn(Context.ConnectionId, gameId, turn));
        }

        private void WithErrorHandling(Action action)
        {
            try
            {
                action();
            }
            catch (EngineException e)
            {
                _log.Warn("Game engine exception", e);
                throw;
            }
            catch (Exception e)
            {
                _log.Error("System exception", e);
#if DEBUG
                throw;
#endif
            }
        }

        public Task Disconnect()
        {
            _gameEngine.Logout(Context.ConnectionId);
            return new Task(()=>{});
        }
    }
}