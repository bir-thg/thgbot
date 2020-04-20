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
// Tutorial vor integrating QnA Service (bir)
/*
using System.Linq;
using Microsoft.Bot.Builder.AI.QnA;
*/

namespace Microsoft.BotBuilderSamples.Bots
{
    public class EchoBot : ActivityHandler
    {
        // Initialize Database (Robert, Bir)
        private const string CosmosServiceEndpoint = "https://thg-cosmos-db.documents.azure.com:443/";
        private const string CosmosDBKey = "A7VarPBUH4Ke89T4ro5ychUrGzqJ0O5KRHTYB4JEVvNTVPZDkWUBPhc3LlcgC2tyZSXGGPgqZUnMZBl07NKBxw==";
        private const string CosmosDBDatabaseId = "ToDoList";
        private const string CosmosDBContainerId = "Items";
        // Create local Memory Storage - commented out.
        // private static readonly MemoryStorage doorStorage = new MemoryStorage();
        // Replaces Memory Storage with reference to Cosmos DB.
        private static readonly CosmosDbPartitionedStorage doorStorage = new CosmosDbPartitionedStorage(new CosmosDbPartitionedStorageOptions
        {
            CosmosDbEndpoint = CosmosServiceEndpoint,
            AuthKey = CosmosDBKey,
            DatabaseId = CosmosDBDatabaseId,
            ContainerId = CosmosDBContainerId,
        });

        // Tutorial integrating QnA to Bot (bir)
        /*
        public QnAMaker EchoBotQnA { get; private set; }
        public EchoBot(QnAMakerEndpoint endpoint)
        {
           // connects to QnA Maker endpoint for each turn
           EchoBotQnA = new QnAMaker(endpoint);
        }
        */
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var replyText = $"Ich wiederhole: {turnContext.Activity.Text}";
            
     
            //Easter Egg (Robert)
            if (turnContext.Activity.Text.ToLower().Contains("tür") && turnContext.Activity.Text.ToLower().Contains("öffne"))
            {
                string[] keys = { "Items.doorTries" };
                IDictionary<string,object> triesDoorColl = doorStorage.ReadAsync(keys,cancellationToken).Result;
                string triesDoorStr = (string) triesDoorColl["doorTries"];

                int triesDoor = Convert.ToInt32(triesDoorStr);
                
                replyText = $"Du kannst die Tür nicht öffnen! Es wurde schon {triesDoor} mal versucht, die Tür zu öffnen";

                triesDoor++;

                triesDoorStr = Convert.ToString(triesDoor);
                triesDoorColl["doorTries"] = triesDoorStr;

                await doorStorage.WriteAsync(triesDoorColl,cancellationToken);
            }
            if (turnContext.Activity.Text.ToLower().Contains("hintertür") && turnContext.Activity.Text.ToLower().Contains("öffne"))
            {
                string[] keys = { "backdoorTries" };
                IDictionary<string, object> triesDoorColl = doorStorage.ReadAsync(keys,cancellationToken).Result;
                string triesDoorStr = (string)triesDoorColl["doorTries"];

                int triesBackdoor = Convert.ToInt32(triesDoorStr);

                replyText = $"Warum hast du nicht früher drangedacht?! Die Hintertür wurde schon {triesBackdoor} mal benutzt";

                triesBackdoor++;

                triesDoorStr = Convert.ToString(triesBackdoor);
                triesDoorColl["backdoorTries"] = triesDoorStr;

                await doorStorage.WriteAsync(triesDoorColl,cancellationToken);
            }
            
            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hallo und Willkommen! Wie kann ich Dir heute helfen?";
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
