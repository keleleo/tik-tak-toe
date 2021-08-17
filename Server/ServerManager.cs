using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net;
using System.Net.Sockets;
namespace Server2
{
    class ServerManager
    {
        public static ServerManager instance;
        private Dictionary<string, ClientInfo> ClientsInfo = new Dictionary<string, ClientInfo>();
        private Dictionary<TcpClient, string> tcpToToken = new Dictionary<TcpClient, string>();
        private List<string> waitingParty = new List<string>();
        private Dictionary<string, Game> games = new Dictionary<string, Game>();
        private Dictionary<string, string> tokenToGameCode = new();
        public ServerManager()
        {
            ServerManager.instance = this;
        }
        public void Start()
        {
            Server.instance.messageReceived += Instance_messageReceived;
            Server.instance.clientConnected += Instance_clientConnected;
            Server.instance.clientDisconnected += Instance_clientDisconnected;
        }
        private void Instance_clientConnected(TcpClient client)
        {

        }
        private void Instance_clientDisconnected(TcpClient client)
        {
            try
            {
                if (!client.GetStream().Socket.Poll(1000, SelectMode.SelectRead))
                    return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }


            string token = tcpToToken.GetValueOrDefault(client);

            if (token != "" && token != null && token.Length > 4)
            {
                string gameCode = tokenToGameCode.GetValueOrDefault(token);
                if (gameCode != "" && gameCode != null && gameCode.Length > 4)
                    PlayerExitGame(gameCode);

                tcpToToken.Remove(client);
                waitingParty.Remove(token);

                if (ClientsInfo.ContainsKey(token))
                    ClientsInfo.Remove(token);
            }
        }

        public void SeePlayersConnected()
        {
            Console.WriteLine($"-----------------------------------");
            ClientsInfo.ToList().ForEach((x) =>
            {
                Console.WriteLine($"-------    {x.Value.user} -- {x.Value.loginToken} -- {x.Value.tcp.Client.RemoteEndPoint}");
            });
            Console.WriteLine($"-----------------------------------");
        }
        public void SeeGames()
        {
            Console.WriteLine($"-----------------------------------");
            games.ToList().ForEach((x) =>
            {
                string grid = "";
                for (int i = 0; i < 9; i++)
                    grid += i == 0 ? x.Value.GameState[i] : "--" + x.Value.GameState[i];
                Console.WriteLine($"-------    {x.Value.Players[0]} -- {x.Value.Players[1]} -- {x.Value._C}");
            });
            Console.WriteLine($"-----------------------------------");
        }
        private void Instance_messageReceived(string json, TcpClient client)
        {
            Message message = JsonSerializer.Deserialize<Message>(json);
            Console.WriteLine($"Received type: {message.type}");
            JsonDocument doc = JsonDocument.Parse(json);
            JsonElement json2 = doc.RootElement;
            bool mObj = (json2.GetProperty("messageObj").ToString()) != "" && (json2.GetProperty("messageObj").ToString().Length > 2);
            

            JsonDocument docMessageObj;
            JsonElement jsonMessageObj = new();
            if (mObj)
            {
                docMessageObj = JsonDocument.Parse(json2.GetProperty("messageObj").ToString());
                jsonMessageObj = docMessageObj.RootElement;
            }

            message.messageObj = json2.GetProperty("messageObj").ToString();

            if (message.token == "" && message.type != Communication.Type.login)
            {
                return;
            }
            if (message.token != "" && message.token != null && message.token.Length > 4 && message.type == Communication.Type.login)
            {
                Console.WriteLine("wtf");
                return;
            }
            if (mObj && message.token == "" && message.type == Communication.Type.login)
            {
                Server2.Login login = new Server2.Login();
                login.user = jsonMessageObj.GetProperty("user").ToString();
                login.token = jsonMessageObj.GetProperty("token").ToString();
                Login(client, login);
                return;
            }

            ClientInfo clientInfo = ClientsInfo.GetValueOrDefault(message.token);
            if (clientInfo == null)
            {
                return;
            }

            if (mObj && message.type == Communication.Type.gameAction)
            {
                GameAction(clientInfo, jsonMessageObj);
                return;
            }
            if (message.type == Communication.Type.gameFindParty)
            {
                PlayerFindParty(clientInfo);
                PlayerFindParty(clientInfo);
                return;
            }
            if (message.type == Communication.Type.gameStopFindParty)
            {
                PlayerStopFindParty(clientInfo);
                return;
            }
            if (message.type == Communication.Type.gameState)
            {
                GameRequestState(clientInfo);
            }
            if (message.type == Communication.Type.gamePlayerReady)
            {
                GamePlayerReady(clientInfo);
            }
            if (message.type == Communication.Type.gamePlayerReadyCancel)
            {
                GamePlayerReadyCancel(clientInfo);
            }

        }

