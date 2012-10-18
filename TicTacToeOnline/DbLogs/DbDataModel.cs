using System;
using System.Collections.Generic;

namespace TicTacToeOnline.DbLogs
{
    public class Game
    {
        public string Id { get; set; }

        public Player Owner { get; set; }
        public Player Opponent { get; set; }
        public ICollection<GameEvent> GameEvents { get; set; }
        public Player Winner { get; set; }
        public Player Looser { get; set; }
        public bool Draw { get; set; }
        public DateTime CreationDate { get; set; }
    }

    public class Player
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string NickName { get; set; }
        public DateTime LoginDate { get; set; }
        public ICollection<PlayerAction> PlayersActions { get; set; }
    }

    public class GameTurn
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public Player TurnPerformer { get; set; }
        public Game Game { get; set; }
        public int Turn { get; set; }
    }

    public class GameEvent
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public GameEventType Type { get; set; }
        public Player Raiser { get; set; }
        public Game Target { get; set; }
    }

    public class PlayerAction
    {
        public int Id { get; set; }
        public Player Performer { get; set; }
        public DateTime Date { get; set; }
        public PlayerActionType Type { get; set; }
        public Game Target { get; set; }
    }

    public class GameEventType
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class PlayerActionType
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public static class GameEventTypes
    {
        public const string GameCreated = "GameCreated";
        public const string GameStarted = "GameStarted";
        public const string OpponentJoinedGame = "OpponentJoinedGame";
        public const string GameEnded = "GameEnded";
        public const string GameTurnMade = "GameTurnMade";
    }

    public static class PlayerActionTypes
    {
        public const string PlayerLoggedIn = "PlayerLoggedIn";
        public const string PlayerLoggedOut = "PlayerLoggedOut";
        public const string PlayerCreatedGame = "PlayerCreatedGame";
        public const string PlayerJoinedGame = "PlayerJoinedGame";
        public const string PlayerMadeTurn = "PlayerMadeTurn";
        public const string PlayerWon = "PlayerWon";
        public const string PlayerLost = "PlayerLost";
        public const string PlayerPlayedInADraw = "PlayerPlayedInADraw";
    }
}