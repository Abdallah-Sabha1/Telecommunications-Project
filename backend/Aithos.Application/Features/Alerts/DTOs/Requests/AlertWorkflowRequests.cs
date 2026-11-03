namespace Aithos.Application.Contracts.Requests;

public sealed record AlertWorkflowRequest(string Status);

public sealed record AlertAssigneeRequest(string AssignedTo);
