using System;
using System.Collections.Generic;
using System.Linq;
using TicTacToeOnline.Engine;

namespace TicTacToeOnline.DbLogs
{
    public class GameEngineListnerThatStoresLogsToDb : GameEngineEventsListnerBase
    {
        private readonly Func<TicTacToeDbContext> _dbContextFactory;

        public GameEngineListnerThatStoresLogsToDb(IGameEngine gameEngine, Func<TicTacToeDbContext> dbContextFactory)
            : base(gameEngine)
        {
            _dbContextFactory = dbContextFactory;
        }

        public override void OnPlayerLoggedIn(Tuple<Engine.Player, IEnumerable<Engine.Player>, IEnumerable<GameSession>> initialData)
        {
            var player = initialData.Item1;

            _dbContextFactory().Players.Add( new Player
            {
                Id = player.Id,
                Email = player.Email,
                LoginDate = DateTime.UtcNow,
                NickName = player.NickName
            });

            AddPlayerAction(player.Id, PlayerActionTypes.PlayerLoggedIn);
            _dbContextFactory().SaveChanges();
        }

        public override void OnPlayerLoggedOut(Engine.Player player)
        {
            AddPlayerAction(player.Id, PlayerActionTypes.PlayerLoggedOut);
            _dbContextFactory().SaveChanges();
        }

        public override void OnGameCreated(GameSession game)
        {
            _dbContextFactory().Games.Add(new Game
            {
                Id = game.Id,
                Owner = _dbContextFactory().Players.Find(game.Owner.Id),
                CreationDate = DateTime.UtcNow
            });
            AddPlayerAction(game.Owner.Id, PlayerActionTypes.PlayerCreatedGame, game.Id);
            AddGameEvent(game.Id, GameEventTypes.GameCreated, game.Owner.Id);

            _dbContextFactory().SaveChanges();
        }

        public override void OnGameJoined(GameSession game)
        {
            AddGameEvent(game.Id, GameEventTypes.OpponentJoinedGame, game.Opponent.Id);
            AddGameEvent(game.Id, GameEventTypes.GameStarted, game.Opponent.Id);
            AddPlayerAction(game.Opponent.Id, PlayerActionTypes.PlayerJoinedGame, game.Id);
            _dbContextFactory().Games.Find(game.Id).Opponent = _dbContextFactory().Players.Find(game.Opponent.Id);

            _dbContextFactory().SaveChanges();
        }

        public override void OnGameEnded(GameSession game)
        {
            AddGameEvent(game.Id, GameEventTypes.GameEnded);
            if (game.IsDraw)
            {
                AddPlayerAction(game.Owner.Id, PlayerActionTypes.PlayerPlayedInADraw, game.Id);
                AddPlayerAction(game.Opponent.Id, PlayerActionTypes.PlayerPlayedInADraw, game.Id);
                _dbContextFactory().Games.Find(game.Id).Draw = true;
            }
            else
            {
                AddPlayerAction(game.Winner.Id, PlayerActionTypes.PlayerWon, game.Id);
                _dbContextFactory().Games.Find(game.Id).Winner = _dbContextFactory().Players.Find(game.Winner.Id);

                AddPlayerAction(game.Looser.Id, PlayerActionTypes.PlayerLost, game.Id);
                _dbContextFactory().Games.Find(game.Id).Looser = _dbContextFactory().Players.Find(game.Looser.Id);
            }

            _dbContextFactory().SaveChanges();
        }

        public override void OnTurnMade(Engine.GameTurn turn)
        {
            AddGameEvent(turn.GameId, GameEventTypes.GameTurnMade, turn.PlayerId);
            AddPlayerAction(turn.PlayerId, PlayerActionTypes.PlayerMadeTurn, turn.GameId);

            _dbContextFactory().Turns.Add(new GameTurn
            {
                Date = DateTime.UtcNow,
                Game = _dbContextFactory().Games.Find(turn.GameId),
                TurnPerformer = _dbContextFactory().Players.Find(turn.PlayerId),
                Turn = turn.Turn
            });

            _dbContextFactory().SaveChanges();
        }

        private void AddGameEvent(string gameId, string eventTypeName, string playerId = null)
        {
            _dbContextFactory().GameEvents.Add(new GameEvent
            {
                Date = DateTime.UtcNow,
                Raiser = string.IsNullOrEmpty(playerId) ? null : _dbContextFactory().Players.Find(playerId),
                Type = GetOrCreateGameEvent(eventTypeName),
                Target = _dbContextFactory().Games.Find(gameId)
            });
        }

        private void AddPlayerAction(string playerId, string actionTypeName, string gameId = null)
        {
            _dbContextFactory().PlayerActions.Add(new PlayerAction
            {
                Date = DateTime.UtcNow,
                Performer = _dbContextFactory().Players.Find(playerId),
                Type = GetOrCreatePlayerAction(actionTypeName),
                Target = string.IsNullOrEmpty(gameId) ? null : _dbContextFactory().Games.Find(gameId)
            });
        }

        private PlayerActionType GetOrCreatePlayerAction(string actionTypeName)
        {
            return _dbContextFactory()
                .PlayerActionTypes
                .SingleOrDefault(a => a.Name == actionTypeName)
                ?? new PlayerActionType { Name = actionTypeName };
        }

        private GameEventType GetOrCreateGameEvent(string eventTypeName)
        {
            return _dbContextFactory()
                .GameEventTypes
                .SingleOrDefault(a => a.Name == eventTypeName)
                ?? new GameEventType { Name = eventTypeName };
        }
    }
}