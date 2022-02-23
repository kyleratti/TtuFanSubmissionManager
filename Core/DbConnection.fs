namespace Core.DbConnection

open System
open System.Data
open System.Data.Common
open System.Threading.Tasks
open Core.AppSettings
open Npgsql

module private DbConnection_impl =
    let private toQueryParams (queryParams : (string * obj) seq) =
        queryParams
        |> Seq.map NpgsqlParameter
        |> Array.ofSeq

    let private createCommandWithParamsAsync (db : NpgsqlConnection) (sql : string) (queryParams : NpgsqlParameter seq) =
        let cmd = new NpgsqlCommand (sql, db)
        cmd.Parameters.AddRange (queryParams |> Array.ofSeq)
        task {
            let! _ = cmd.PrepareAsync ()
            return cmd
        }

    let private createCommandAsync (db : NpgsqlConnection) (sql : string) (queryParams : (string * obj) seq) =
        queryParams
        |> Seq.map NpgsqlParameter
        |> createCommandWithParamsAsync db sql

    let connectToDbIfNotConnectedAsync (db : NpgsqlConnection) =
        match db.State with
        | ConnectionState.Closed ->
            task { return! db.OpenAsync () }
        | _ -> task { return! Task.CompletedTask }

    let executeSqlNonQueryAsync (db : NpgsqlConnection) (sql : string) (queryParams : NpgsqlParameter seq) =
        task {
            let! _ = db |> connectToDbIfNotConnectedAsync
            use! cmd = (sql, queryParams) ||> createCommandWithParamsAsync db
            return! cmd.ExecuteNonQueryAsync ()
        }

    let executeScalarAsync (db : NpgsqlConnection) (sql : string) (queryParams : NpgsqlParameter seq) =
        task {
            let! _ = db |> connectToDbIfNotConnectedAsync
            use! cmd = (sql, queryParams) ||> createCommandWithParamsAsync db
            return! cmd.ExecuteScalarAsync ()
        }

    let executeReaderAsync (db : NpgsqlConnection) (sql : string) (queryParams : NpgsqlParameter seq) =
        task {
            let! _ = db |> connectToDbIfNotConnectedAsync
            use! cmd = (sql, queryParams) ||> createCommandWithParamsAsync db
            return! cmd.ExecuteReaderAsync ()
        }

type DbConnection (settings : DbSettings) =
    let connectionString =
        let builder = NpgsqlConnectionStringBuilder ()
        builder.Host <- settings.HostName
        builder.Database <- settings.DatabaseName
        builder.Username <- settings.Username
        builder.Password <- settings.Password
        builder.ToString ()

    let db = new NpgsqlConnection (connectionString)

    member _.ExecuteSqlNonQueryAsync (sql : string, [<ParamArray>] queryParams : NpgsqlParameter seq) =
        (sql, queryParams)
        ||> DbConnection_impl.executeSqlNonQueryAsync db

    member _.ExecuteScalarAsync (sql : string, [<ParamArray>] queryParams : NpgsqlParameter seq) =
        (sql, queryParams)
        ||> DbConnection_impl.executeScalarAsync db

    member _.ExecuteReaderAsync (sql : string, [<ParamArray>] queryParams : NpgsqlParameter seq) : Task<DbDataReader> =
        (sql, queryParams)
        ||> DbConnection_impl.executeReaderAsync db

    interface IDisposable with
        member _.Dispose () =
            db.Dispose ()
