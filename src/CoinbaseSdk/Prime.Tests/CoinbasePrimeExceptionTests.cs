/*
 * Copyright 2026-present Coinbase Global, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Net;
using CoinbaseSdk.Prime.Error;
using CoinbaseSdk.Prime.Serialization;
using Xunit;

namespace CoinbaseSdk.Prime.Tests;

public class CoinbasePrimeExceptionTests
{
  [Fact]
  public void FromStatus_BadRequest_ReturnsSubclassWithBodyFields()
  {
    var body = new PrimeErrorResponse
    {
      Code = "VALIDATION_ERROR",
      Message = "invalid order",
      Subcode = "ORDER_SIZE_INVALID",
      TraceId = "trace-1",
    };

    var ex = CoinbasePrimeException.FromStatus(HttpStatusCode.BadRequest, body);

    var typed = Assert.IsType<PrimeBadRequestException>(ex);
    Assert.Equal(HttpStatusCode.BadRequest, typed.StatusCode);
    Assert.Equal("VALIDATION_ERROR", typed.Code);
    Assert.Equal("ORDER_SIZE_INVALID", typed.Subcode);
    Assert.Equal("trace-1", typed.TraceId);
    Assert.Equal("invalid order", typed.Message);
  }

  [Theory]
  [InlineData(HttpStatusCode.Unauthorized, typeof(PrimeUnauthorizedException))]
  [InlineData(HttpStatusCode.Forbidden, typeof(PrimeForbiddenException))]
  [InlineData(HttpStatusCode.NotFound, typeof(PrimeNotFoundException))]
  [InlineData(HttpStatusCode.TooManyRequests, typeof(PrimeTooManyRequestsException))]
  [InlineData(HttpStatusCode.InternalServerError, typeof(PrimeInternalServerException))]
  [InlineData(HttpStatusCode.NotImplemented, typeof(PrimeNotImplementedException))]
  [InlineData(HttpStatusCode.ServiceUnavailable, typeof(PrimeServiceUnavailableException))]
  public void FromStatus_KnownStatuses_ReturnExpectedSubclass(HttpStatusCode status, Type expectedType)
  {
    var ex = CoinbasePrimeException.FromStatus(status, new PrimeErrorResponse { Message = "err" });
    Assert.IsType(expectedType, ex);
    Assert.Equal(status, ex.StatusCode);
  }

  [Fact]
  public void FromStatus_UnknownStatus_ReturnsBaseException()
  {
    var ex = CoinbasePrimeException.FromStatus((HttpStatusCode)418, new PrimeErrorResponse { Message = "teapot" });
    Assert.IsType<CoinbasePrimeException>(ex);
    Assert.False(ex is PrimeBadRequestException);
    Assert.Equal((HttpStatusCode)418, ex.StatusCode);
  }

  [Fact]
  public void FromResponse_ParsesSnakeCaseJson()
  {
    const string json = """
      {"code":"RESOURCE_NOT_FOUND","message":"missing","subcode":"AUTH_RESOURCE_NOT_FOUND","trace_id":"trace-2"}
      """;

    var ex = CoinbasePrimeException.FromResponse(
      HttpStatusCode.NotFound,
      json,
      PrimeJsonDefaults.JsonUtility);

    var typed = Assert.IsType<PrimeNotFoundException>(ex);
    Assert.Equal("RESOURCE_NOT_FOUND", typed.Code);
    Assert.Equal("AUTH_RESOURCE_NOT_FOUND", typed.Subcode);
    Assert.Equal("trace-2", typed.TraceId);
    Assert.Equal("missing", typed.Message);
  }

  [Fact]
  public void FromResponse_MalformedBody_PreservesStatusAndRawContent()
  {
    const string content = "not-json";

    var ex = CoinbasePrimeException.FromResponse(
      HttpStatusCode.BadRequest,
      content,
      PrimeJsonDefaults.JsonUtility);

    var typed = Assert.IsType<PrimeBadRequestException>(ex);
    Assert.Equal(HttpStatusCode.BadRequest, typed.StatusCode);
    Assert.Equal("not-json", typed.Message);
    Assert.Null(typed.Code);
    Assert.Null(typed.Subcode);
  }
}
