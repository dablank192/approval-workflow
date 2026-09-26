using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApprovalWorkflow.Feature.Workflow;

[ApiController]
[Authorize]
[Route("api/v1/workflow")]
public class WorkflowApi : ControllerBase
{

}
