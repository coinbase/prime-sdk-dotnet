/*
 * Copyright 2026-present Coinbase Global, Inc.
 *
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *  http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

namespace CoinbaseSdk.Prime.ApiKeyManagement
{
  using System.Net;
  using CoinbaseSdk.Core.Client;
  using CoinbaseSdk.Core.Http;
  using CoinbaseSdk.Core.Service;

  public class ApiKeyManagementService(ICoinbaseClient client) : CoinbaseService(client), IApiKeyManagementService
  {
    /// <summary>
    /// Rotate API Key.
    /// </summary>
    public RotateAPIKeyResponse RotateAPIKey(
      RotateAPIKeyRequest request,
      CallOptions? options = null)
    {
      return Request<RotateAPIKeyResponse>(
        HttpMethod.Post,
        $"/api-keys/rotate",
        [HttpStatusCode.OK],
        request,
        options);
    }

    public Task<RotateAPIKeyResponse> RotateAPIKeyAsync(
      RotateAPIKeyRequest request,
      CallOptions? options = null,
      CancellationToken cancellationToken = default)
    {
      return RequestAsync<RotateAPIKeyResponse>(
        HttpMethod.Post,
        $"/api-keys/rotate",
        [HttpStatusCode.OK],
        request,
        options,
        cancellationToken);
    }
  }
}
