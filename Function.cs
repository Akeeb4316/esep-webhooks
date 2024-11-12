using System.Text;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using System.Net.Http;
using System;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook;

public class Function
{
    /// <summary>
    /// A function that handles a GitHub webhook and sends a message to Slack.
    /// </summary>
    /// <param name="input">The event input from GitHub webhook.</param>
    /// <param name="context">The Lambda context.</param>
    /// <returns>A response string.</returns>
    public string FunctionHandler(object input, ILambdaContext context)
    {
        // Log input to CloudWatch
        context.Logger.LogInformation($"FunctionHandler received: {input}");

        // Deserialize GitHub webhook payload into dynamic object
        dynamic json = JsonConvert.DeserializeObject<dynamic>(input.ToString());

        // Prepare payload for Slack with the issue URL
        string payload = $"{{'text':'Issue Created: {json.issue.html_url}'}}";

        // Send POST request to Slack webhook URL
        var client = new HttpClient();
        var webRequest = new HttpRequestMessage(HttpMethod.Post, Environment.GetEnvironmentVariable("SLACK_URL"))
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        var response = client.Send(webRequest);

        // Read and return Slack response
        using var reader = new StreamReader(response.Content.ReadAsStream());
        return reader.ReadToEnd();
    }
}
