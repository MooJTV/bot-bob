#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    class BobBot
    {
        private DateTime? lastMorningTime;
        private DateTime? lastAfternoonTime;
        private DateTime? lastNightTime;
        private DateTime? lastAiMessageTime;
        private Random random = new Random();

        private bool IsTimeForGreeting(int startHour, int endHour)
        {
            var currentHour = DateTime.Now.Hour;
            return currentHour >= startHour && currentHour < endHour;
        }

        private bool CanSendGreeting(DateTime? lastTime)
        {
            if (!lastTime.HasValue)
                return true;
            
            var timeSinceLast = DateTime.Now - lastTime.Value;
            return timeSinceLast.TotalHours >= 24;
        }

        private bool CanSendAiMessage()
        {
            if (!lastAiMessageTime.HasValue)
                return true;
            
            var timeSinceLast = DateTime.Now - lastAiMessageTime.Value;
            return timeSinceLast.TotalHours >= 2;
        }

        private (string greeting, GreetingType type)? GetCurrentGreeting()
        {
            // Morning: 5 AM - 12 PM
            if (IsTimeForGreeting(5, 12) && CanSendGreeting(lastMorningTime))
                return ("Good Morning", GreetingType.Morning);
            
            // Afternoon: 12 PM - 6 PM
            if (IsTimeForGreeting(12, 18) && CanSendGreeting(lastAfternoonTime))
                return ("Good Afternoon", GreetingType.Afternoon);
            
            // Night: 6 PM - 5 AM
            if ((IsTimeForGreeting(18, 24) || IsTimeForGreeting(0, 5)) && CanSendGreeting(lastNightTime))
                return ("Good Night", GreetingType.Night);

            return null;
        }

        private enum GreetingType
        {
            Morning,
            Afternoon,
            Night
        }

        private void UpdateGreetingTime(GreetingType type)
        {
            var now = DateTime.Now;
            switch (type)
            {
                case GreetingType.Morning:
                    lastMorningTime = now;
                    break;
                case GreetingType.Afternoon:
                    lastAfternoonTime = now;
                    break;
                case GreetingType.Night:
                    lastNightTime = now;
                    break;
            }
        }

        public string? Speak()
        {
            var now = DateTime.Now;

            // Check for AI message (2-5 hour interval)
            if (CanSendAiMessage() && random.NextDouble() < 0.2)
            {
                lastAiMessageTime = now;
                return "AI is dehumanizing technology";
            }

            // Check for time-appropriate greeting
            var greetingInfo = GetCurrentGreeting();
            if (greetingInfo.HasValue)
            {
                UpdateGreetingTime(greetingInfo.Value.type);
                return $"{greetingInfo.Value.greeting}, fart time";
            }

            return null;
        }
    }

    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Bob Bot is running! ===");
        Console.WriteLine("(Press Ctrl+C or close the tab to stop)");
        Console.WriteLine("Time periods for greetings:");
        Console.WriteLine("Morning: 5 AM - 12 PM");
        Console.WriteLine("Afternoon: 12 PM - 6 PM");
        Console.WriteLine("Night: 6 PM - 5 AM");
        Console.WriteLine("Each greeting can appear once per 24h");
        Console.WriteLine("================================\n");

        var bob = new BobBot();
        var random = new Random();
        var cancellationTokenSource = new CancellationTokenSource();

        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            cancellationTokenSource.Cancel();
        };

        try
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                var message = bob.Speak();
                if (!string.IsNullOrEmpty(message))
                {
                    Console.WriteLine($"Bob: {message} [Server time: {DateTime.Now:HH:mm}]");
                }

                // Random delay between 2-5 seconds
                await Task.Delay(random.Next(2000, 5000), cancellationTokenSource.Token);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\nBob Bot is shutting down...");
        }
    }
}
