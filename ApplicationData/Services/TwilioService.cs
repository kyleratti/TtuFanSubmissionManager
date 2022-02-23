using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;

namespace AdminData.Services;

public class TwilioService
{
	private readonly ITwilioRestClient _client;

	public TwilioService(
		ITwilioRestClient twilioClient
	)
	{
		_client = twilioClient;
	}

	public async Task<IReadOnlyCollection<IncomingPhoneNumberResource>> GetAllIncomingPhoneNumbersAsync(int limit) =>
		(await IncomingPhoneNumberResource.ReadAsync(limit: limit, client: _client))
		.ToArray();
}