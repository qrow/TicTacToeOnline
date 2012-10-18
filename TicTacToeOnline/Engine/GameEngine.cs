using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace TicTacToeOnline.Engine
{
    public interface IGameEngine
    {
        event Action<Tuple<Player, IEnumerable<Player>, IEnumerable<GameSession>>> PlayerLoggedIn;
        event Action<Player> PlayerLoggedOut;
        event Action<GameSession> GameCreated;
        event Action<GameSession> GameJoined;
        event Action<GameSession> GameEnded;
        event Action<GameTurn> TurnMade;

        void Login(string id, string email, string nickName);
        void Logout(string playerId);
        void CreateGame(string creatorId);
        void JoinGame(string gameId, string opponentId);
        void MakeTurn(string playerId, string gameId, int turn);
    }

    public class GameEngine : IGameEngine
    {
        public event Action<Tuple<Player, IEnumerable<Player>, IEnumerable<GameSession>>> PlayerLoggedIn;
        public event Action<Player> PlayerLoggedOut;
        public event Action<GameSession> GameCreated;
        public event Action<GameSession> GameJoined;
        public event Action<GameSession> GameEnded;
        public event Action<GameTurn> TurnMade;

        private readonly IRepository<GameSession> _gameSessionsRepository;
        private readonly IRepository<Player> _playersRepository;

        public GameEngine(IRepository<GameSession> gameSessionsRepository, IRepository<Player> playersRepository)
        {
            _gameSessionsRepository = gameSessionsRepository;
            _playersRepository = playersRepository;
        }

        private void PublishEvent<T>(Action<T> e, T eventArg)
        {
            if (e != null) e(eventArg);
        }

        public void Login(string id, string email, string nickName)
        {
            ValidateEmail(email);
            ValidateNickName(nickName);
            CheckForDublicates(email, nickName);

            var player = new Player(id, email, nickName);
            if (!_playersRepository.TryAdd(player.Id, player))
            {
                throw new EngineException("Unable to login user, you are already logged in");
            }
            PublishEvent(PlayerLoggedIn, Tuple.Create(player, GetPlayers(), GetGames()));
        }

        private void CheckForDublicates(string email, string nickName)
        {
            if (_playersRepository.Query().SingleOrDefault(p => p.Email == email) != null)
            {
                throw new PlayerWithSameEmailAlreadyLogedIn(
                    string.Format("Player with Email {0} already logged in to game", email));
            }
            if (_playersRepository.Query().SingleOrDefault(p => p.NickName == nickName) != null)
            {
                throw new PlayerWithSameEmailAlreadyLogedIn(
                    string.Format("Player with NickName {0} already playing", nickName));
            }
        }

        private void ValidateNickName(string nickName)
        {
            if (nickName == null || nickName.Length < 3)
            {
                throw new NickNameValidationException("Nick name should be at least 3 characters long");
            }
        }

        private void ValidateEmail(string email)
        {
            var rx = new Regex(
            @"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])");

            if (!rx.IsMatch(email))
            {
                throw new EmailValidationException("Please enter valid email");
            }
        }

        public void Logout(string playerId)
        {
            Player player = null;
            try
            {
                player = GetPlayerById(playerId);
                ForceEndOfPlayerGame(player);
                _playersRepository.TryRemove(player.Id, out player);
            }
            finally
            {
                PublishEvent(PlayerLoggedOut, player);
            }
        }

        public void CreateGame(string creatorId)
        {
            Player creator = GetPlayerById(creatorId);

            if (_gameSessionsRepository.Query().SingleOrDefault(g => g.Opponent != null && g.Opponent.Id == creatorId) != null)
            {
                throw new EngineException(
                    string.Format("You can not start new game because you already playing in one"));
            }

            var gameSession = new GameSession(Guid.NewGuid().ToString(), creator);
            if (!_gameSessionsRepository.TryAdd(gameSession.Id, gameSession))
            {
                throw new GameSessionWithSameIdAlreadyCreated(
                    string.Format("Game with id {0} already created", gameSession.Id));
            }
            PublishEvent(GameCreated, gameSession);
        }

        public void JoinGame(string gameId, string opponentId)
        {
            GameSession game = GetGameSessionById(gameId);

            Player opponent = GetPlayerById(opponentId);

            game.Join(opponent);
            PublishEvent(GameJoined, game);
        }

        public void MakeTurn(string playerId, string gameId, int turn)
        {
            Player player = GetPlayerById(playerId);
            GameSession game = GetGameSessionById(gameId);
            var boardMark = game.MakeTurn(player, turn);
            PublishEvent(TurnMade, new GameTurn(playerId, gameId, turn, boardMark));
            if (game.GameIsOver)
            {
                _gameSessionsRepository.TryRemove(gameId, out game);
                PublishEvent(GameEnded, game);
            }
        }

        private IEnumerable<GameSession> GetGames()
        {
            return _gameSessionsRepository.Query().ToList();
        }

        private IEnumerable<Player> GetPlayers()
        {
            return _playersRepository.Query().ToList();
        }

        private void ForceEndOfPlayerGame(Player player)
        {
            GameSession gameWherePlayerWasPlaying =
                _gameSessionsRepository.Query().SingleOrDefault(g => g.Opponent == player || g.Owner == player);
            if (gameWherePlayerWasPlaying != null)
            {
                gameWherePlayerWasPlaying.EndWithLooser(player);
                _gameSessionsRepository.TryRemove(gameWherePlayerWasPlaying.Id, out gameWherePlayerWasPlaying);
                PublishEvent(GameEnded, gameWherePlayerWasPlaying);
            }
        }

        private GameSession GetGameSessionById(string gameId)
        {
            GameSession game;
            if (!_gameSessionsRepository.TryGetValue(gameId, out game))
            {
                throw new NoGameWithSuchId(
                    string.Format("Game with id {0} dosen't exist", gameId));
            }
            return game;
        }

        private Player GetPlayerById(string playerId)
        {
            Player player;
            if (!_playersRepository.TryGetValue(playerId, out player))
            {
                throw new NoUserWithSuchId(
                    string.Format("User with id {0} is not logged in", playerId));
            }
            return player;
        }
    }
}