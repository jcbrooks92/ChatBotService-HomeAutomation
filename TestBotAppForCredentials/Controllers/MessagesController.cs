using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using System.Net.Http.Headers;
using System.Threading;
using Microsoft.IdentityModel.Protocols;
using System.Configuration;
using System.Linq;

namespace FirstBotApp
{
    [BotAuthentication]

    public class Switches
    {
        public string name { get; set; }
        public string value { get; set; }
        public string dimmer { get; set; }

    }

    public class MessagesController : ApiController
    {

        static HttpClient client = new HttpClient();
        static HttpResponseMessage responsemain = null;
        static int j = 0;


        static async Task<Switches[]> GetSwitchesAsync(string path)
        {
            Switches[] switches = null;
            responsemain = await client.GetAsync(path);
            if (responsemain.IsSuccessStatusCode)
            {
                var response1 = await client.GetStringAsync("");
                Console.WriteLine($"{response1}");

                switches = await responsemain.Content.ReadAsAsync<Switches[]>();
            }
            else Console.WriteLine($"Error in response: {responsemain.StatusCode}");

            return switches;
        }

        private async void CheckForDimmingAttribute(HttpClient client, Activity activity, String[] individualInputWords, HttpResponseMessage responsemain, Switches[] switches)
        {
            if (individualInputWords[3] == null)
            {
                responsemain = await client.PutAsJsonAsync($"switches/{individualInputWords.ToArray()[1]}?room={individualInputWords.ToArray()[2].ToLower()}", switches);
            }
            else
            {
                responsemain = await client.PutAsJsonAsync($"setLevel/{individualInputWords.ToArray()[3]}?room={individualInputWords.ToArray()[2].ToLower()}", switches);
            }

            return;
        }

        private async void calltoSmartThings(HttpClient client, Activity activity, String[] individualInputWords, HttpResponseMessage responsemain, Switches[] switches, ConnectorClient connector)
        {
            try
            {
                Activity reply1;
                //responsemain = await client.PutAsJsonAsync($"switches/{individualInputWords.ToArray()[1]}?room={individualInputWords.ToArray()[2].ToLower()}", switches);
                if (individualInputWords.Length<4)
                {
                    responsemain = await client.PutAsJsonAsync($"switches/{individualInputWords.ToArray()[1]}?room={individualInputWords.ToArray()[2].ToLower()}", switches);
                    responsemain.EnsureSuccessStatusCode();
                    switches = await responsemain.Content.ReadAsAsync<Switches[]>();
                    reply1 = activity.CreateReply($"Successfully turned {individualInputWords.ToArray()[1]} the {individualInputWords.ToArray()[2]} light.");
                }
                else
                {
                    responsemain = await client.PutAsJsonAsync($"setLevel/{individualInputWords.ToArray()[3]}?room={individualInputWords.ToArray()[2].ToLower()}", switches);
                    responsemain.EnsureSuccessStatusCode();
                    switches = await responsemain.Content.ReadAsAsync<Switches[]>();
                    reply1 = activity.CreateReply($"Successfully turned {individualInputWords.ToArray()[1]} the {individualInputWords.ToArray()[2]} light. D = {individualInputWords.ToArray()[3]}");
                }

                // Deserialize the updated product from the response body.
                
                await connector.Conversations.ReplyToActivityAsync(reply1);
                return;
            }

            catch
            {
                
                Activity reply1;
                reply1 = activity.CreateReply($"Error: Incorrect input, ({responsemain.StatusCode}) \n Room was {individualInputWords.ToArray()[2]}");
                await connector.Conversations.ReplyToActivityAsync(reply1);
                return;
            }
        }


        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {

       

            Switches[] switches = null;
            MicrosoftAppCredentials creds = new MicrosoftAppCredentials(ConfigurationManager.AppSettings["MicrosoftappID"], ConfigurationManager.AppSettings["MicrosoftappPassword"]);

            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            if (j == 0)
            {
                string accessToken = ConfigurationManager.AppSettings["accessToken"];
                client.BaseAddress = new Uri($"https://graph-na02-useast1.api.smartthings.com:443/api/smartapps/installations/"+ ConfigurationManager.AppSettings["SmartThingsSubscription"] +"/switches");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
                j++;
            }

            if (activity.Type == ActivityTypes.Message)
            {
                Activity reply1;
                string caseInput;

                //Parsing String into words
                var parseForSpaces = activity.Text.Where(Char.IsPunctuation).Distinct().ToArray();
                var individualInputWords = activity.Text.Split().Select(x => x.Trim(parseForSpaces));
                
                //Checking if one word or multiple words were sent in the message
                if (individualInputWords.ToArray().Length > 1)
                {
                    caseInput = individualInputWords.ToArray()[0] + " " + individualInputWords.ToArray()[1];
                }
                else {
                    caseInput = individualInputWords.ToArray()[0];
                }
             
                //Case for responses back to client
                switch (caseInput.ToLower())
                {
                    case "turn on":
                        
                        calltoSmartThings(client, activity, individualInputWords.ToArray(), responsemain, switches, connector);
                        break;
                
                    case "turn off":
                        calltoSmartThings(client, activity, individualInputWords.ToArray(), responsemain, switches, connector);
                        break;


                    default: //"What lights are on?":
                        try
                        {
                            switches = await GetSwitchesAsync("");
                            //ShowSwitches(switches);

                            for (int i = switches.Length - 1; i >= 0; i--)
                            {
                                reply1 = activity.CreateReply($"Room: {switches[i].name}\tStatus: {switches[i].value}\tDimmerLevel: {switches[i].dimmer}");
                                responsemain.EnsureSuccessStatusCode();
                                await connector.Conversations.ReplyToActivityAsync(reply1);
                            }
                            break;
                        }
                        catch
                        {

                        }
                        break;
                }

                // calculate something for us to return
              
            }
            else
            {
                HandleSystemMessage(activity);
            }

            return responsemain;
        }

        static Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
                //Request.CreateResponse(HttpStatusCode.OK);
            }

            return null;
        }
        //client.dispose();

    }
}
