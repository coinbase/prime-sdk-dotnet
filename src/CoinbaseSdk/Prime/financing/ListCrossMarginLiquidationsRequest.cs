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

namespace CoinbaseSdk.Prime.Financing
{
  using System.Text.Json.Serialization;
  using CoinbaseSdk.Core.Error;
  using CoinbaseSdk.Prime.Common;
  using CoinbaseSdk.Prime.Model.Enums;

  /// <summary>
  /// List Cross Margin Liquidations.
  /// </summary>
  public class ListCrossMarginLiquidationsRequest(string entityId) : PaginatedRequest
  {
    [JsonIgnore]
    public string EntityId { get; set; } = entityId;

    public string? Status { get; set; }

    public string? StartTime { get; set; }

    public string? EndTime { get; set; }

    public class ListCrossMarginLiquidationsRequestBuilder
    {
      private string? _entityId;
      private string? _status;
      private string? _startTime;
      private string? _endTime;
      private string? _cursor;
      private SortDirection? _sortDirection;
      private int? _limit;

      public ListCrossMarginLiquidationsRequestBuilder WithEntityId(string entityId)
      {
        _entityId = entityId;
        return this;
      }

      public ListCrossMarginLiquidationsRequestBuilder WithStatus(string? status)
      {
        _status = status;
        return this;
      }

      public ListCrossMarginLiquidationsRequestBuilder WithStartTime(string? startTime)
      {
        _startTime = startTime;
        return this;
      }

      public ListCrossMarginLiquidationsRequestBuilder WithEndTime(string? endTime)
      {
        _endTime = endTime;
        return this;
      }

      public ListCrossMarginLiquidationsRequestBuilder WithCursor(string cursor)
      {
        _cursor = cursor;
        return this;
      }

      public ListCrossMarginLiquidationsRequestBuilder WithSortDirection(SortDirection sortDirection)
      {
        _sortDirection = sortDirection;
        return this;
      }

      public ListCrossMarginLiquidationsRequestBuilder WithLimit(int limit)
      {
        _limit = limit;
        return this;
      }

      private void Validate()
      {
        if (string.IsNullOrWhiteSpace(_entityId))
        {
          throw new CoinbaseClientException("EntityId is required");
        }
      }

      public ListCrossMarginLiquidationsRequest Build()
      {
        Validate();
        return new ListCrossMarginLiquidationsRequest(_entityId!)
        {
          Status = _status,
          StartTime = _startTime,
          EndTime = _endTime,
          Cursor = _cursor,
          SortDirection = _sortDirection,
          Limit = _limit,
        };
      }
    }
  }
}
