using CommonCore.Base.Extensions;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubmissionController
{
	private readonly ISubmissionQueue _submissionQueue;

	public SubmissionController(
		ISubmissionQueue submissionQueue
	)
	{
		_submissionQueue = submissionQueue;
	}

	[Route("Image/{SubmissionId:int}")]
	[ResponseCache(Duration = 60 * 60 * 24)]
	public async Task<IActionResult> GetImage(int submissionId)
	{
		var maybeSubmission = (await _submissionQueue.GetSubmissionImageAsync(submissionId)).ToMaybe();

		if (!maybeSubmission.Try(out var submission) || submission.SubmissionStatus.IsDiscarded)
			return new NotFoundResult();

		return new FileContentResult(submission.RawImage, submission.MimeType);
	}
}