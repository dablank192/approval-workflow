using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApprovalWorkflow.Feature.Request;

[ApiController]
[Route("api/v1/request")]
[Authorize]
public class RequestApi : ControllerBase
{

}
