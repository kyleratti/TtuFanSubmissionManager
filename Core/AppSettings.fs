namespace Core.AppSettings

open Microsoft.Extensions.Options

[<CLIMutable>]
type RawDbSettings = {
    HostName : string
    DatabaseName : string
    Username : string
    Password : string
}

[<CLIMutable>]
type RawTwilioSettings = {
    AccountSid : string
    AuthToken : string
}

[<CLIMutable>]
type RawAppSettings = {
    SubmissionPhoneNumber : string
}

type DbSettings (config : IOptions<RawDbSettings>) =
    member _.HostName = config.Value.HostName
    member _.DatabaseName = config.Value.DatabaseName
    member _.Username = config.Value.Username
    member _.Password = config.Value.Password

type TwilioSettings (config : IOptions<RawTwilioSettings>) =
    member _.AccountSid = config.Value.AccountSid
    member _.AuthToken = config.Value.AuthToken