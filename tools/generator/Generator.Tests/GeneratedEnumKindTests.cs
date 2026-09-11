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
using Xunit;

namespace CoinbaseSdk.Tools.Generator.Tests;

public class GeneratedEnumKindTests
{
  [Theory]
  [InlineData("CreateOrderForbiddenSubcode", true)]
  [InlineData("UnauthorizedSubcode", true)]
  [InlineData("OrderSide", false)]
  [InlineData("PrimeActivityType", false)]
  public void IsSubcode_DetectsErrorEnums(string typeName, bool expected)
  {
    Assert.Equal(expected, GeneratedEnumKind.IsSubcode(typeName));
  }

  [Theory]
  [InlineData("CreateOrderForbiddenSubcode?", true)]
  [InlineData("UnauthorizedSubcode[]", true)]
  [InlineData("OrderSide?", false)]
  public void IsSubcodeClr_StripsNullabilityAndArrays(string clrType, bool expected)
  {
    Assert.Equal(expected, GeneratedEnumKind.IsSubcodeClr(clrType));
  }

  [Fact]
  public void OutputDirectory_RoutesSubcodesToErrors()
  {
    Assert.Equal("errors", GeneratedEnumKind.OutputDirectory("TooManyRequestsSubcode", "enums", "errors"));
    Assert.Equal("errors", GeneratedEnumKind.OutputDirectory("BadRequestErrorCode", "enums", "errors"));
    Assert.Equal("enums", GeneratedEnumKind.OutputDirectory("WalletType", "enums", "errors"));
  }

  [Theory]
  [InlineData("BadRequestErrorCode", true)]
  [InlineData("CreateOrderForbiddenSubcode", true)]
  [InlineData("OrderSide", false)]
  public void IsErrorEnum_DetectsErrorCodesAndSubcodes(string typeName, bool expected)
  {
    Assert.Equal(expected, GeneratedEnumKind.IsErrorEnum(typeName));
  }

  [Fact]
  public void ApplyEnumMappings_AddsErrorsUsingForSubcodeReferences()
  {
    var transforms = new SharedTransforms(new GeneratorConfiguration());
    var content =
      "namespace CoinbaseSdk.Prime.Model\n{\n  using System.Text.Json.Serialization;\n\n  public class ErrorBody { public CreateOrderForbiddenSubcode Subcode { get; set; } }\n}\n";

    var result = transforms.ApplyEnumMappings(content, new HashSet<string> { "CreateOrderForbiddenSubcode", "OrderSide" });

    Assert.Contains(GeneratedEnumKind.ErrorsUsing, result, StringComparison.Ordinal);
    Assert.DoesNotContain(GeneratedEnumKind.EnumsUsing, result, StringComparison.Ordinal);
  }

  [Fact]
  public void ApplyEnumMappings_AddsErrorsUsingForErrorCodeReferences()
  {
    var transforms = new SharedTransforms(new GeneratorConfiguration());
    var content =
      "namespace CoinbaseSdk.Prime.Model\n{\n  using System.Text.Json.Serialization;\n\n  public class ErrorBody { public BadRequestErrorCode Code { get; set; } }\n}\n";

    var result = transforms.ApplyEnumMappings(content, new HashSet<string> { "BadRequestErrorCode", "OrderSide" });

    Assert.Contains(GeneratedEnumKind.ErrorsUsing, result, StringComparison.Ordinal);
    Assert.DoesNotContain(GeneratedEnumKind.EnumsUsing, result, StringComparison.Ordinal);
  }

  [Fact]
  public void ApplyEnumMappings_AddsEnumsUsingForDomainEnumReferences()
  {
    var transforms = new SharedTransforms(new GeneratorConfiguration());
    var content =
      "namespace CoinbaseSdk.Prime.Model\n{\n  using System.Text.Json.Serialization;\n\n  public class Order { public OrderSide Side { get; set; } }\n}\n";

    var result = transforms.ApplyEnumMappings(content, new HashSet<string> { "CreateOrderForbiddenSubcode", "OrderSide" });

    Assert.Contains(GeneratedEnumKind.EnumsUsing, result, StringComparison.Ordinal);
    Assert.DoesNotContain(GeneratedEnumKind.ErrorsUsing, result, StringComparison.Ordinal);
  }
}
