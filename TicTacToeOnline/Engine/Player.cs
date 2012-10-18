namespace TicTacToeOnline.Engine
{
    public class Player
    {
        public Player(string id, string email, string nickName)
        {
            Id = id;
            Email = email;
            NickName = nickName;
        }

        public string Id { get; private set; }
        public string Email { get; private set; }
        public string NickName { get; private set; }
    }
}