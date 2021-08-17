using System;
namespace server
{
    public class Communication
    {
        public enum Type
        {
            TestConnection,
            errorJson,
            login,
            gameAction,
            gameState,
            gameFindParty,
            gameStopFindParty,
            gameFoundParty,
            gamePartyLost,

            gameStartGame,
            gamePlayerReady,
            gamePlayerReadyCancel,
            gameYouLose,
            gameYouWon,
            gameNoWinner
        }
    }

    [Serializable]
    public class Message
    {
        public Communication.Type type;
        public string token;
        public string messageObj;
    }

    [Serializable]
    public class Login
    {
        public string user;
        //public string pass;
        public string token;
    }

    [Serializable]
    public class GameAction
    {
        public int square;
    }

    [Serializable]
    public class GameState
    {
        public int _c;
        public int whoIsMe;
        public int[] gameState;
    }
}