        private void Login(TcpClient client, Login loginInfo)
        {

            if (loginInfo.user.Length > 4)
            {
                string token;
                token = AddClientToList(client, loginInfo.user);
                Login login = new Login();
                login.user = loginInfo.user;
                login.token = token;
                Send(client, Communication.Type.login, login);
            }
        }
        private string AddClientToList(TcpClient client, string user)
        {
            string token = Guid.NewGuid().ToString();
            ClientInfo cf = new ClientInfo();
            cf.tcp = client;
            cf.loginToken = token;
            cf.user = user;

            ClientsInfo.Add(token, cf);
            tcpToToken.Add(cf.tcp, token);
            return token;
        }

        private void GameRequestState(ClientInfo client)
        {
            GameState gameState = new();
            if (client.gameCode != null && client.gameCode.Length > 4)
            {
                Game game = games.GetValueOrDefault(client.gameCode);
                gameState.gameState = game.GameState;
                gameState._c = game._C;
                gameState.whoIsMe = game.WhoIsMe(client.loginToken);
                Send(client.tcp, Communication.Type.gameState, gameState);
            }
        }
        private void GameAction(ClientInfo client, JsonElement json)
        {
            GameAction gameAction = new GameAction();
            gameAction.square = (int)json.GetProperty("square").GetInt32();

            string gameCoode = client.gameCode;

            Game game = games.GetValueOrDefault(gameCoode);
            bool actionValid = game.Action(client, gameAction.square);
            if (actionValid)
            {
                ClientInfo player1 = ClientsInfo.GetValueOrDefault(game.Players[0]);
                ClientInfo player2 = ClientsInfo.GetValueOrDefault(game.Players[1]);

                bool verficPlayer1 = VerificPlayer(player1);
                bool verficPlayer2 = VerificPlayer(player2);
                if (!verficPlayer1)
                {
                    return;
                }
                if (!verficPlayer2)
                {
                    return;
                }

                GameRequestState(player1);
                GameRequestState(player2);

                int winner = game.GetWinner();
                if (winner == 9999)
                    return;
                if (winner == 0)
                {
                    Send(player1.tcp, Communication.Type.gameYouWon);
                    Send(player2.tcp, Communication.Type.gameYouLose);
                }
                else if (winner == 1)
                {
                    Send(player1.tcp, Communication.Type.gameYouLose);
                    Send(player2.tcp, Communication.Type.gameYouWon);
                }
                else if (winner == 2)
                {
                    Send(player1.tcp, Communication.Type.gameNoWinner);
                    Send(player2.tcp, Communication.Type.gameNoWinner);
                }
            }
        }

        private void GamePlayerReadyCancel(ClientInfo client)
        {
            Game game = games.GetValueOrDefault(client.gameCode);
            game.PlayerReadyCancel(client);
        }
        private void GamePlayerReady(ClientInfo client)
        {
            Game game = games.GetValueOrDefault(client.gameCode);
            bool allReady = game.PlayerReady(client);
            if (allReady)
            {
                game.startGame();
                ClientInfo player1 = ClientsInfo.GetValueOrDefault(game.Players[0]);
                ClientInfo player2 = ClientsInfo.GetValueOrDefault(game.Players[1]);

                Send(player1.tcp, Communication.Type.gameStartGame);
                Send(player2.tcp, Communication.Type.gameStartGame);

                GameRequestState(player1);
                GameRequestState(player2);
            }
        }

