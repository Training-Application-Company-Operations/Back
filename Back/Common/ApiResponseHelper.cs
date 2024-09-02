﻿using Back.Enums;
using Back.Extensions;
using Back.Models;
using Microsoft.AspNetCore.Mvc;

namespace Back.Common
{
    public static class ApiResponseHelper
    {
        public static ActionResult BadRequestMissingPayload()
        {
            return new BadRequestObjectResult(new ErrorResponse(ErrorCodes.PayloadIsMissing, ErrorCodes.PayloadIsMissing.GetDescription()));
        }

        public static ActionResult BadRequestDecryptionFailed()
        {
            return new BadRequestObjectResult(new ErrorResponse(ErrorCodes.PayloadDecryptionFailed, ErrorCodes.PayloadDecryptionFailed.GetDescription()));
        }

        public static ActionResult BadRequestInvalidData()
        {
            return new BadRequestObjectResult(new ErrorResponse(ErrorCodes.InvalidDecryptedData, ErrorCodes.InvalidDecryptedData.GetDescription()));
        }
    }
}