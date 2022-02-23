namespace Core.Interfaces

open System
open System.Collections.Generic
open System.Threading.Tasks

type RawSubmissionStatus =
    | Pending = 1
    | Approved = 2
    | Discarded = 3

type SubmissionStatus =
    | Pending
    | Approved
    | Discarded

type PendingSubmission = {
    SubmissionId : int
    PhoneNumber : string
    LastSubmissionAt : DateTime option
    SubmittedAt : DateTime
}

type ApprovedSubmission = {
    SubmissionId : int
    PhoneNumber : string
    LastSubmissionAt : DateTime option
    ApprovedAt : DateTime
    SubmittedAt : DateTime
}

type GetSubmissionImageResponse = {
    SubmissionStatus : SubmissionStatus
    RawImage : byte[]
    MimeType : string
}

type ISubmissionQueue =
    abstract member GetAllPendingSubmissionsAsync : unit -> Task<IReadOnlyCollection<PendingSubmission>>
    abstract member GetPendingSubmissionsByIdAsync : submissionIds : IReadOnlyCollection<int> -> Task<IReadOnlyCollection<PendingSubmission>>
    abstract member GetSubmissionImageAsync : submissionId : int -> Task<GetSubmissionImageResponse option>
    abstract member GetAllApprovedSubmissionsAsync : unit -> Task<IReadOnlyCollection<ApprovedSubmission>>
    abstract member GetApprovedSubmissionByIdAsync : submissionId : int -> Task<ApprovedSubmission option>
    abstract member AddNewImageSubmissionAsync :
        phoneNumber : string ->
        submittedAt : DateTime ->
        image : byte[] ->
        mimeType : string ->
        Task<int>
    abstract member ApproveSubmissionAsync : submissionId : int -> Task<bool>
    abstract member DiscardSubmissionAsync : submissionId : int -> Task<bool>

type IAccessProvider =
    abstract member IsPhoneNumberBlockedAsync : phoneNumber : string -> Task<bool>

type ISubmissionHub =
    abstract member SendSubmissionApprovedAsync : submission : ApprovedSubmission -> Task
    abstract member SendSubmissionDiscardedAsync : submissionId : int -> Task
    abstract member SendNewSubmissionsAsync : submissionIds : IReadOnlyCollection<PendingSubmission> -> Task
