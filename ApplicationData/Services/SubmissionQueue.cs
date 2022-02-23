using Core.DbConnection;
using Core.Interfaces;
using Microsoft.FSharp.Core;
using Npgsql;

namespace AdminData.Services;

public class SubmissionQueue : ISubmissionQueue
{
	private readonly DbConnection _dbConnection;

	public SubmissionQueue(
		DbConnection dbConnection
	)
	{
		_dbConnection = dbConnection;
	}

	/// <inheritdoc />
	public async Task<IReadOnlyCollection<PendingSubmission>> GetAllPendingSubmissionsAsync()
	{
		await using var reader =
			await _dbConnection.ExecuteReaderAsync(
				@"SELECT
						submission_id, phone_number, submitted_at
					FROM app.submissions
					WHERE
						status = @status
					ORDER BY submitted_at ASC",
				new NpgsqlParameter[] { new("@status", (int)RawSubmissionStatus.Pending) }
			);

		var results = new List<PendingSubmission>();

		while (await reader.ReadAsync())
		{
			var submissionId = reader.GetInt32(0);
			var phoneNumber = reader.GetString(1);
			var submittedAt = reader.GetDateTime(2);

			results.Add(new PendingSubmission(
				submissionId,
				phoneNumber,
				FSharpOption<DateTime>.None,
				submittedAt
			));
		}

		return results;
	}

	public async Task<IReadOnlyCollection<PendingSubmission>> GetPendingSubmissionsByIdAsync(IReadOnlyCollection<int> submissionIds)
	{
		var ids =
			submissionIds
				.Select((x, i) => (Sql: $"@submissionId{i}", QueryParam: new NpgsqlParameter($"@submissionId{i}", x)))
				.ToArray();

		await using var reader = await _dbConnection.ExecuteReaderAsync(
			$@"SELECT submission_id, phone_number, submitted_at
				FROM app.submissions
				WHERE submission_id IN ({string.Join(", ", ids.Select(x => x.Sql))})",
			ids.Select(x => x.QueryParam)
		);

		var results = new List<PendingSubmission>();

		while (await reader.ReadAsync())
		{
			var submissionId = reader.GetInt32(0);
			var phoneNumber = reader.GetString(1);
			var submittedAt = reader.GetDateTime(2);

			results.Add(new PendingSubmission(
				submissionId,
				phoneNumber,
				FSharpOption<DateTime>.None,
				submittedAt
			));
		}

		return results;
	}

	/// <inheritdoc />
	public async Task<IReadOnlyCollection<ApprovedSubmission>> GetAllApprovedSubmissionsAsync()
	{
		await using var reader =
			await _dbConnection.ExecuteReaderAsync(
				@"SELECT
						submission_id, phone_number, submitted_at, approved_at
					FROM app.submissions
					WHERE
						status = @status
					ORDER BY approved_at ASC",
				new NpgsqlParameter[] { new("@status", (int)RawSubmissionStatus.Approved) }
			);

		var results = new List<ApprovedSubmission>();

		while (await reader.ReadAsync())
		{
			var submissionId = reader.GetInt32(0);
			var phoneNumber = reader.GetString(1);
			var submittedAt = reader.GetDateTime(2);
			var approvedAt = reader.GetDateTime(3);

			results.Add(new ApprovedSubmission(
				submissionId,
				phoneNumber,
				FSharpOption<DateTime>.None,
				approvedAt,
				submittedAt
			));
		}

		return results;
	}

	public async Task<FSharpOption<ApprovedSubmission>> GetApprovedSubmissionByIdAsync(int submissionId)
	{
		await using var reader =
			await _dbConnection.ExecuteReaderAsync(
				@"SELECT phone_number, submitted_at, approved_at
					FROM app.submissions
					WHERE status = @status
					ORDER BY approved_at ASC",
				new NpgsqlParameter[] { new("@status", (int)RawSubmissionStatus.Approved) }
			);

		if(!await reader.ReadAsync())
			return FSharpOption<ApprovedSubmission>.None;

		var phoneNumber = reader.GetString(0);
		var submittedAt = reader.GetDateTime(1);
		var approvedAt = reader.GetDateTime(2);

		return new ApprovedSubmission(
			submissionId,
			phoneNumber,
			FSharpOption<DateTime>.None,
			approvedAt,
			submittedAt
		);
	}

	/// <inheritdoc />
	public async Task<FSharpOption<GetSubmissionImageResponse>> GetSubmissionImageAsync(int submissionId)
	{
		await using var reader =
			await _dbConnection.ExecuteReaderAsync(
				@"SELECT
						length(image) AS file_size,
						image,
						image_mimetype,
       					status
					FROM app.submissions
					WHERE submission_id = @submissionId",
				new NpgsqlParameter[] { new("@submissionId", submissionId) }
			);

		if (!await reader.ReadAsync())
			return FSharpOption<GetSubmissionImageResponse>.None;

		var size = reader.GetInt32(0);

		var buffer = new byte[size];
		reader.GetBytes(1, 0, buffer, 0, size);

		var mimeType = reader.GetString(2);
		var rawStatus = reader.GetInt16(3);
		var status = Enum.Parse<RawSubmissionStatus>(rawStatus.ToString()) switch
		{
			RawSubmissionStatus.Pending => SubmissionStatus.Pending,
			RawSubmissionStatus.Approved => SubmissionStatus.Approved,
			RawSubmissionStatus.Discarded => SubmissionStatus.Discarded,
			_ => throw new ArgumentOutOfRangeException("status", rawStatus, "The submission status is not supported")
		};

		return new GetSubmissionImageResponse(status, buffer, mimeType);
	}

	/// <inheritdoc />
	public async Task<int> AddNewImageSubmissionAsync(string phoneNumber, DateTime submittedAt, byte[] image, string mimeType)
	{
		var result = await _dbConnection.ExecuteScalarAsync(
			@"INSERT INTO app.submissions (phone_number, submitted_at, status, image, image_mimetype)
				VALUES(@phoneNumber, @submittedAt, @status, @image, @mimeType)
				RETURNING submission_id",
			new NpgsqlParameter[]
			{
				new("@phoneNumber", phoneNumber),
				new("@submittedAt", submittedAt),
				new("@status", (int)RawSubmissionStatus.Pending),
				new("@image", image),
				new("@mimeType", mimeType)
			});

		return (int)result;
	}

	/// <inheritdoc />
	public async Task<bool> ApproveSubmissionAsync(int submissionId)
	{
		var result = await _dbConnection.ExecuteSqlNonQueryAsync(
			@"UPDATE app.submissions SET status = @newStatus, approved_at = NOW() WHERE submission_id = @submissionId",
			new NpgsqlParameter[]
			{
				new("@newStatus", (int)RawSubmissionStatus.Approved),
				new("@submissionId", submissionId)
			}
		);

		return result == 1;
	}

	/// <inheritdoc />
	public async Task<bool> DiscardSubmissionAsync(int submissionId)
	{
		var result = await _dbConnection.ExecuteSqlNonQueryAsync(
			@"UPDATE app.submissions SET status = @newStatus, discarded_at = NOW() WHERE submission_id = @submissionId",
			new NpgsqlParameter[]
			{
				new("@newStatus", (int)RawSubmissionStatus.Discarded),
				new("@submissionId", submissionId)
			}
		);

		return result == 1;
	}
}