// Copyright (C) TBC Bank. All Rights Reserved.

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Discounts.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
    }
}
