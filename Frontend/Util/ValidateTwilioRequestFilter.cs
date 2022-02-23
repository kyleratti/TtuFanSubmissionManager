using System.Net;
using Core.AppSettings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Twilio.Security;

namespace Frontend.Util;

[AttributeUsage(AttributeTargets.Method)]
public class ValidateTwilioRequestFilter : ActionFilterAttribute
{
	private readonly TwilioSettings _twilioSettings;

	public ValidateTwilioRequestFilter(
		TwilioSettings twilioSettings
	)
	{
		_twilioSettings = twilioSettings;
	}

	public override void OnActionExecuting(ActionExecutingContext actionContext)
	{
		var context = actionContext.HttpContext;
		if (!IsValidRequest(CreateRequestValidator(_twilioSettings.AuthToken), context.Request))
		{
			actionContext.Result = new StatusCodeResult((int)HttpStatusCode.Forbidden);
		}

		base.OnActionExecuting(actionContext);
	}

	private static RequestValidator CreateRequestValidator(string twilioAuthToken) => new(twilioAuthToken);

	private static bool IsValidRequest(RequestValidator requestValidator, HttpRequest request) =>
		requestValidator.Validate(
			url: $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}",
			parameters: request.Form.ToDictionary(x => x.Key, x => x.Value.ToString()),
			expected: request.Headers["X-Twilio-Signature"]
		);
}