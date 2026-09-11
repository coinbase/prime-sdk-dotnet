#!/usr/bin/env -S dotnet run --file
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

#:project ../../../Prime
#:project ../../
#:package Newtonsoft.Json@13.0.3

using System.CommandLine;
using CoinbaseSdk.Prime.Client;
using CoinbaseSdk.Prime.Error;
using CoinbaseSdk.Prime.Orders;

// Catch typed Prime API errors and inspect code / subcode / trace_id.
//
// Required env vars: PRIME_ACCESS_KEY, PRIME_PASSPHRASE, PRIME_SIGNING_KEY
// Optional: PRIME_PORTFOLIO_ID
//
// Examples:
//   dotnet run --file src/CoinbaseSdk/PrimeExample/examples/errors/HandleApiErrors.cs -- --orderId 00000000-0000-0000-0000-000000000000
//   dotnet run --file src/CoinbaseSdk/PrimeExample/examples/errors/HandleApiErrors.cs -- --demo-validation

DotNetEnv.Env.TraversePath().Load();

const string MissingOrderId = "00000000-0000-0000-0000-000000000000";

var portfolioIdOption = new Option<string?>(
    name: "--portfolioId",
    description: "The Portfolio ID");
var orderIdOption = new Option<string?>(
    name: "--orderId",
    description: "Order ID to look up (default: a UUID that should 404)");
var demoValidationOption = new Option<bool>(
    name: "--demo-validation",
    description: "Also submit an invalid create_order request to show HTTP 400 handling");

var rootCommand = new RootCommand("Demonstrate catching typed Prime API errors")
{
    portfolioIdOption,
    orderIdOption,
    demoValidationOption,
};

rootCommand.SetHandler((context) =>
{
    var portfolioId = context.ParseResult.GetValueForOption(portfolioIdOption)
        ?? Environment.GetEnvironmentVariable("PRIME_PORTFOLIO_ID");
    var orderId = context.ParseResult.GetValueForOption(orderIdOption) ?? MissingOrderId;
    var demoValidation = context.ParseResult.GetValueForOption(demoValidationOption);

    if (string.IsNullOrEmpty(portfolioId))
    {
        Console.Error.WriteLine("Error: --portfolioId is required (or set PRIME_PORTFOLIO_ID env var).");
        Environment.ExitCode = 1;
        return;
    }

    var client = CoinbasePrimeClient.FromEnv();
    var ordersService = new OrdersService(client);

    Console.WriteLine($"GET order {orderId}");
    try
    {
        var response = ordersService.GetOrder(new GetOrderRequest(portfolioId, orderId));
        Console.WriteLine(response);
    }
    catch (CoinbasePrimeException ex)
    {
        PrintApiError(ex);
    }

    if (demoValidation)
    {
        Console.WriteLine();
        Console.WriteLine("POST create_order with an invalid product_id");
        try
        {
            ordersService.CreateOrder(new CreateOrderRequest(portfolioId)
            {
                ProductId = "NOT-A-PRODUCT",
                Side = CoinbaseSdk.Prime.Model.Enums.OrderSide.BUY,
                Type = CoinbaseSdk.Prime.Model.Enums.OrderType.MARKET,
                ClientOrderId = Guid.NewGuid().ToString(),
                BaseQuantity = "0.001",
            });
        }
        catch (PrimeBadRequestException ex)
        {
            Console.WriteLine("caught PrimeBadRequestException (HTTP 400)");
            PrintApiError(ex);
        }
        catch (CoinbasePrimeException ex)
        {
            PrintApiError(ex);
        }
    }

    Environment.ExitCode = 0;
});

return rootCommand.Invoke(args);

static void PrintApiError(CoinbasePrimeException error)
{
    Console.WriteLine($"  exception: {error.GetType().Name}");
    Console.WriteLine($"  status_code: {(int)error.StatusCode}");
    Console.WriteLine($"  message: {error.Message}");
    Console.WriteLine($"  code: {error.Code}");
    Console.WriteLine($"  subcode: {error.Subcode}");
    Console.WriteLine($"  trace_id: {error.TraceId}");
}
