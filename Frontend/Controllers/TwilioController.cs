using AdminData.Hubs;
using AdminData.Services;
using Core.Interfaces;
using Frontend.Models;
using Frontend.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;

namespace Frontend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TwilioController
{
	private readonly ISubmissionQueue _submissionQueue;
	private readonly HttpHelperClient _httpHelper;
	private readonly IHubContext<SubmissionHub, ISubmissionHub> _submissionHub;
	private readonly ITwilioRestClient _twilioRestClient;

	public TwilioController(
		ISubmissionQueue submissionQueue,
		HttpHelperClient httpHelper,
		IHubContext<SubmissionHub, ISubmissionHub> submissionHub,
		ITwilioRestClient twilioRestClient
	)
	{
		_submissionQueue = submissionQueue;
		_httpHelper = httpHelper;
		_submissionHub = submissionHub;
		_twilioRestClient = twilioRestClient;
	}

	[HttpGet("")]
	public string Index()
	{
		return "testing";
	}

	[HttpPost("ReceiveMms")]
	[ServiceFilter(typeof(ValidateTwilioRequestFilter))]
	public async Task<IActionResult> ReceiveMmsAsync([FromForm] IFormCollection data)
	{
		// If this isn't a valid request, still return Accepted 202.
		// Otherwise, Twilio will start logging errors.
		// TODO: log these somewhere useful probably
		if (!IsValidMmsRequest(data))
			return new AcceptedResult();

		var parsedAttachments = ParseAttachedFiles(data);
		var invalidAttachments = parsedAttachments
			.Where(x => !AllowedMimeTypes.Contains(x.MimeType))
			.ToArray();
		var validAttachments = parsedAttachments
			.Except(invalidAttachments)
			.ToArray();

		if (invalidAttachments.Any())
			throw new InvalidOperationException(
				$"Unsupported attachment types: {string.Join(", ", invalidAttachments.Select(x => x.MimeType).Distinct())}");

		if (!validAttachments.Any())
			throw new InvalidOperationException("No attachments on message");

		var fullAttachments = await Task.WhenAll(validAttachments.Select(async x =>
			(InboundAttachment: x, File: await DownloadFile(x.Url))
		));

		var now = DateTime.UtcNow;
		var newSubmissionIds = new List<int>();

		foreach (var (attachment, file) in fullAttachments)
			newSubmissionIds.Add(await _submissionQueue.AddNewImageSubmissionAsync(
				attachment.PhoneNumber,
				now,
				file,
				attachment.MimeType
			));

		var newSubmissions = await _submissionQueue.GetPendingSubmissionsByIdAsync(newSubmissionIds);

		await _submissionHub.Clients.All.SendNewSubmissionsAsync(newSubmissions);

		_ = Task.Run(async () =>
		{
			await Task.Delay(TimeSpan.FromSeconds(30));

			foreach (var attachment in validAttachments)
				await MessageResource.DeleteAsync(attachment.SmsSid, client: _twilioRestClient);
		});

		return new OkResult();
	}

	private static readonly IReadOnlySet<string> AllowedMimeTypes = new HashSet<string>(new[]
	{
		"image/jpeg",
		"image/png"
	});

	private static IReadOnlyCollection<InboundMmsAttachment> ParseAttachedFiles(IFormCollection data) =>
		Enumerable.Range(0, Convert.ToInt32(data["NumMedia"]))
			.Select(i => new InboundMmsAttachment(
				Url: data[$"MediaUrl{i}"],
				MimeType: data[$"MediaContentType{i}"],
				PhoneNumber: data["From"],
				SmsSid: data["SmsSid"]
			))
			.ToArray();

	private static readonly string[] SmsBodyFields =
	{
		"ToCountry",
		"MediaContentType0",
		"ToState",
		"SmsMessageSid",
		"NumMedia",
		"ToCity",
		"FromZip",
		"SmsSid",
		"FromState",
		"SmsStatus",
		"FromCity",
		"Body",
		"FromCountry",
		"To",
		"ToZip",
		"NumSegments",
		"MessageSid",
		"AccountSid",
		"From",
		"MediaUrl0",
		"ApiVersion"
	};

	private static bool IsValidSmsRequest(IFormCollection data) =>
		SmsBodyFields.All(data.ContainsKey);

	private static bool IsValidMmsRequest(IFormCollection data)
	{
		if (!IsValidSmsRequest(data))
			return false;

		// The only way, so far as I can tell, to determine if the form data is from an MMS request is to check if media is attached.
		// Therefore, if _at least_ "MediaUrl0" and "MediaContentType0" exist on the data, it's MMS.

		return data.ContainsKey("MediaUrl0") && data.ContainsKey("MediaContentType0");
	}

	private async Task<byte[]> DownloadFile(string url)
	{
		var result = await _httpHelper.Client.GetAsync(url);

		if (!result.IsSuccessStatusCode)
			throw new Exception(
				$"HTTP request failed (HTTP {(int)result.StatusCode}: {await result.Content.ReadAsStringAsync()}");

		return await result.Content.ReadAsByteArrayAsync();
	}
}