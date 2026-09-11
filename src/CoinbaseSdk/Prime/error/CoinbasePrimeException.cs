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

namespace CoinbaseSdk.Prime.Error
{
  using System.Net;
  using CoinbaseSdk.Core.Error;
  using CoinbaseSdk.Core.Serialization;

  /// <summary>
  /// Exception thrown when a Prime API request returns a non-success status with an error body.
  /// </summary>
  public class CoinbasePrimeException : CoinbaseException
  {
    public CoinbasePrimeException(HttpStatusCode statusCode, PrimeErrorResponse body)
      : base(statusCode, ResolveMessage(statusCode, body))
    {
      this.Body = body ?? new PrimeErrorResponse();
    }

    public PrimeErrorResponse Body { get; }

    public string? Code => this.Body.Code;

    public string? Subcode => this.Body.Subcode;

    public string? TraceId => this.Body.TraceId;

    public static CoinbasePrimeException FromStatus(HttpStatusCode statusCode, PrimeErrorResponse? body)
    {
      var resolved = body ?? new PrimeErrorResponse();
      return statusCode switch
      {
        HttpStatusCode.BadRequest => new PrimeBadRequestException(resolved),
        HttpStatusCode.Unauthorized => new PrimeUnauthorizedException(resolved),
        HttpStatusCode.Forbidden => new PrimeForbiddenException(resolved),
        HttpStatusCode.NotFound => new PrimeNotFoundException(resolved),
        HttpStatusCode.TooManyRequests => new PrimeTooManyRequestsException(resolved),
        HttpStatusCode.InternalServerError => new PrimeInternalServerException(resolved),
        HttpStatusCode.NotImplemented => new PrimeNotImplementedException(resolved),
        HttpStatusCode.ServiceUnavailable => new PrimeServiceUnavailableException(resolved),
        _ => new CoinbasePrimeException(statusCode, resolved),
      };
    }

    public static CoinbasePrimeException FromResponse(
      HttpStatusCode statusCode,
      string? content,
      IJsonUtility jsonUtility)
    {
      if (string.IsNullOrWhiteSpace(content))
      {
        return FromStatus(statusCode, new PrimeErrorResponse());
      }

      try
      {
        var body = jsonUtility.Deserialize<PrimeErrorResponse>(content);
        return FromStatus(statusCode, body);
      }
      catch (Exception)
      {
        return FromStatus(statusCode, new PrimeErrorResponse { Message = content });
      }
    }

    public override string ToString()
    {
      return
        $"CoinbasePrimeException{{StatusCode={this.StatusCode}, Code={this.Code}, Subcode={this.Subcode}, TraceId={this.TraceId}, Message={this.Message}}}";
    }

    private static string ResolveMessage(HttpStatusCode statusCode, PrimeErrorResponse? body)
    {
      if (!string.IsNullOrWhiteSpace(body?.Message))
      {
        return body.Message;
      }

      return $"Request failed with status {(int)statusCode}";
    }
  }
}
