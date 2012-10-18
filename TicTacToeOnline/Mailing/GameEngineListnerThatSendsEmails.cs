using System.Net.Mail;
using Mvc.Mailer;
using TicTacToeOnline.Engine;

namespace TicTacToeOnline.Mailing
{
    public class GameEngineListnerThatSendsEmails : GameEngineEventsListnerBase
    {

        public GameEngineListnerThatSendsEmails(IGameEngine gameEngine) : base(gameEngine)
        {
        }

        public override void OnGameEnded(GameSession game)
        {
            if (!game.IsDraw)
            {
                var mailMessage = new MailMessage
                {
                    Subject = "You won in Tic Tac Toe Online",
                    Body = "Congratulations you won in Tic Tac Toe against" + game.Looser.NickName
                };
                mailMessage.To.Add(game.Winner.Email);
                mailMessage.Send(); //uncomment to really send mails
            }
        }
    }
}