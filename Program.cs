using Twilio.AspNet.Core.MinimalApi;
using Twilio.TwiML;
using Twilio.AspNet.Core;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

var builder = WebApplication.CreateBuilder(args);

// add TwilioRestClient to Dependency Injection Container
// configured using 'Twilio:Client:AccountSid' and 'Twilio.Client:AuthToken'
builder.Services.AddTwilioClient();

var app = builder.Build();

app.MapPost("/message", () =>
{
    var messagingResponse = new MessagingResponse();
    messagingResponse.Message("Ahoy!");
    return Results.Extensions.TwiML(messagingResponse);
});

await app.StartAsync();

var devTunnelUrl = Environment.GetEnvironmentVariable("VS_TUNNEL_URL");
if (devTunnelUrl != null)
{
    using var serviceScope = app.Services.CreateScope();
    var twilioClient = serviceScope.ServiceProvider.GetRequiredService<ITwilioRestClient>();
    var phoneNumber = app.Configuration["Twilio:PhoneNumber"];
    if (string.IsNullOrEmpty(phoneNumber)) throw new Exception("'Twilio:PhoneNumber' not configured");

    var phoneNumberResources = await IncomingPhoneNumberResource.ReadAsync(
        phoneNumber: new PhoneNumber(phoneNumber),
        client: twilioClient
    );
    var phoneNumberResource = phoneNumberResources.First();

    var smsUrl = $"{devTunnelUrl}message";
    await IncomingPhoneNumberResource.UpdateAsync(
        pathSid: phoneNumberResource.Sid,
        smsUrl: new Uri(smsUrl),
        smsMethod: Twilio.Http.HttpMethod.Post,
        client: twilioClient
    );

    app.Logger.LogInformation("Updated {TwilioPhoneNumber} SMS URL to {SmsUrl}", phoneNumber, smsUrl);
}

await app.WaitForShutdownAsync();