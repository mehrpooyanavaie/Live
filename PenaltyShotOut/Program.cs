using System;
using System.Threading;
using StackExchange.Redis;
using Microsoft.AspNetCore.SignalR.Client;

class Program
{
    private static ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
    private static IDatabase db = redis.GetDatabase();
    private static HubConnection hubConnection;

    static void Main(string[] args)
    {
        Console.CursorVisible = false;
        Console.WriteLine("Welcome to the Penalty Shootout Game!");

        hubConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:44310/gameHub")
            .Build();

        hubConnection.StartAsync().Wait();

        int rounds = 5;
        ResetScores();

        for (int i = 1; i <= rounds; i++)
        {
            Console.Clear();
            Console.WriteLine($"\nRound {i}:");

            Console.WriteLine("Player 1");
            Thread.Sleep(2000);
            int player1Result = TakeShot("Player 1");

            if (player1Result == 1) IncrementScore("Player1Score");
            DisplayShotResult(player1Result, "Player 1");

            Console.WriteLine("Player 2");
            Thread.Sleep(2000);
            int player2Result = TakeShot("Player 2");

            if (player2Result == 1) IncrementScore("Player2Score");
            DisplayShotResult(player2Result, "Player 2");

            DisplayScore();
            UpdateScoreOnServer();
        }

        DisplayFinalResult();
        UpdateFinalResultOnServer();
    }

    static void ResetScores()
    {
        db.StringSet("Player1Score", 0);
        db.StringSet("Player2Score", 0);
    }

    static void IncrementScore(string playerKey)
    {
        db.StringIncrement(playerKey);
    }

    static int GetScore(string playerKey)
    {
        return (int)db.StringGet(playerKey);
    }

    static void UpdateScoreOnServer()
    {
        int player1Score = GetScore("Player1Score");
        int player2Score = GetScore("Player2Score");
        hubConnection.InvokeAsync("UpdateScore", player1Score, player2Score);
    }

    static void UpdateFinalResultOnServer()
    {
        int player1Score = GetScore("Player1Score");
        int player2Score = GetScore("Player2Score");
        hubConnection.InvokeAsync("FinalResult", player1Score, player2Score);
    }

    static int TakeShot(string player)
    {
        Random random = new Random();
        int randomNumber = random.Next(0, 100);
        int result;

        if (randomNumber < 50)
        {
            result = 1;
        }
        else
        {
            int outcome = randomNumber % 3;
            if (outcome == 0)
            {
                result = 0;
            }
            else if (outcome == 1)
            {
                result = 2;
            }
            else
            {
                result = 3;
            }
        }

        int shotDirection = random.Next(0, 5);
        int goalkeeperMove = random.Next(0, 3);

        DisplayScene(player, shotDirection, goalkeeperMove, result);

        return result;
    }

