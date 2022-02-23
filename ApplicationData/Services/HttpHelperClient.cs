namespace AdminData.Services;

public class HttpHelperClient
{
	public HttpHelperClient(
		HttpClient httpClient
	)
	{
		Client = httpClient;
	}

	public readonly HttpClient Client;
}