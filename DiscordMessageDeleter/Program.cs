using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace DiscordMessageDeleter
{
    class Program
    {
        static DiscordClient client;

        static async Task DeleteDmsMessages(DiscordDmChannel channel)
        {
            Console.WriteLine($"Deleting messages...", Color.Green);


            ulong? first = null;

            int cnt = 0;
            do
            {
                var messages = await channel.GetMessagesAsync(100, first);

                if (messages.Count == 0) break;

                var ownMessages = messages.Where(k => k.Author.Id == client.CurrentUser.Id);

                cnt += ownMessages.Count();
                first = messages.Last().Id;

                foreach (var message in ownMessages)
                    await channel.DeleteMessageAsync(message);
            }
            while (first.HasValue);


            Console.WriteLine($"Deleted {cnt} messages!", Color.Green);
        }

        static async Task DeleteChnMessages(DiscordChannel channel)
        {
            Console.WriteLine($"Deleting messages...", Color.Green);


            ulong? first = null;

            int cnt = 0;
            do
            {
                var messages = await channel.GetMessagesAsync(100, first);

                if (messages.Count == 0) break;

                var ownMessages = messages.Where(k => k.Author.Id == client.CurrentUser.Id);

                cnt += ownMessages.Count();
                first = messages.Last().Id;

                foreach (var message in ownMessages)
                    await channel.DeleteMessageAsync(message);
            }
            while (first.HasValue);


            Console.WriteLine($"Deleted {cnt} messages!", Color.Green);
        }

        static Task DeleteDms()
        {
            
            DiscordDmChannel channel;

            while (true)
            {
                Console.Write($"[Config] Input the DM Id: ", Color.Orange);
                string input = Console.ReadLine();

                if (!ulong.TryParse(input, out ulong dmId))
                {
                    Console.WriteLine($"[Error] Invalid ID specified!", Color.Red);
                    continue;
                }

                channel = client.PrivateChannels.FirstOrDefault(k => k.Id == dmId);

                if(channel == null)
                {
                    var channels = client.PrivateChannels.Where(k => k.Type == ChannelType.Private);

                    channel = channels.FirstOrDefault(k => k.Recipients.FirstOrDefault(k => k.Id == dmId) != null);

                    if(channel == null)
                    {
                        Console.WriteLine($"[Error] The specified DM was not found!", Color.Red);
                        continue;
                    }
                }

                string channelName = channel.Name == null ? $"{channel.Recipients[0].Username}#{channel.Recipients[0].Discriminator}" : channel.Name;

                bool confirm = false;
                while(true)
                {
                    Console.Write($"[Confirm] You choose to delete all of your message from {channelName} channel, is that correct? (y/n): ", Color.Yellow);

                    string choice = Console.ReadLine().ToLower();

                    if(choice != "y" && choice != "n")
                    {
                        Console.WriteLine($"[Error] Invalid choice", Color.Red);
                        continue;
                    }

                    confirm = choice == "y";
                    break;
                }

                if (!confirm)
                    continue;

                break;
            }

            return DeleteDmsMessages(channel);


        }

        static async Task DeleteChn()
        {

            DiscordChannel channel;

            while (true)
            {
                Console.Write($"[Config] Input the channel Id: ", Color.Orange);
                string input = Console.ReadLine();

                if (!ulong.TryParse(input, out ulong channelId))
                {
                    Console.WriteLine($"[Error] Invalid ID specified!", Color.Red);
                    continue;
                }

                channel = await client.GetChannelAsync(channelId);

                if (channel == null)
                {
                    Console.WriteLine($"[Error] The specified channel was not found!", Color.Red);
                    continue;
                }

                bool confirm = false;
                while (true)
                {
                    Console.Write($"[Confirm] You choose to delete all of your message from {channel.Name} channel in {channel.Guild.Name} server, is that correct? (y/n): ", Color.Yellow);

                    string choice = Console.ReadLine().ToLower();

                    if (choice != "y" && choice != "n")
                    {
                        Console.WriteLine($"[Error] Invalid choice", Color.Red);
                        continue;
                    }

                    confirm = choice == "y";
                    break;
                }

                if (!confirm)
                    continue;

                break;
            }

            await DeleteChnMessages(channel);


        }


        static async Task DeleteSrv()
        {

            DiscordGuild guild;

            while (true)
            {
                Console.Write($"[Config] Input the server Id: ", Color.Orange);
                string input = Console.ReadLine();

                if (!ulong.TryParse(input, out ulong channelId))
                {
                    Console.WriteLine($"[Error] Invalid ID specified!", Color.Red);
                    continue;
                }

                try
                {
                    guild = await client.GetGuildAsync(channelId);
                }
                catch(Exception)
                {
                    Console.WriteLine($"[Error] The specified channel was not found!", Color.Red);
                    continue;
                }

                if (guild == null)
                {
                    Console.WriteLine($"[Error] The specified channel was not found!", Color.Red);
                    continue;
                }

                bool confirm = false;
                while (true)
                {
                    Console.Write($"[Confirm] You choose to delete all of your message from {guild.Name} server, is that correct? (y/n): ", Color.Yellow);

                    string choice = Console.ReadLine().ToLower();

                    if (choice != "y" && choice != "n")
                    {
                        Console.WriteLine($"[Error] Invalid choice", Color.Red);
                        continue;
                    }

                    confirm = choice == "y";
                    break;
                }

                if (!confirm)
                    continue;

                break;
            }

            foreach(var channel in guild.Channels)
            {
                Console.WriteLine($"[Server] Deleting {channel.Name} messages...", Color.Green);
                await DeleteChnMessages(channel);
            }



        }


        static async Task Gather()
        {
            string choice = "";
            while(true)
            {
                Console.Write($"[Config] Would you like to delete your message from a DM, a channel or a whole server? (dm/chn/srv): ", Color.Orange);
                choice = Console.ReadLine().ToLower();

                if (choice != "dm" && choice != "chn" && choice != "srv")
                {
                    Console.WriteLine($"[Error] Your choice is not valid!", Color.Red);
                    continue;
                }


                switch (choice)
                {
                    case "dm":
                        await DeleteDms();
                        break;

                    case "chn":
                        await DeleteChn();
                        break;

                    case "srv":
                        await DeleteSrv();
                        break;
                }
            }

        }

        static async Task Auth()
        {
            while(true)
            {
                Console.Write($"[Config] Input your Discord Token: ", Color.Orange);
                string token = Console.ReadLine();

                Console.WriteLine($"[Auth] Authenticating...", Color.Yellow);

                client = new DiscordClient(new DiscordConfiguration()
                {
                    Token = token,
                    TokenType = TokenType.User
                });

                try
                {
                    await client.ConnectAsync();
                    break;
                }
                catch(Exception ex)
                {
                    client.Dispose();
                    client = null;
                    Console.WriteLine($"[Error] {ex.Message}", Color.Red);
                    continue;
                }
            }


            Console.WriteLine($"[Auth] Authentication suceeded for {client.CurrentUser.Username}#{client.CurrentUser.Discriminator} user!", Color.Green);

            await Gather();
        }
        static void Main(string[] args)
        {
            Console.WriteLine($"Discord Message Deleter - by Aesir - Nulled: SickAesir | Discord: Aesir#1337 | Telegram: @sickaesir", Color.Cyan);
            Auth().ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