    static void DisplayScene(string player, int shotDirection, int goalkeeperMove, int result)
    {
        int playerPosition = 5;
        int ballPosition = playerPosition + 3;
        int goalkeeperPosition = 50;
        int goalTop = 10;
        int goalBottom = 15;
        int shotY = 12;

        Console.Clear();
        Console.WriteLine($"{player} is taking the shot...");

        for (int i = 0; i < 45; i++)
        {
            Console.Clear();
            DrawGoal(goalkeeperPosition, goalTop, goalBottom);
            DrawPlayer(playerPosition, shotY);
            DrawGoalkeeper(goalkeeperPosition, shotY, goalkeeperMove, i);

            int currentBallPosition = ballPosition + i;

            if (currentBallPosition <= goalkeeperPosition)
            {
                DrawBall(currentBallPosition, shotY);
            }
            else if (result == 0 && currentBallPosition <= goalkeeperPosition + 10)
            {
                DrawBall(currentBallPosition, shotY);
            }
            else if (result == 2 && currentBallPosition == goalkeeperPosition)
            {
                DrawBall(goalkeeperPosition, shotY + goalkeeperMove * 2 - 2);
                break;
            }
            else if (result == 3 && currentBallPosition == goalkeeperPosition)
            {
                DrawBall(goalkeeperPosition - 1, shotY + (goalkeeperMove - 1) * 2);
                break;
            }
            else if (result == 1 && currentBallPosition == goalkeeperPosition + 10)
            {
                DrawBall(goalkeeperPosition + 10, shotY);
                break;
            }
            else if (currentBallPosition > goalkeeperPosition + 10)
            {
                DrawBall(goalkeeperPosition + 10, shotY);
                break;
            }

            Thread.Sleep(100);
        }


        if (result == 0)
        {
            Console.SetCursorPosition(goalkeeperPosition + 12, shotY);
            Console.WriteLine("OUT!");
        }
        else if (result == 1)
        {
            Console.SetCursorPosition(goalkeeperPosition + 12, shotY);
            Console.WriteLine("GOAL!");
        }
        else if (result == 2)
        {
            Console.SetCursorPosition(goalkeeperPosition + 12, shotY);
            Console.WriteLine("SAVE!");
        }
        else if (result == 3)
        {
            Console.SetCursorPosition(goalkeeperPosition + 12, shotY);
            Console.WriteLine("POST!");
        }

        Thread.Sleep(1000);
    }

    static void DrawPlayer(int x, int y)
    {
        Console.SetCursorPosition(x, y - 1);
        Console.WriteLine(" O ");
        Console.SetCursorPosition(x, y);
        Console.WriteLine("/|\\");
        Console.SetCursorPosition(x, y + 1);
        Console.WriteLine("/ \\");
    }

    static void DrawGoalkeeper(int x, int y, int moveDirection, int frame)
    {
        int offset = (frame >= 15) ? (moveDirection - 1) * 5 : 0;
        Console.SetCursorPosition(x + offset, y - 1);
        Console.WriteLine(" \\O/ ");
        Console.SetCursorPosition(x + offset, y);
        Console.WriteLine("  |  ");
        Console.SetCursorPosition(x + offset, y + 1);
        Console.WriteLine(" / \\ ");
    }

    static void DrawBall(int x, int y)
    {
        Console.SetCursorPosition(x, y);
        Console.WriteLine(" o ");
    }

    static void DrawGoal(int x, int top, int bottom)
    {
        Console.SetCursorPosition(x - 1, top - 1);
        Console.WriteLine(new string('#', 27));

        for (int i = top; i <= bottom; i++)
        {
            Console.SetCursorPosition(x - 1, i);
            Console.WriteLine("#                         #");
        }

        Console.SetCursorPosition(x - 1, bottom + 1);
        Console.WriteLine(new string('#', 27));
    }

    static void DisplayShotResult(int result, string player)
    {
        switch (result)
        {
            case 1:
                Console.WriteLine($"{player} scored a goal! ⚽");
                break;
            case 2:
                Console.WriteLine($"{player}'s shot was saved! ❌");
                break;
            case 3:
                Console.WriteLine($"{player}'s shot hit the post! 🥅");
                break;
            default:
                Console.WriteLine($"{player} missed the shot! ❌");
                break;
        }
    }

    static void DisplayScore()
    {
        int player1Score = GetScore("Player1Score");
        int player2Score = GetScore("Player2Score");
        Console.WriteLine($"Score: Player 1 - {player1Score} | Player 2 - {player2Score}");
    }

    static void DisplayFinalResult()
    {
        int player1Score = GetScore("Player1Score");
        int player2Score = GetScore("Player2Score");

        Console.WriteLine("\nFinal Result:");
        Console.WriteLine($"Player 1: {player1Score}");
        Console.WriteLine($"Player 2: {player2Score}");

        if (player1Score > player2Score)
        {
            Console.WriteLine("Player 1 wins the game! 🏆");
        }
        else if (player2Score > player1Score)
        {
            Console.WriteLine("Player 2 wins the game! 🏆");
        }
        else
        {
            Console.WriteLine("The game is a draw! 🤝");
        }
    }
}