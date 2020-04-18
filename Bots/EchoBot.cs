// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class EchoBot : ActivityHandler
    {
        //TODO
        private const string CosmosServiceEndpoint = "<your-cosmos-db-URI>";
        private const string CosmosDBKey = "<your-authorization-key>";
        private const string CosmosDBDatabaseId = "<your-database-id>";
        private const string CosmosDBContainerId = "bot-storage";
        // Create local Memory Storage - commented out.
        // private static readonly MemoryStorage _myStorage = new MemoryStorage();
        // Replaces Memory Storage with reference to Cosmos DB.
        private static readonly CosmosDbPartitionedStorage doorStorage = new CosmosDbPartitionedStorage(new CosmosDbPartitionedStorageOptions
        {
            CosmosDbEndpoint = CosmosServiceEndpoint,
            AuthKey = CosmosDBKey,
            DatabaseId = CosmosDBDatabaseId,
            ContainerId = CosmosDBContainerId,
        });

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var replyText = $"Ich wiederhole: {turnContext.Activity.Text}";
            

            //Easter Egg 
            if (turnContext.Activity.Text.ToUpper().Contains("tür") && turnContext.Activity.Text.ToUpper().Contains("öffne"))
            {
                string[] keys = { "doorTries" };
                IDictionary<string,object> triesDoorColl = doorStorage.ReadAsync(keys).Result;
                string triesDoorStr = (string) triesDoorColl["doorTries"];

                int triesDoor = Convert.ToInt32(triesDoorStr);
                
                replyText = $"Du kannst die Tür nicht öffnen! Es wurde schon {triesDoor} versucht die Tür zu öffnen";

                triesDoor++;

                triesDoorStr = Convert.ToString(triesDoor);
                triesDoorColl["doorTries"] = triesDoorStr;

                await doorStorage.WriteAsync(triesDoorColl);
            }
            if (turnContext.Activity.Text.ToUpper().Contains("hintertür") && turnContext.Activity.Text.ToUpper().Contains("öffne"))
            {
                string[] keys = { "backdoorTries" };
                IDictionary<string, object> triesDoorColl = doorStorage.ReadAsync(keys).Result;
                string triesDoorStr = (string)triesDoorColl["doorTries"];

                int triesBackdoor = Convert.ToInt32(triesDoorStr);

                replyText = $"Warum hast du nicht früher drangedacht?! Die Hintertür wurde schon {triesBackdoor} mal benutzt";

                triesBackdoor++;

                triesDoorStr = Convert.ToString(triesBackdoor);
                triesDoorColl["backdoorTries"] = triesDoorStr;

                await doorStorage.WriteAsync(triesDoorColl);
            }

            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hallo und Willkommen! Wie kann ich Dir helfen?";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}
