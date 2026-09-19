using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApprovalWorkflow.Feature.Workspace;

[ApiController]
[Route("api/v1/workspace")]
[Authorize]
public class WorkspaceApi : ControllerBase
{

}
