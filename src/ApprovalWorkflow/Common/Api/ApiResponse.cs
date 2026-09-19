using System;

namespace ApprovalWorkflow.Common.Api;

public record ApiResponse<T>(
    T? Data,
    string? Message
)
{
    public static ApiResponse<T> Success(
        T? data,
        string? message
    )
    {
        return new ApiResponse<T>(
            Data: data,
            Message: message
        );
    }
}
