using Twilio.AspNet.Core.MinimalApi;
using Twilio.TwiML;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/message", () =>
{
    var messagingResponse = new MessagingResponse();
    messagingResponse.Message("Ahoy!");
    return Results.Extensions.TwiML(messagingResponse);
});

app.Run();