using Blazored.LocalStorage;

namespace Frontend.Data;

public class ClientSettingsProvider
{
	private readonly ILocalStorageService _localStorage;

	public ClientSettingsProvider(
		ILocalStorageService localStorage
	)
	{
		_localStorage = localStorage;
	}

	public delegate void SlideshowIntervalSettingChanged(SlideshowIntervalSettingChangedEventArgs args);

	public event SlideshowIntervalSettingChanged? OnSlideshowIntervalSettingChanged;

	public record SlideshowIntervalSettingChangedEventArgs(TimeSpan NewInterval);

	public async Task<TimeSpan?> GetSlideshowIntervalAsync()
	{
		var result = await _localStorage.GetItemAsync<int?>("slideshowIntervalMilliseconds");

		if (result is null)
			return null;

		return TimeSpan.FromMilliseconds(result.Value);
	}

	public async Task SetSlideshowIntervalAsync(TimeSpan span)
	{
		await _localStorage.SetItemAsync("slideshowIntervalMilliseconds", span.TotalMilliseconds);

		OnSlideshowIntervalSettingChanged?.Invoke(new SlideshowIntervalSettingChangedEventArgs(span));
	}
}