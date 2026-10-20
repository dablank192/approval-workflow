using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApprovalWorkflow.Feature.Approval;

[ApiController]
[Route("api/v1/approval")]
[Authorize]
public class ApprovalApi : ControllerBase
{

}