        private void PlayerStopFindParty(ClientInfo client)
        {
            if (client.state == ClientInfo.State.findingParty)
            {
                waitingParty.Remove(client.loginToken);
                client.state = ClientInfo.State.home;
            }
        }
        private void PlayerFindParty(ClientInfo client)
        {
            try
            {
                if (client.state == ClientInfo.State.home)
                {
                    client.state = ClientInfo.State.findingParty;

                    waitingParty.Add(client.loginToken);

                    Console.WriteLine($"FiendParty-> waiting Party count {waitingParty.Count}");

                    if (waitingParty.Count >= 2)
                    {
                        string code = Guid.NewGuid().ToString();
                        ClientInfo player1 = ClientsInfo.GetValueOrDefault(waitingParty[0]);
                        ClientInfo player2 = ClientsInfo.GetValueOrDefault(waitingParty[1]);
                        bool verficPlayer1 = VerificPlayer(player1);
                        bool verficPlayer2 = VerificPlayer(player2);

                        if (!verficPlayer1)
                        {
                            return;
                        }
                        if (!verficPlayer2)
                        {
                            return;
                        }
                        player1.state = ClientInfo.State.playing;
                        player2.state = ClientInfo.State.playing;
                        player1.gameCode = code;
                        player2.gameCode = code;

                        tokenToGameCode.Add(player1.loginToken, code);
                        tokenToGameCode.Add(player2.loginToken, code);

                        waitingParty.Remove(player1.loginToken);
                        waitingParty.Remove(player2.loginToken);

                        Game game = new();
                        game.SetPlayers(player1.loginToken, player2.loginToken);
                        game.startGame();
                        Send(player1.tcp, Communication.Type.gameFoundParty);
                        Send(player2.tcp, Communication.Type.gameFoundParty);

                        games.Add(code, game);
                    }
                }
            }catch(Exception _ex)
            {
                Console.WriteLine(_ex);
            }
        }
        
        private void PlayerExitGame(string gameCode)
        {
            Game game = games.GetValueOrDefault(gameCode);

            ClientInfo player1 = ClientsInfo.GetValueOrDefault(game.Players[0]);
            ClientInfo player2 = ClientsInfo.GetValueOrDefault(game.Players[1]);

            bool verficPlayer1 = VerificPlayer(player1);
            bool verficPlayer2 = VerificPlayer(player2);

            if (verficPlayer1)
            {
                player1.gameCode = null;
                player1.state = ClientInfo.State.home;
                tokenToGameCode.Remove(player1.loginToken);
                Send(player1.tcp, Communication.Type.gamePartyLost);
            }
            if (verficPlayer2)
            {
                player2.gameCode = null;
                player2.state = ClientInfo.State.home;
                tokenToGameCode.Remove(player2.loginToken);
                Send(player2.tcp, Communication.Type.gamePartyLost);
            }
            games.Remove(gameCode);
            //gamePartyLost
        }
        private bool VerificPlayer(ClientInfo client)
        {
            bool r = false;
            try
            {
                r = !client.tcp.GetStream().Socket.Poll(1000, SelectMode.SelectRead);
                if (!r)
                {
                    waitingParty.Remove(client.loginToken);
                    ClientsInfo.Remove(client.loginToken);
                    tcpToToken.Remove(client.tcp);

                }
            }
            catch (Exception ex)
            {

            }
            return r;
        }
        public void Send(TcpClient client, Communication.Type type)
        {
            
            Message message = new Message();
            message.type = type;
            message.token = "server-0001";
            message.messageObj = "";

            string msg = JsonSerializer.Serialize(message);

            msg += "@";
            Byte[] data = Encoding.ASCII.GetBytes(msg);
            client.GetStream().Write(data, 0, data.Length);
        }
        private void Send<t>(TcpClient client, Communication.Type type, t messageObj)
        {
            Message message = new Message();
            message.type = type;
            message.token = "server-0001";

            string obj = JsonSerializer.Serialize(messageObj);
            message.messageObj = obj;

            string msg = JsonSerializer.Serialize(message);
            msg += "@";

            Byte[] data = Encoding.ASCII.GetBytes(msg);
            client.GetStream().Write(data, 0, data.Length);
        }

