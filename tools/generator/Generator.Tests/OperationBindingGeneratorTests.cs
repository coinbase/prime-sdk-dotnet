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

using CoinbaseSdk.Tools.Generator.Processing;
using CoinbaseSdk.Tools.Generator.Spec;
using Xunit;

namespace CoinbaseSdk.Tools.Generator.Tests;

public class OperationBindingGeneratorTests
{
  [Theory]
  [InlineData("PrimeRESTAPI_GetXMLiquidation", "GetCrossMarginLiquidation")]
  [InlineData("PrimeRESTAPI_ListXMLiquidations", "ListCrossMarginLiquidations")]
  [InlineData("PrimeBeta_GetEntityRewardsRate", "GetEntityRewardsRate")]
  [InlineData("PrimeBeta_GetPortfolioRewardsRate", "GetPortfolioRewardsRate")]
  [InlineData("PrimeRESTAPI_GetOrder", "GetOrder")]
  public void DeriveSdkMethod_StripsKnownPrefixesAndAppliesRenames(string operationId, string expected)
  {
    var transforms = new SharedTransforms(new GeneratorConfiguration
    {
      AcronymMappings =
      [
        new AcronymMappingEntry { Acronym = "XM", Normalized = "Xm" },
      ],
    });
    var op = new ParsedOperation
    {
      OperationId = operationId,
      HttpMethod = "GET",
    };

    Assert.Equal(expected, OperationBindingGenerator.DeriveSdkMethod(op, transforms));
  }
}
