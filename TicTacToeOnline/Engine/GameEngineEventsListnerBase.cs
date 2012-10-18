using System;
using System.Collections.Generic;

namespace TicTacToeOnline.Engine
{
    /// <summary>
    /// Base class for game engine event listner. If you want to subscribe to events produced by game enginge then 
    /// you need to implement this class and override any event listner methods that you want to be trancking.
    /// Also you need to create instance of your listner together with application start. Get instance of your listner
    /// from kernel in NinjectMVC3.Start()
    /// </summary>
    public abstract class GameEngineEventsListnerBase
    {
        protected GameEngineEventsListnerBase(IGameEngine gameEngine)
        {
            gameEngine.PlayerLoggedIn += OnPlayerLoggedIn;
            gameEngine.PlayerLoggedOut += OnPlayerLoggedOut;
            gameEngine.GameCreated += OnGameCreated;
            gameEngine.GameJoined += OnGameJoined;
            gameEngine.GameEnded += OnGameEnded;
            gameEngine.TurnMade += OnTurnMade;
        }

        public virtual void OnTurnMade(GameTurn turn){}

        public virtual void OnGameEnded(GameSession game){}

        public virtual void OnGameJoined(GameSession game){}

        public virtual void OnGameCreated(GameSession game){}

        public virtual void OnPlayerLoggedOut(Player player){}

        public virtual void OnPlayerLoggedIn(Tuple<Player , IEnumerable<Player>, IEnumerable<GameSession>> initialData) { }
    }
}