﻿using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;

namespace spoBotv4.Dialogs
{
    //mainly to hanlde abandoned queries and do other clean up activities on the IDialog
    public static class DialogExtensions
    {
        public static void NotifyLongRunningOperation<T>(this Task<T> operation, IDialogContext context, Func<T, string> handler)
        {
            operation.ContinueWith(
                async (t, ctx) =>
                {
                    var messageText = handler(t.Result);
                    await NotifyUser((IDialogContext)ctx, messageText);
                },
                context);
        }

        public static void NotifyLongRunningOperation<T>(this Task<T> operation, IDialogContext context, Func<T, IDialogContext, string> handler)
        {
            operation.ContinueWith(
                async (t, ctx) =>
                {
                    var messageText = handler(t.Result, (IDialogContext)ctx);
                    await NotifyUser((IDialogContext)ctx, messageText);
                },
                context);
        }

        public static string GetEntityOriginalText(this EntityRecommendation recommendation, string query)
        {
            if (recommendation.StartIndex.HasValue && recommendation.EndIndex.HasValue)
            {
                return query.Substring(recommendation.StartIndex.Value, recommendation.EndIndex.Value - recommendation.StartIndex.Value + 1);
            }

            return null;
        }

        public static async Task NotifyUser(this IDialogContext context, string messageText)
        {
            if (!string.IsNullOrEmpty(messageText))
            {
                var reply = context.MakeMessage();
                reply.Text = messageText;

                using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, reply))
                {
                    var client = scope.Resolve<IConnectorClient>();
                    await client.Messages.SendMessageAsync(reply);
                }
            }
        }
    }
}