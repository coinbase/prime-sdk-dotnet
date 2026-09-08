#!/usr/bin/env -S dotnet run --file
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

#:project ../../../Prime
#:project ../../
#:package Newtonsoft.Json@13.0.3

using System.CommandLine;
using CoinbaseSdk.Prime.ApiKeyManagement;
using CoinbaseSdk.Prime.Client;
using CoinbaseSdk.Prime.Common;

// Load environment variables
DotNetEnv.Env.TraversePath().Load();

var rootCommand = new RootCommand("Rotate A P I Key")
{
};

rootCommand.SetHandler(() =>
{
    try
    {

        var client = CoinbasePrimeClient.FromEnv();
        var apiKeyManagementService = new ApiKeyManagementService(client);

        var request = new RotateAPIKeyRequest.RotateAPIKeyRequestBuilder()
            .Build();

        PrettyPrinter.PrintResponse("RotateAPIKeyRequest", request);

        var response = apiKeyManagementService.RotateAPIKey(request);

        PrettyPrinter.PrintResponse("RotateAPIKeyResponse", response);

        Environment.ExitCode = 0;
    }
    catch (Exception ex)
    {
        PrettyPrinter.PrintError("Error calling RotateAPIKey", ex);
        Environment.ExitCode = 1;
    }
});

return rootCommand.Invoke(args);
