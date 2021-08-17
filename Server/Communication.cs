using System;

namespace Server2
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
        public Communication.Type type { get; set; }
        public string token { get; set; }
        public string messageObj { get; set; }
    }

    [Serializable]
    public class Login
    {
        public string user { get; set; }
        //public string pass;
        public string token { get; set; }
    }

    [Serializable]
    public class GameAction
    {
        public int square { get; set; }
    }

    [Serializable]
    public class GameState
    {
        public int _c { get; set; }
        public int whoIsMe { get; set; }
        public int[] gameState { get; set; }
    }
}
