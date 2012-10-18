using System;
using System.Collections.Generic;
using SignalR;
using TicTacToeOnline.Engine;

namespace TicTacToeOnline.Hubs
{
    /// <summary>
    /// Listens for game engine events and routes them to players
    /// </summary>
    public class GamesRoomGameEngineEventListner : GameEngineEventsListnerBase
    {
        private static readonly object Locker = new Object();

        private readonly Func<IConnectionManager> _connectionManagerFactory;

        private const string LoggedInUsersGroupName = "loggedInUsers";

        public GamesRoomGameEngineEventListner(IGameEngine gameEngine, Func<IConnectionManager> connectionManagerFactory)
            : base(gameEngine)
        {
            _connectionManagerFactory = connectionManagerFactory;
        }

        public override void OnTurnMade(GameTurn turn)
        {
            dynamic clients = _connectionManagerFactory().GetClients<GamesRoom>();
            clients[turn.GameId].turnMade(turn);
        }

        public override void OnGameEnded(GameSession game)
        {
            dynamic clients = _connectionManagerFactory().GetClients<GamesRoom>();
            clients[LoggedInUsersGroupName].gameEnded(game);
            clients[game.Id].yourGameIsOver(game);
            var groupManager = (IGroupManager)clients;
            lock (Locker)
            {//to prevent raise conditions
                groupManager.RemoveFromGroup(game.Owner.Id, game.Id);
                groupManager.RemoveFromGroup(game.Opponent.Id, game.Id);
            }
            
        }

        public override void OnGameJoined(GameSession game)
        {
            dynamic clients = _connectionManagerFactory().GetClients<GamesRoom>();
            clients[LoggedInUsersGroupName].gameJoined(game);
            var groupManager = (IGroupManager)clients;
            groupManager.AddToGroup(game.Opponent.Id, game.Id);
            clients[game.Opponent.Id].youJoinedGame();
            clients[game.Id].yourGameStarted(game);
        }

        public override void OnGameCreated(GameSession game)
        {
            dynamic clients = _connectionManagerFactory().GetClients<GamesRoom>();
            clients[LoggedInUsersGroupName].gameCreated(game);
            var groupManager = (IGroupManager)clients;
            groupManager.AddToGroup(game.Owner.Id, game.Id);
            clients[game.Owner.Id].youCreatedGame();
        }

        public override void OnPlayerLoggedOut(Player player)
        {
            dynamic clients = _connectionManagerFactory().GetClients<GamesRoom>();
            var groupManager = (IGroupManager)clients;
            groupManager.RemoveFromGroup(player.Id, LoggedInUsersGroupName);
            clients[LoggedInUsersGroupName].playerLoggedOut(player);
        }

        public override void OnPlayerLoggedIn(Tuple<Player, IEnumerable<Player>, IEnumerable<GameSession>> initialData)
        {
            var player = initialData.Item1;
            var players = initialData.Item2;
            var games = initialData.Item3;
            dynamic clients = _connectionManagerFactory().GetClients<GamesRoom>();
            var groupManager = (IGroupManager)clients;
            groupManager.AddToGroup(player.Id, LoggedInUsersGroupName);
            clients[LoggedInUsersGroupName].playerLoggedIn(player);
            clients[player.Id].youLoggedIn(player, players, games);
        }
    }
}