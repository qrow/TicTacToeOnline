using System;

namespace TicTacToeOnline.Engine
{
    public class EngineException : Exception
    {
        public EngineException()
        {
        }

        public EngineException(string message)
            : base(message)
        {
        }
    }

    public class NoUserWithSuchId : EngineException
    {
        public NoUserWithSuchId(string message)
            : base(message)
        {
        }
    }

    public class NoGameWithSuchId : EngineException
    {
        public NoGameWithSuchId(string message)
            : base(message)
        {
        }
    }

    public class GameSessionWithSameIdAlreadyCreated : EngineException
    {
        public GameSessionWithSameIdAlreadyCreated(string message)
            : base(message)
        {
        }
    }

    public class NotThisPlayersTurn : EngineException
    {
        public NotThisPlayersTurn(string message)
            : base(message)
        {
        }
    }

    public class PlayerWithSameEmailAlreadyLogedIn : EngineException
    {
        public PlayerWithSameEmailAlreadyLogedIn(string message)
            : base(message)
        {
        }
    }

    public class PlayerWithSameNickNameAlreadyLogedIn : EngineException
    {
        public PlayerWithSameNickNameAlreadyLogedIn(string message)
            : base(message)
        {
        }
    }

    public class ThisGameInstanceIsFull : EngineException
    {
    }

    public class NickNameValidationException : EngineException
    {
        public NickNameValidationException(string message)
            : base(message)
        {
        }
    }

    public class EmailValidationException : EngineException
    {
        public EmailValidationException(string message)
            : base(message)
        {
        }
    }
}