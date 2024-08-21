// <copyright file="WorkbookTableParser.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Product.Component.Configuration.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using CSharpFunctionalExtensions;
    using global::FlexCel.Core;
    using UBind.Application.FlexCel;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Product.Component;

    /// <summary>
    /// Base class for a parser of a workbook table.
    /// </summary>
    public abstract class WorkbookTableParser
    {
        private int? sheetIndex;
        private TXlsCellRange range;
        private Dictionary<int, string> columnIndexes;
        private int? headerColumnIndex;
        private int? keyColumnIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookTableParser"/> class.
        /// </summary>
        /// <param name="workbook">The workbook.</param>
        /// <param name="tableName">The table name.</param>
        public WorkbookTableParser(FlexCelWorkbook workbook, string tableName)
        {
            this.Workbook = workbook;
            this.TableName = tableName;
        }

        /// <summary>
        /// Gets the workbook.
        /// </summary>
        protected FlexCelWorkbook Workbook { get; }

        /// <summary>
        /// Gets the table name.
        /// </summary>
        protected string TableName { get; }

        /// <summary>
        /// Gets the sheet index for the sheet the table is in.
        /// </summary>
        /// <returns>THe sheet index.</returns>
        protected int SheetIndex
        {
            get
            {
                if (this.sheetIndex == null)
                {
                    try
                    {
                        this.sheetIndex = this.Workbook.XlsFile.GetTableSheet(this.TableName);
                    }
                    catch (FlexCelException ex)
                    {
                        throw new ErrorException(
                            Errors.Product.WorkbookParseFailure($"Could not find the table {this.TableName}. " + ex.Message));
                    }
                }

                return (int)this.sheetIndex;
            }
        }

        /// <summary>
        /// Gets the range for the table.
        /// </summary>
        protected TXlsCellRange Range
        {
            get
            {
                if (this.range == null)
                {
                    TTableDefinition table = this.Workbook.XlsFile.GetTable(this.TableName);
                    if (table == null)
                    {
                        throw new ErrorException(
                            Errors.Product.WorkbookParseFailure($"Could not find the table {this.TableName}"));
                    }

                    this.range = table.Range;
                }

                return this.range;
            }
        }

        /// <summary>
        /// Gets the start column index for the range.
        /// </summary>
        protected int StartColumn => this.Range.Left;

        /// <summary>
        /// Gets the end column index for the range (inclusive).
        /// </summary>
        protected int EndColumn => this.Range.Right;

        /// <summary>
        /// Gets the index for the first row of data (not the heading row).
        /// </summary>
        protected int FirstRow => this.Range.Top + 1;

        /// <summary>
        /// Gets the index for the last row of data.
        /// </summary>
        protected int LastRow => this.Range.Bottom;

        /// <summary>
        /// Gets a dictionary of column names for column indexes for the table.
        /// </summary>
        /// <returns>A dictionary of column indexes.</returns>
        protected Dictionary<int, string> ColumnIndexes
        {
            get
            {
                if (this.columnIndexes == null)
                {
                    this.columnIndexes = this.Workbook.GetTableColumnNames(
                        this.SheetIndex,
                        this.Range.Top,
                        this.StartColumn,
                        this.EndColumn,
                        this.TableName);
                }

                return this.columnIndexes;
            }
        }

        /// <summary>
        /// Gets the index of the column named "Header".
        /// </summary>
        protected int HeaderColumn
        {
            get
            {
                if (this.headerColumnIndex == null)
                {
                    var headerColumns = this.ColumnIndexes.Where(c => c.Value == "Header");
                    if (!headerColumns.Any())
                    {
                        throw new ErrorException(Errors.Product.WorkbookParseFailure(
                            $"Could not find the header column in the table {this.TableName}"));
                    }

                    this.headerColumnIndex = headerColumns.First().Key;
                }

                return (int)this.headerColumnIndex;
            }
        }

        /// <summary>
        /// Gets the index of the column named "Key".
        /// </summary>
        protected int KeyColumn
        {
            get
            {
                if (this.keyColumnIndex == null)
                {
                    var keyColumns = this.ColumnIndexes.Where(c => c.Value == "Key");
                    if (!keyColumns.Any())
                    {
                        throw new ErrorException(Errors.Product.WorkbookParseFailure(
                            $"Could not find the Key column in the table {this.TableName}"));
                    }

                    this.keyColumnIndex = keyColumns.First().Key;
                }

                return (int)this.keyColumnIndex;
            }
        }

        /// <summary>
        /// Gets the cell value at the given coordinates.
        /// </summary>
        /// <typeparam name="TReturn">The type of value to be returned.</typeparam>
        /// <param name="rowIndex">The row index.</param>
        /// <param name="colIndex">The column index.</param>
        /// <returns>The value, or null if it was not of the expected type.</returns>
        protected TReturn GetCellValueOrDefault<TReturn>(int rowIndex, int colIndex)
        {
            var maybeValue = this.Workbook.GetCellValue<TReturn>(this.SheetIndex, rowIndex, colIndex);
            return maybeValue.HasValue
                ? maybeValue.Value
                : default;
        }

        /// <summary>
        /// Gets the cell value at the given coordinates.
        /// </summary>
        /// <typeparam name="TReturn">The type of value to be returned.</typeparam>
        /// <param name="rowIndex">The row index.</param>
        /// <param name="colIndex">The column index.</param>
        /// <returns>The value, or null if it was not of the expected type.</returns>
        protected Maybe<TReturn> GetCellValue<TReturn>(int rowIndex, int colIndex)
        {
            return this.Workbook.GetCellValue<TReturn>(this.SheetIndex, rowIndex, colIndex);
        }

        /// <summary>
        /// Gets the cell's named style at the given coordinates.
        /// </summary>
        /// <param name="rowIndex">The row index.</param>
        /// <param name="colIndex">The column index.</param>
        /// <returns>The named style, or null.</returns>
        protected string GetCellStyleName(int rowIndex, int colIndex)
        {
            return this.Workbook.GetCellStyleName(this.SheetIndex, rowIndex, colIndex);
        }

        /// <summary>
        /// Returns true if the first column of the row is empty. We don't check all columns because
        /// that would slow down parsing, and we know the uBindWorkbooks leave the first column empty to
        /// when they want that row to be ingored.
        /// </summary>
        /// <param name="rowIndex">The index of the row.</param>
        /// <returns>true if the first column of the row is empty.</returns>
        protected bool IsIgnorableRow(int rowIndex)
        {
            object cellValue = this.GetCellValueOrDefault<object>(rowIndex, this.StartColumn);
            return cellValue == null
                ? true
                : string.IsNullOrEmpty(cellValue.ToString());
        }

        /// <summary>
        /// Gets a value indicating whether the given row is a header row.
        /// </summary>
        /// <param name="rowIndex">The row index.</param>
        /// <returns>True if it's a header row, otherwise false.</returns>
        protected virtual bool IsHeaderRow(int rowIndex)
        {
            return this.GetCellValueOrDefault<string>(rowIndex, this.HeaderColumn) == "Yes";
        }

        /// <summary>
        /// Gets the value of the first column which is used as a header value when it's a header row.
        /// </summary>
        /// <param name="rowIndex">The row index.</param>
        /// <returns>The name of the header.</returns>
        protected string GetHeaderValue(int rowIndex)
        {
            string headerValue = this.GetCellValueOrDefault<string>(rowIndex, this.StartColumn);
            if (string.IsNullOrEmpty(headerValue))
            {
                throw new ErrorException(Errors.Product.WorkbookParseFailure(
                    $"When getting the header value in row {rowIndex} of {this.TableName}, the cell was "
                    + " either empty or did not contain a string value."));
            }

            return headerValue;
        }

        /// <summary>
        /// Gets the value of the header, if this is a header row, otherwise returns null.
        /// </summary>
        /// <param name="rowIndex">The row index.</param>
        /// <returns>The value of the header, if this is a header row, otherwise returns null.</returns>
        protected string GetHeaderValueOrNull(int rowIndex)
        {
            return this.IsHeaderRow(rowIndex)
                ? this.GetHeaderValue(rowIndex)
                : null;
        }

        /// <summary>
        /// Gets the value of the key column for the given rown.
        /// </summary>
        /// <param name="rowIndex">The index of the row.</param>
        /// <returns>The value of the key column.</returns>
        protected string GetKeyValue(int rowIndex)
        {
            string keyValue = this.GetCellValueOrDefault<string>(rowIndex, this.KeyColumn);
            return keyValue != null ? keyValue : string.Empty;
        }

        /// <summary>
        /// Parses a structure that has items typed by a header row, meaning a list of items, with their
        /// Type property being set by the most recent header value.
        /// </summary>
        /// <typeparam name="TItem">The type of the items.</typeparam>
        /// <param name="createItem">A delegate which creates the item with the given type property.</param>
        /// <param name="populateProperty">A delegate to populate a property on the item.
        /// This delegate takes 4 params:
        ///  - The column name
        ///  - the item to populate a property of
        ///  - the row index of the cell within the table
        ///  - the column index of the cell within the table.
        /// </param>
        /// <returns>A list of the items.</returns>
        protected List<TItem> ParseTypedByHeader<TItem>(
            Func<string, TItem> createItem,
            Action<string, TItem, int, int> populateProperty)
        {
            var items = new List<TItem>();
            string itemType = null;
            for (var rowIndex = this.FirstRow; rowIndex <= this.LastRow; rowIndex++)
            {
                if (this.IsIgnorableRow(rowIndex))
                {
                    continue;
                }

                string headerValue = this.GetHeaderValueOrNull(rowIndex);
                if (headerValue != null)
                {
                    itemType = headerValue;
                    continue;
                }

                if (itemType == null)
                {
                    throw new ErrorException(Errors.Product.WorkbookParseFailure(
                        $"Could not determine the item type. The first row in the table {this.TableName} should "
                        + "be a header row, with the column \"Header\" set to \"Yes\"."));
                }

                var item = createItem(itemType);
                this.LoopThroughColumnsAndPopulateProperties(rowIndex, item, populateProperty);
                items.Add(item);
            }

            return items;
        }

        /// <summary>
        /// Parses a structure that has items categorised by a header and subheader row which determines
        /// the category and subcategory.
        /// </summary>
        /// <typeparam name="TItem">The type of the items.</typeparam>
        /// <param name="createItem">A delegate which creates the item with the given category and subcategory.</param>
        /// <param name="populateProperty">A delegate to populate a property on the item.
        /// This delegate takes 4 params:
        ///  - The column name
        ///  - the item to populate a property of
        ///  - the row index of the cell within the table
        ///  - the column index of the cell within the table.
        /// </param>
        /// <returns>A list of the items.</returns>
        protected List<TItem> ParseItemsByCategoryAndSubCategory<TItem>(
            Func<string, string, TItem> createItem,
            Action<string, TItem, int, int> populateProperty)
        {
            var items = new List<TItem>();
            string category = null;
            string subcategory = null;
            for (var rowIndex = this.FirstRow; rowIndex <= this.LastRow; rowIndex++)
            {
                if (this.IsIgnorableRow(rowIndex))
                {
                    continue;
                }

                string headerValue = this.GetHeaderValueOrNull(rowIndex);
                if (headerValue != null)
                {
                    string headingStyle = this.GetCellStyleName(rowIndex, this.HeaderColumn);
                    switch (headingStyle)
                    {
                        case "uBind Heading 2":
                            category = headerValue;
                            subcategory = null;
                            break;
                        case "uBind Heading 3":
                            subcategory = headerValue;
                            break;
                        default:
                            throw new ErrorException(Errors.Product.WorkbookParseFailure(
                                $"When parsing the table {this.TableName}, we came across a heading row with a "
                                + $"style name of \"{headingStyle}\", however we were expecting a value of "
                                + "\"uBind Heading 2\" or \"uBind Heading 3\"."));
                    }

                    continue;
                }

                if (category == null)
                {
                    throw new ErrorException(Errors.Product.WorkbookParseFailure(
                        $"Could not determine the item type. The first row in the table {this.TableName} should "
                        + "be a header row, with the column \"Header\" set to \"Yes\"."));
                }

                var item = createItem(category, subcategory);
                this.LoopThroughColumnsAndPopulateProperties(rowIndex, item, populateProperty);
                items.Add(item);
            }

            return items;
        }

        /// <summary>
        /// Parses a structure that has items grouped by a header row, meaning a list of objects, each with contains
        /// a list of sub objects.
        /// </summary>
        /// <typeparam name="TGroup">The type of the object which groups the items.</typeparam>
        /// <typeparam name="TItem">The type of the items.</typeparam>
        /// <param name="createGroup">A delegate to create the group with the given header.
        /// This delegate takes 2 params:
        ///  - The column name
        ///  - the row index, in case the delegate wants to look up further values.
        /// </param>
        /// <param name="addItemToGroup">A delegate to add the item to the group.
        /// This delegate takes 2 params:
        ///  - The group object instance
        ///  - The item object instance.
        /// </param>
        /// <param name="populateProperty">A delegate to populate a property on the item.
        /// This delegate takes 4 params:
        ///  - The column name
        ///  - the item to populate a property of
        ///  - the row index of the cell within the table
        ///  - the column index of the cell within the table.
        /// </param>
        /// <param name="itemCompleted">A delegate which is called when an item's processing has completed.
        /// This can be useful if you want to perform postprocessing on the item.
        /// This delegate takes 2 param:
        ///  - The item which was completed
        ///  - The row index.
        /// </param>
        /// <returns>A list of the groups.</returns>
        protected List<TGroup> ParseGroupedByHeader<TGroup, TItem>(
            Func<string, int, TGroup> createGroup,
            Action<TGroup, TItem> addItemToGroup,
            Action<string, TItem, int, int> populateProperty,
            Action<TItem, int> itemCompleted = null)
            where TItem : new()
        {
            return this.ParseGroupedByHeaderWithItemFactory<TGroup, TItem>(
                createGroup,
                null,
                (typeName) => new TItem(),
                addItemToGroup,
                populateProperty,
                itemCompleted);
        }

        /// <summary>
        /// Parses a structure that has items grouped by a header row, meaning a list of objects, each with contains
        /// a list of sub objects which are of a different type and are created using a factory based upon their
        /// type name.
        /// </summary>
        /// <typeparam name="TGroup">The type of the object which groups the items.</typeparam>
        /// <typeparam name="TItem">The type of the items.</typeparam>
        /// <param name="createGroup">A delegate to create the group with the given header.
        /// This delegate takes 2 params:
        ///  - The column name
        ///  - the row index, in case the delegate wants to look up further values.
        /// </param>
        /// <param name="itemTypeNameColumnName">The name of the column which holds the type name,
        /// or null if this is not a dynamicly typed instance to be created with a factory.</param>
        /// <param name="createItem">A factory method that creates the item based upon the type name.</param>
        /// <param name="addItemToGroup">A delegate to add the item to the group.
        /// This delegate takes 3 params:
        ///  - The group object instance
        ///  - The item object instance.
        /// </param>
        /// <param name="populateProperty">A delegate to populate a property on the item.
        /// This delegate takes 4 params:
        ///  - The column name
        ///  - the item to populate a property of
        ///  - the row index of the cell within the table
        ///  - the column index of the cell within the table.
        /// </param>
        /// <param name="itemCompleted">A delegate which is called when an item's processing has completed.
        /// This can be useful if you want to perform postprocessing on the item.
        /// This delegate takes 2 param:
        ///  - The item which was completed
        ///  - The row index.
        /// </param>
        /// <returns>A list of the groups.</returns>
        protected List<TGroup> ParseGroupedByHeaderWithItemFactory<TGroup, TItem>(
            Func<string, int, TGroup> createGroup,
            string itemTypeNameColumnName,
            Func<string, TItem> createItem,
            Action<TGroup, TItem> addItemToGroup,
            Action<string, TItem, int, int> populateProperty,
            Action<TItem, int> itemCompleted = null)
        {
            var groups = new List<TGroup>();
            TGroup group = default;
            for (var rowIndex = this.FirstRow; rowIndex <= this.LastRow; rowIndex++)
            {
                if (this.IsIgnorableRow(rowIndex))
                {
                    continue;
                }

                string headerValue = this.GetHeaderValueOrNull(rowIndex);
                if (headerValue != null)
                {
                    if (group != null)
                    {
                        groups.Add(group);
                    }

                    group = createGroup(headerValue, rowIndex);
                    continue;
                }

                string typeName = null;
                if (itemTypeNameColumnName != null)
                {
                    typeName = this.GetItemTypeName(itemTypeNameColumnName, rowIndex);
                }

                TItem item;
                try
                {
                    item = createItem(typeName);
                }
                catch (Exception) when (typeName == null)
                {
                    throw new ErrorException(
                        Errors.Product.WorkbookParseFailure(
                            "When parsing a field in the workbook, the item's type was not specified."
                            + "Please ensure you specify the type.",
                            new List<string>
                            {
                                $"Base type: {typeof(TItem).Name}",
                                $"Table: {this.TableName}",
                                $"Row index: {rowIndex}",
                            }));
                }

                this.LoopThroughColumnsAndPopulateProperties(rowIndex, item, populateProperty);
                addItemToGroup(group, item);
                if (itemCompleted != null)
                {
                    itemCompleted(item, rowIndex);
                }
            }

            if (group != null)
            {
                groups.Add(group);
            }

            return groups;
        }

        /// <summary>
        /// Populates a property on an object using map from column name to property.
        /// </summary>
        /// <param name="columnToPropertyMap">A map from column name to property.</param>
        /// <param name="columnName">The column name in the workbook.</param>
        /// <param name="object">The object whose property should be populated.</param>
        /// <param name="rowIndex">The index of the column in the workbook.</param>
        /// <param name="colIndex">The row index.</param>
        protected void PopulateProperty(
            Dictionary<string, PropertyInfo> columnToPropertyMap,
            string columnName,
            object @object,
            int rowIndex,
            int colIndex)
        {
            if (columnToPropertyMap.TryGetValue(columnName, out PropertyInfo property))
            {
                Type propertyType = property.PropertyType;
                var method = this.GetType().GetMethod(
                    nameof(this.GetCellValue),
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new Type[] { typeof(int), typeof(int) },
                    null);
                var genericMethod = method.MakeGenericMethod(propertyType);
                try
                {
                    var result = genericMethod.Invoke(this, new object[] { rowIndex, colIndex });
                    this.SetValueOfPropertyFromResult(@object, property, result);
                }
                catch (TargetInvocationException ex) when (ex.GetBaseException() is FormatException formatException)
                {
                    throw new ErrorException(
                        Errors.Product.WorkbookParseFailure(
                            $"When attempting to parse a value as a {propertyType}, "
                            + $"the following error occurred: {formatException.Message}",
                            new List<string>
                            {
                                $"Table: {this.TableName}",
                                $"Column name: {columnName}",
                            }),
                        formatException);
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.GetBaseException();
                }
            }
        }

        /// <summary>
        /// Sets the value of a property from the result, which is expected to be of Type Maybe&lt;T&gt;.
        /// </summary>
        /// <param name="object">The object with the property to set the value of.</param>
        /// <param name="property">The property to set the value of.</param>
        /// <param name="result">The result, which is a Maybe type.</param>
        protected void SetValueOfPropertyFromResult(object @object, PropertyInfo property, object result)
        {
            var hasValueProperty = result.GetType().GetProperty("HasValue", BindingFlags.Instance | BindingFlags.Public);
            bool hasValue = (bool)hasValueProperty.GetValue(result);
            if (hasValue)
            {
                var valueProperty = result.GetType().GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);
                var value = valueProperty.GetValue(result);
                if (value is string stringValue && stringValue == string.Empty)
                {
                    PopulateWhenEmptyAttribute attr = property.GetCustomAttribute<PopulateWhenEmptyAttribute>();
                    if (attr == null || attr.Populate)
                    {
                        property.SetValue(@object, value);
                    }
                }
                else
                {
                    property.SetValue(@object, value);
                }
            }
        }

        /// <summary>
        /// Gets the value of the "Value" column when the "Property" value matches, within a given section.
        /// </summary>
        /// <typeparam name="TReturn">The type of the value.</typeparam>
        /// <param name="majorHeader">The major heading that the minor heading is found under.</param>
        /// <param name="minorHeader">The minor heading that the property is found under (optional).</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The value, if found, otherwise null.</returns>
        protected Maybe<TReturn> GetSectionPropertyValue<TReturn>(string majorHeader, string minorHeader, string propertyName)
        {
            int propertyColumnIndex = this.ColumnIndexes.Where(c => c.Value == "Property").First().Key;
            int valueColumnIndex = this.ColumnIndexes.Where(c => c.Value == "Value").First().Key;
            bool majorHeaderMatched = false;
            bool minorHeaderMatched = false;
            for (int rowIndex = this.FirstRow; rowIndex <= this.LastRow; rowIndex++)
            {
                if (!majorHeaderMatched || (!minorHeaderMatched && minorHeader != null))
                {
                    if (this.IsHeaderRow(rowIndex))
                    {
                        string headerValue = this.GetHeaderValue(rowIndex);
                        if (!majorHeaderMatched)
                        {
                            majorHeaderMatched = headerValue == majorHeader;
                        }
                        else if (!minorHeaderMatched && minorHeader != null)
                        {
                            minorHeaderMatched = headerValue == minorHeader;
                        }
                    }

                    continue;
                }

                if (this.IsHeaderRow(rowIndex))
                {
                    // We couldn't find it
                    return Maybe<TReturn>.None;
                }
                else if (this.GetCellValueOrDefault<string>(rowIndex, propertyColumnIndex) == propertyName)
                {
                    return this.GetCellValue<TReturn>(rowIndex, valueColumnIndex);
                }
            }

            return Maybe<TReturn>.None;
        }

        /// <summary>
        /// Gets the value of the "Value" column when the "Property" value matches, within a given section.
        /// </summary>
        /// <typeparam name="TReturn">The type of the value.</typeparam>
        /// <param name="majorHeader">The major heading that the minor heading is found under.</param>
        /// <param name="minorHeader">The minor heading that the property is found under (optional).</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The value, if found, otherwise null.</returns>
        protected TReturn GetSectionPropertyValueOrDefault<TReturn>(string majorHeader, string minorHeader, string propertyName)
        {
            Maybe<TReturn> maybe = this.GetSectionPropertyValue<TReturn>(majorHeader, minorHeader, propertyName);
            return maybe.HasValue ? maybe.Value : default;
        }

        /// <summary>
        /// Gets the value of the "Value" column for each of the properties in a given section, and retuns them as
        /// a List.
        /// </summary>
        /// <typeparam name="TReturn">The type of the values.</typeparam>
        /// <param name="majorHeader">The major heading that the minor heading is found under.</param>
        /// <param name="minorHeader">The minor heading that the property is found under (optional).</param>
        /// <returns>A List of values, if the section was found, otherwise an empty list.</returns>
        protected List<TReturn> GetSectionPropertyValues<TReturn>(string majorHeader, string minorHeader)
        {
            List<TReturn> results = new List<TReturn>();
            int valueColumnIndex = this.ColumnIndexes.Where(c => c.Value == "Value").First().Key;
            bool majorHeaderMatched = false;
            bool minorHeaderMatched = false;
            for (int rowIndex = this.FirstRow; rowIndex <= this.LastRow; rowIndex++)
            {
                if (!majorHeaderMatched || (!minorHeaderMatched && minorHeader != null))
                {
                    if (this.IsHeaderRow(rowIndex))
                    {
                        string headerValue = this.GetHeaderValue(rowIndex);
                        if (!majorHeaderMatched)
                        {
                            majorHeaderMatched = headerValue == majorHeader;
                        }
                        else if (!minorHeaderMatched && minorHeader != null)
                        {
                            minorHeaderMatched = headerValue == minorHeader;
                        }
                    }

                    continue;
                }

                if (this.IsHeaderRow(rowIndex))
                {
                    // We've hit the end of this section, so return the results
                    return results;
                }

                TReturn item = this.GetCellValueOrDefault<TReturn>(rowIndex, valueColumnIndex);
                if (item != null)
                {
                    results.Add(item);
                }
            }

            return results;
        }

        /// <summary>
        /// Gets a list of key value pairs, where the key is the "Property" column and the value is the "Value" column.
        /// </summary>
        /// <typeparam name="TKey">The type of the key, e.g. string.</typeparam>
        /// <typeparam name="TValue">The type of the value, e.g. string.</typeparam>
        /// <param name="majorHeader">The major heading that the minor heading is found under.</param>
        /// <param name="minorHeader">The minor heading that the property is found under (optional).</param>
        /// <returns>A List of key value pairs.</returns>
        protected List<KeyValuePair<TKey, TValue>> GetSectionPropertyKeyValuePairs<TKey, TValue>(
            string majorHeader,
            string minorHeader)
        {
            List<KeyValuePair<TKey, TValue>> results = new List<KeyValuePair<TKey, TValue>>();
            int propertyColumnIndex = this.ColumnIndexes.Where(c => c.Value == "Property").First().Key;
            int valueColumnIndex = this.ColumnIndexes.Where(c => c.Value == "Value").First().Key;
            bool majorHeaderMatched = false;
            bool minorHeaderMatched = false;
            for (int rowIndex = this.FirstRow; rowIndex <= this.LastRow; rowIndex++)
            {
                if (!majorHeaderMatched || (!minorHeaderMatched && minorHeader != null))
                {
                    if (this.IsHeaderRow(rowIndex))
                    {
                        string headerValue = this.GetHeaderValue(rowIndex);
                        if (!majorHeaderMatched)
                        {
                            majorHeaderMatched = headerValue == majorHeader;
                        }
                        else if (!minorHeaderMatched && minorHeader != null)
                        {
                            minorHeaderMatched = headerValue == minorHeader;
                        }
                    }

                    continue;
                }

                if (this.IsHeaderRow(rowIndex))
                {
                    // We've hit the end of this section, so return the results
                    return results;
                }

                TKey key = this.GetCellValueOrDefault<TKey>(rowIndex, propertyColumnIndex);
                TValue value = this.GetCellValueOrDefault<TValue>(rowIndex, valueColumnIndex);
                if (key != null && value != null)
                {
                    results.Add(new KeyValuePair<TKey, TValue>(key, value));
                }
            }

            return results;
        }

        private string GetItemTypeName(string itemTypeNameColumnName, int rowIndex)
        {
            var matchingColumns = this.ColumnIndexes.Where(c => c.Value == itemTypeNameColumnName);
            if (matchingColumns.Any())
            {
                int colIndex = matchingColumns.First().Key;
                return this.GetCellValueOrDefault<string>(rowIndex, colIndex);
            }

            return null;
        }

        private void LoopThroughColumnsAndPopulateProperties<TItem>(
            int rowIndex,
            TItem item,
            Action<string, TItem, int, int> populateProperty)
        {
            for (var colIndex = this.StartColumn; colIndex <= this.EndColumn; colIndex++)
            {
                string columnName;
                if (!this.ColumnIndexes.TryGetValue(colIndex, out columnName))
                {
                    continue;
                }

                populateProperty(columnName, item, rowIndex, colIndex);
            }
        }
    }
}
