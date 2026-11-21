using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApprovalWorkflow.Feature.User;

[ApiController]
[Route("api/v1/user")]
[Authorize]
public class UserApi : ControllerBase
{

}