        class ClientInfo
        {
            public TcpClient tcp;
            public string loginToken = "";
            public string user;
            public string gameCode = "";
            public enum State
            {
                findingParty,
                playing,
                home
            }
            public State state = State.home;
        }
        class Game
        {
            private string[] playersToken = new string[2];
            private bool[] playOK = new bool[2] { false, false };
            private int _c = 0;
            private bool paused = true;
            private int[] gameState = new int[9] {
                0,1,2,
                3,4,5,
                6,7,8
            };

            public int _C { get => _c; }
            public int[] GameState { get => gameState; }
            public string[] Players { get => playersToken; }
            public void SetPlayers(string player1, string player2)
            {
                playersToken[0] = player1;
                playersToken[1] = player2;
            }
            public int WhoIsMe(string playersToken)
            {
                return this.playersToken[0] == playersToken ? 0 : 1;
            }


            public void PlayerReadyCancel(ClientInfo player)
            {
                if (WhoIsMe(player.loginToken) == 0)
                    playOK[0] = true;
                else
                    playOK[1] = true;
            }
            public bool PlayerReady(ClientInfo player)
            {
                if (WhoIsMe(player.loginToken) == 0)
                    playOK[0] = true;
                else
                    playOK[1] = true;

                if (playOK[0] && playOK[1])
                    return true;

                return false;
            }

            public bool Action(ClientInfo player, int square)
            {
                if (paused)
                    return false;

                if (!(player.loginToken == playersToken[_c]))
                {
                    return false;
                }
                if (square < 9 || square >= 0)
                {
                    if (gameState[square] == 0)
                    {
                        if (_c == 0)
                            gameState[square] = 1;
                        else
                            gameState[square] = 2;

                        _c = _c == 0 ? 1 : 0;

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
                return false;
            }
            public void startGame()
            {
                gameState = new int[9]
                {
                    0,0,0,
                    0,0,0,
                    0,0,0
                };
                playOK = new bool[2] { false, false };
                Random random = new Random();
                _c = random.Next(0, 2);
                paused = false;
            }
            public int GetWinner()
            {
                if (VericGrid(0))
                    return 0;
                if (VericGrid(1))
                    return 1;
                int noWinner = 0;
                for (int i = 0; i < 9; i++)
                {
                    if (gameState[i] != 0)
                        noWinner++;
                }
                Console.WriteLine($"squares: {noWinner}");
                if (noWinner == 9)
                    return 2;
                return 9999;
            }
            private bool VericGrid(int playerIndex)
            {
                int c = playerIndex == 0 ? 1 : 2;
                // -
                if (gameState[0] == c && gameState[1] == c && gameState[2] == c)
                    return true;
                if (gameState[3] == c && gameState[4] == c && gameState[5] == c)
                    return true;
                if (gameState[6] == c && gameState[7] == c && gameState[8] == c)
                    return true;
                // |
                if (gameState[0] == c && gameState[3] == c && gameState[6] == c)
                    return true;
                if (gameState[1] == c && gameState[4] == c && gameState[7] == c)
                    return true;
                if (gameState[2] == c && gameState[5] == c && gameState[8] == c)
                    return true;
                // \
                if (gameState[0] == c && gameState[4] == c && gameState[8] == c)
                    return true;
                // /
                if (gameState[2] == c && gameState[4] == c && gameState[6] == c)
                    return true;
                if (gameState[6] == c && gameState[7] == c && gameState[8] == c)
                    return true;

                return false;
            }

        }
    }
}

