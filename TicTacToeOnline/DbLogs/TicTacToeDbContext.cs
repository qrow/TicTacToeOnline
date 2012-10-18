using System.Data.Entity;

namespace TicTacToeOnline.DbLogs
{
    public class TicTacToeDbContext : DbContext
    {
        public TicTacToeDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public DbSet<Game> Games { get; set; }
        public DbSet<GameEvent> GameEvents { get; set; }
        public DbSet<GameEventType> GameEventTypes { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<PlayerAction> PlayerActions { get; set; }
        public DbSet<PlayerActionType> PlayerActionTypes { get; set; }
        public DbSet<GameTurn> Turns { get; set; }
    }
}