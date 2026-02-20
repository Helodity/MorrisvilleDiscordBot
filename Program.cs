using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace MorrisvilleDiscordBot
{
    internal static class Program
    {
        public static BotConfig config;
        public static ServerConfig serverConfig;
        static async Task Main(string[] args)
        {
            //Load the config settings. If we can't, abort
            if (!TryLoadConfig()) return;

            //Setup the client.
            DiscordClientBuilder builder = 
            DiscordClientBuilder.CreateDefault(
                config.Token, 
                DiscordIntents.AllUnprivileged | DiscordIntents.GuildMembers
            );

            //Add the commands
            builder.UseCommands((IServiceProvider serviceProvider, CommandsExtension extension) =>
            {
                extension.AddCommands([typeof(VerifyCommand), typeof(SetVerifiedRole), typeof(DeverifyAllMembers)]);
            });

            //Add interactivity (used for modal responses)
            builder.UseInteractivity();

            //Connect to Discord
            await builder.ConnectAsync();

            //Indefinite delay to ensure the program doesn end
            await Task.Delay(-1);
        }
   
        static bool TryLoadConfig()
        {
            //Load the bot config
            config = Util.LoadJson<BotConfig>(BotConfig.JsonLocation);

            //Make sure the token is set.
            if (string.IsNullOrWhiteSpace(config.Token))
            {
                Console.WriteLine($"No token is set! Set one in {BotConfig.JsonLocation}");
                Util.SaveJson(config, BotConfig.JsonLocation);
                Console.ReadLine();
                return false;
            }

            //Load the config for individual servers
            serverConfig = Util.LoadJson<ServerConfig>(ServerConfig.JsonLocation);

            return true;
        }
    
    }

    public class VerifyCommand
    {
        //List of characters that can be sent during verification
        readonly static string validCharacters = "abcdefghijklmnopqrstuvwxyz0123456789";

        [Command("verify")]
        [Description("Verify your account for the server!")]
        [RequireGuild]
        public static async ValueTask ExecuteAsync(SlashCommandContext context, string email)
        {
            //Ensure user isn't already verified
            if(context.Member.Roles.Contains(await context.Guild.GetRoleAsync(Program.serverConfig.guildRoleMappings[context.Guild.Id])))
            {
                DiscordInteractionResponseBuilder errorBuilder =
                    new DiscordInteractionResponseBuilder().WithContent("You are already verified!").AsEphemeral();
                await context.RespondAsync(errorBuilder);
                return;
            }


            //Ensure the email is valid.
            if (!Regex.IsMatch(email, Program.config.EmailRegex))
            {
                DiscordInteractionResponseBuilder errorBuilder =
                    new DiscordInteractionResponseBuilder().WithContent("Invalid Email Address!").AsEphemeral();
                await context.RespondAsync(errorBuilder);
                return;
            }


            //Get the interactivity service and make sure it exists
            InteractivityExtension? interactivity = 
                context.Client.ServiceProvider.GetService(typeof(InteractivityExtension)) as InteractivityExtension;
            if (interactivity == null)
            {
                DiscordInteractionResponseBuilder errorBuilder = 
                    new DiscordInteractionResponseBuilder().WithContent("An error occured!").AsEphemeral();
                await context.RespondAsync(errorBuilder);
                return;
            }

            //Create the modal to send to the user
            var codeInput = new DiscordTextInputComponent("codeInput");
            DiscordModalBuilder modal = new DiscordModalBuilder()
                .WithTitle($"{context.Guild.Name} Verification")
                .AddTextInput(codeInput, "Verification Code", "Check your email and place the code here.")
                .WithCustomId($"verificationPrompt[{context.User.Id}]");

            //Send the modal to the user
            await context.RespondWithModalAsync(modal);

            //Generate a verification code, and email it to the given email address
            Random random = new();
            string verificationCode = new string(Enumerable.Repeat(validCharacters, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            if (EmailInterface.SendVerificationEmail(email, context.Guild.Name, verificationCode))
            {    
                //Wait for the user to put the code into the modal
                var modalResponse = await interactivity.WaitForModalAsync($"verificationPrompt[{context.User.Id}]", TimeSpan.FromMinutes(15));
                if (!modalResponse.TimedOut)
                {
                    //Check if the input code and the actual code are the same
                    string userCode = ((TextInputModalSubmission)modalResponse.Result.Values["codeInput"]).Value;
                    if(userCode == verificationCode.ToLower())
                    {
                        //Send a success Response
                        DiscordInteractionResponseBuilder successfulResponseBuilder =
                            new DiscordInteractionResponseBuilder().WithContent("Verification Successful!").AsEphemeral();
                        await modalResponse.Result.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, successfulResponseBuilder);

                        //Give the member the verified role.
                        await context.Member.GrantRoleAsync(
                            await context.Guild.GetRoleAsync(Program.serverConfig.guildRoleMappings[context.Guild.Id])
                        );
                    }
                }

            }
        }
    }

    public class SetVerifiedRole
    { 
        [Command("set_verified_role")]
        [Description("Set the role that is given upon verification.")]
        [RequireGuild]
        [RequirePermissions(DiscordPermission.ManageRoles)]
        public static async ValueTask ExecuteAsync(SlashCommandContext context, DiscordRole role)
        {
            //Update and save the role change
            Program.serverConfig.guildRoleMappings.AddOrUpdate(context.Guild.Id, role.Id);
            Util.SaveJson(Program.serverConfig, ServerConfig.JsonLocation);

            //Respond to the user
            DiscordInteractionResponseBuilder responseBuilder =
                   new DiscordInteractionResponseBuilder().WithContent("Role Updated Successfully!").AsEphemeral();
            await context.RespondAsync(responseBuilder);
        }
    }

    public class DeverifyAllMembers
    {
        [Command("deverify")]
        [Description("Remove verification from ALL server members.")]
        [RequireGuild]
        [RequirePermissions(DiscordPermission.ManageRoles)]
        public static async ValueTask ExecuteAsync(SlashCommandContext context)
        {
            //Respond to the user
            DiscordInteractionResponseBuilder responseBuilder =
                   new DiscordInteractionResponseBuilder().WithContent("Deverifying all members now. This might take a few minute to complete.").AsEphemeral();
            await context.RespondAsync(responseBuilder);

            DiscordRole role = await context.Guild.GetRoleAsync(Program.serverConfig.guildRoleMappings[context.Guild.Id]);
            foreach (DiscordMember member in context.Guild.Members.Values)
            {
                await context.Member.RevokeRoleAsync(role);
            }
        }
    }

}
