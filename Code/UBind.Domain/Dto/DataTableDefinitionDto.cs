// <copyright file="DataTableDefinitionDto.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Dto;

using System;
using Newtonsoft.Json;
using UBind.Domain.Entities;
using UBind.Domain.Models.DataTable;

public class DataTableDefinitionDto
{
    public DataTableDefinitionDto(DataTableDefinition dataTableDefinition)
    {
        this.Id = dataTableDefinition.Id;
        this.TenantId = dataTableDefinition.TenantId;
        this.EntityType = dataTableDefinition.EntityType;
        this.EntityId = dataTableDefinition.EntityId;
        this.Name = dataTableDefinition.Name;
        this.Alias = dataTableDefinition.Alias;
        this.TableSchema = dataTableDefinition.TableSchemaJson != null
            ? JsonConvert.DeserializeObject<DataTableSchema>(dataTableDefinition.TableSchemaJson)
            : null;
        this.DatabaseTableName = dataTableDefinition.DatabaseTableName;
        this.ColumnCount = dataTableDefinition.ColumnCount;
        this.RecordCount = dataTableDefinition.RecordCount;
        this.MemoryCachingEnabled = dataTableDefinition.MemoryCachingEnabled;
        this.CacheExpiryInSeconds = dataTableDefinition.CacheExpiryInSeconds;
    }

    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public EntityType EntityType { get; private set; }

    public Guid EntityId { get; private set; }

    public string Name { get; private set; }

    public string Alias { get; private set; }

    public DataTableSchema? TableSchema { get; private set; }

    public string DatabaseTableName { get; private set; }

    public int ColumnCount { get; private set; }

    public long RecordCount { get; private set; }

    public bool MemoryCachingEnabled { get; }

    public int CacheExpiryInSeconds { get; }
}
