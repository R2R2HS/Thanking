﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordBot
{
    public class MessageHandler
    {
        public static async Task InitResponses() => Vars.Client.MessageReceived += Respond;

        private static async Task Respond(SocketMessage Context)
        {
            String Content = Context.Content.ToUpperInvariant();

            if (Content.Contains("FEATURES"))
            {
                await Context.Channel.SendMessageAsync("", embed: await Vars.Features());
                return;
            }

            if (Content.Contains("BUY"))
            {
                await Context.Channel.SendMessageAsync("Pay at https://paypal.me/Teo427/35 then message zoomy#3127.");
                return;
            }
        }
        
        #region Logger
        
        public static async Task InitMessageLogger() => Vars.Client.MessageReceived += LogMessage;

        private static async Task LogMessage(SocketMessage Context)
        {
            if (!(Context.Channel is IDMChannel)) return;

            if (Context.Author.IsBot) return;
            
            Utilities.Log(Context.Content, Context.Channel);
        }
        
        #endregion
        
        #region Commands

        public static async Task InitCMDs()
        {
            Vars.Commands = new CommandService();

            await Vars.Commands.AddModulesAsync(Assembly.GetEntryAssembly());

            Vars.Client.MessageReceived += HandleCommand;
        }

        private static async Task HandleCommand(SocketMessage CommandParameter)
        {   
            if (!await CommandParameter.Author.IsAdministrator()) return;
            
            SocketUserMessage Message = CommandParameter as SocketUserMessage;

            if (Message == null) return;

            int argPos = 0;
            
            if (!Message.HasCharPrefix('*', ref argPos)) return;
            
            CommandContext Context = new CommandContext(Vars.Client, Message);

            IResult Result = await Vars.Commands.ExecuteAsync(Context, argPos);

            if (!Result.IsSuccess)
                await Message.Channel.SendMessageAsync($"**Error:** {Result.ErrorReason}");
        }
        
        #endregion
    }
}