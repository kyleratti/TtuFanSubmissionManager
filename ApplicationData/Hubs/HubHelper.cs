using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace AdminData.Hubs;

public class HubHelper
{
	private readonly NavigationManager _navManager;

	public HubHelper(
		NavigationManager navManager
	)
	{
		_navManager = navManager;
	}

	public HubConnection SubmissionHub =>
		new HubConnectionBuilder()
			.WithUrl(_navManager.ToAbsoluteUri("/hubs/submissions"))
			.Build();
}