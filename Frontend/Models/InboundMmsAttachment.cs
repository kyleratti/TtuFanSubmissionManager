namespace Frontend.Models;

/// <summary>
/// Inbound MMS Message
/// </summary>
/// <param name="Url">The publicly accessible URL of the media object.</param>
/// <param name="MimeType">The mime type of the media object.</param>
/// <param name="PhoneNumber">The phone number that submitted the media object.</param>
/// <param name="SmsSid"></param>
public record InboundMmsAttachment(
	string Url,
	string MimeType,
	string PhoneNumber,
	string SmsSid
);