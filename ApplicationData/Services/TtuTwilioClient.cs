using Core.AppSettings;
using Twilio.Clients;
using Twilio.Http;
using HttpClient = System.Net.Http.HttpClient;

namespace AdminData.Services;

public class TtuTwilioClient : ITwilioRestClient
{
	private readonly ITwilioRestClient _client;

	public TtuTwilioClient(
		TwilioSettings twilioSettings,
		HttpClient httpClient
	)
	{
		_client = new TwilioRestClient(
			twilioSettings.AccountSid,
			twilioSettings.AuthToken,
			httpClient: new SystemNetHttpClient(httpClient)
		);
	}

	public Response Request(Request request) => _client.Request(request);
	public Task<Response> RequestAsync(Request request) => _client.RequestAsync(request);
	public string AccountSid => _client.AccountSid;
	public string Region => _client.Region;
	public Twilio.Http.HttpClient HttpClient => _client.HttpClient;
}