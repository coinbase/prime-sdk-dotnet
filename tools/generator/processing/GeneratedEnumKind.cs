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

namespace CoinbaseSdk.Tools.Generator.Processing;

/// <summary>
/// Distinguishes domain enums (<c>model/enums</c>) from OpenAPI error Subcode enums (<c>model/errors</c>).
/// </summary>
public static class GeneratedEnumKind
{
  public const string EnumsNamespace = "CoinbaseSdk.Prime.Model.Enums";
  public const string ErrorsNamespace = "CoinbaseSdk.Prime.Model.Errors";
  public const string EnumsUsing = "using CoinbaseSdk.Prime.Model.Enums;";
  public const string ErrorsUsing = "using CoinbaseSdk.Prime.Model.Errors;";

  public static bool IsSubcode(string typeName)
  {
    return typeName.EndsWith("Subcode", StringComparison.Ordinal);
  }

  public static bool IsSubcodeClr(string clrType)
  {
    var name = clrType.Trim();
    while (name.EndsWith("[]", StringComparison.Ordinal) || name.EndsWith("?", StringComparison.Ordinal))
    {
      name = name.EndsWith("[]", StringComparison.Ordinal)
        ? name[..^2]
        : name[..^1];
    }

    return IsSubcode(name);
  }

  public static string NamespaceFor(string typeName)
  {
    return IsSubcode(typeName) ? ErrorsNamespace : EnumsNamespace;
  }

  public static string OutputDirectory(string typeName, string enumsDir, string errorsDir)
  {
    return IsSubcode(typeName) ? errorsDir : enumsDir;
  }
}
