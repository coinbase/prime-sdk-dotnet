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

using CoinbaseSdk.Tools.Generator;
using CoinbaseSdk.Tools.Generator.Processing;
using Xunit;

namespace CoinbaseSdk.Tools.Generator.Tests;

public class SchemaDocumentationIndexTests
{
  [Fact]
  public async Task LoadAsync_ResolvesTitleFallbackAndRefPropertyDocs()
  {
    var yamlPath = Path.Combine(Path.GetTempPath(), $"schema-doc-index-{Guid.NewGuid():N}.yaml");
    const string yaml = """
      openapi: 3.0.0
      components:
        schemas:
          coinbase.public_rest_api.Sample:
            type: object
            description: Sample model
            properties:
              name:
                title: Legal name
              side:
                $ref: '#/components/schemas/coinbase.public_rest_api.OrderSide'
          coinbase.public_rest_api.OrderSide:
            title: Order side
            type: string
            description: Side enum
            enum:
            - BUY
      """;

    await File.WriteAllTextAsync(yamlPath, yaml);

    try
    {
      var cfg = GeneratorConfiguration.Load(GeneratorPaths.FindProjectRoot());
      var transforms = new SharedTransforms(cfg);
      var index = await SchemaDocumentationIndex.LoadAsync(
        yamlPath,
        transforms,
        new Dictionary<string, string>(),
        new Dictionary<string, string>());

      var entry = index.TryGet("Sample");
      Assert.NotNull(entry);
      Assert.Equal("Sample model", entry!.TypeDoc);
      Assert.Equal("Legal name", entry.PropertyDocs["name"]);
      Assert.Equal("Side enum", entry.PropertyDocs["side"]);
    }
    finally
    {
      File.Delete(yamlPath);
    }
  }

  [Fact]
  public void DeriveClrName_AppliesCommonModelAndEnumNameMappings()
  {
    var cfg = GeneratorConfiguration.Load(GeneratorPaths.FindProjectRoot());
    var transforms = new SharedTransforms(cfg);

    var pagination = SchemaDocumentationIndex.DeriveClrName(
      "coinbase.public_rest_api.PaginatedResponse",
      transforms,
      cfg.CommonModels,
      cfg.EnumNameMappings);
    Assert.Equal("Pagination", pagination);

    var activityType = SchemaDocumentationIndex.DeriveClrName(
      "coinbase.public_rest_api.ActivityType",
      transforms,
      cfg.CommonModels,
      cfg.EnumNameMappings);
    Assert.Equal("PrimeActivityType", activityType);
  }

  [Fact]
  public async Task LoadAsync_LoadsErrorCatalogFromTags()
  {
    var yamlPath = Path.Combine(Path.GetTempPath(), $"schema-doc-catalog-{Guid.NewGuid():N}.yaml");
    const string yaml = """
      openapi: 3.0.0
      tags:
      - name: Error Codes
        x-error-codes:
        - name: VALIDATION_ERROR
          httpStatus: 400
          description: One or more request fields are invalid.
        x-subcodes:
        - name: ORDER_SIZE_INVALID
          errorCode: VALIDATION_ERROR
          httpStatus: 400
          description: The order size is invalid.
      components:
        schemas:
          coinbase.public_rest_api.BadRequestErrorCode:
            type: string
            enum:
            - VALIDATION_ERROR
      """;

    await File.WriteAllTextAsync(yamlPath, yaml);
    try
    {
      var cfg = GeneratorConfiguration.Load(GeneratorPaths.FindProjectRoot());
      var transforms = new SharedTransforms(cfg);
      var index = await SchemaDocumentationIndex.LoadAsync(
        yamlPath,
        transforms,
        new Dictionary<string, string>(),
        new Dictionary<string, string>());

      Assert.True(index.HasErrorCatalogDocs);
      Assert.Equal("One or more request fields are invalid.", index.TryGetErrorCatalogDoc("VALIDATION_ERROR"));
      Assert.Equal("The order size is invalid.", index.TryGetErrorCatalogDoc("ORDER_SIZE_INVALID"));
    }
    finally
    {
      File.Delete(yamlPath);
    }
  }
}
