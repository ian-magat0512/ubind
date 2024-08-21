// <copyright file="AutomationDeserializationConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation;

using System.Collections;
using Newtonsoft.Json;
using NodaTime;
using UBind.Application.Automation.Actions;
using UBind.Application.Automation.Actions.AdditionalPropetyValueActions;
using UBind.Application.Automation.Data;
using UBind.Application.Automation.Filters;
using UBind.Application.Automation.Http;
using UBind.Application.Automation.PathLookup;
using UBind.Application.Automation.Providers;
using UBind.Application.Automation.Providers.Binary;
using UBind.Application.Automation.Providers.Conditions;
using UBind.Application.Automation.Providers.Date;
using UBind.Application.Automation.Providers.DateTime;
using UBind.Application.Automation.Providers.Duration;
using UBind.Application.Automation.Providers.Entity;
using UBind.Application.Automation.Providers.Expression;
using UBind.Application.Automation.Providers.File;
using UBind.Application.Automation.Providers.File.ArchiveFile;
using UBind.Application.Automation.Providers.Integer;
using UBind.Application.Automation.Providers.List;
using UBind.Application.Automation.Providers.Number;
using UBind.Application.Automation.Providers.Object;
using UBind.Application.Automation.Providers.Object.PatchObject;
using UBind.Application.Automation.Providers.Period;
using UBind.Application.Automation.Providers.Text;
using UBind.Application.Automation.Providers.Time;
using UBind.Application.Automation.Providers.Value;
using UBind.Application.Automation.Triggers;
using UBind.Application.Automation.Triggers.ExtensionPointTrigger;
using UBind.Application.Export;
using ProviderEntity = UBind.Domain.SerialisedEntitySchemaObject;

/// <summary>
/// Specifies settings to use during deserialization of automation configuration json.
/// </summary>
public static class AutomationDeserializationConfiguration
{
    static AutomationDeserializationConfiguration()
    {
        BinaryProviderTypeMap = new TypeMap
        {
            { "fileBinary", typeof(FileBinaryProviderConfigModel) },
            { "base64TextBinary", typeof(Base64TextBinaryProviderConfigModel) },
            { "objectPathLookupBinary", typeof(PathLookupBinaryProviderConfigModel) },
        };

        GenericDataProviderTypeMap = new TypeMap
        {
            { "liquidText", typeof(LiquidTextProviderConfigModel) },
            { "razorText", typeof(RazorTextProviderConfigModel) },
            { "jsonText", typeof(ObjectToJsonTextProviderConfigModel) },
            { "objectToJsonText", typeof(ObjectToJsonTextProviderConfigModel) },
            { "environmentText", typeof(EnvironmentTextProviderConfigModel) },
            { "objectPathLookupText", typeof(PathLookupTextProviderConfigModel) },
            { "fileText", typeof(FileTextProviderConfigModel) },
            { "binaryBase64Text", typeof(BinaryBase64TextProviderConfigModel) },
            { "additionalPropertyValueText", typeof(AdditionalPropertyValueTextProviderConfigModel) },
            { "parseTextInteger", typeof(TextToIntegerProviderConfigModel) }, // Deprecated the parseTextInteger. But use the class for textToInteger provider.
            { "parseTextNumber", typeof(TextToNumberProviderConfigModel) }, // Deprecated the parseTextNumber. But use the class for textToNumber provider.
            { "parseTextDateTime", typeof(TextToDateTimeProviderConfigModel) }, // Deprecated the parseTextDateTime. But use the class for textToDateTime provider.
            { "dateAndTimeDateTime", typeof(DateAndTimeDateTimeProviderConfigModel) },
            { "parseTextDate", typeof(TextToDateProviderConfigModel) }, // Deprecated the parseTextDate. But use the class for textToDate provider.
            { "parseTextTime", typeof(TextToTimeProviderConfigModel) }, // Deprecated the parseTextTime. But use the class for textToTime provider.
            { "countListItemsInteger", typeof(CountListItemsIntegerProviderConfigModel) },
            { "objectPathLookupInteger", typeof(PathLookupIntegerProviderConfigModel) },
            { "objectPathLookupNumber", typeof(PathLookupNumberProviderConfigModel) },
            { "objectPathLookupDateTime", typeof(PathLookupDateTimeProviderConfigModel) },
            { "objectPathLookupDate", typeof(PathLookupDateProviderConfigModel) },
            { "objectPathLookupTime", typeof(PathLookupTimeProviderConfigModel) },
            { "jsonObject", typeof(JsonTextToObjectProviderConfigModel) },
            { "objectPathLookupListObject", typeof(PathLookupListObjectProviderConfigModel) },
            { "jsonTextToObject", typeof(JsonTextToObjectProviderConfigModel) },
            { "xmlObject", typeof(XmlTextToObjectProviderConfigModel) },
            { "xmlTextToObject", typeof(XmlTextToObjectProviderConfigModel) },
            { "objectPathLookupObject", typeof(PathLookupObjectProviderConfigModel) },
            { "patchObject", typeof(PatchObjectProviderConfigModel) },
            { "entityObject", typeof(EntityObjectProviderConfigModel) },
            { "kmlPlacemarksWhereCoordinatesInPolygonList", typeof(KmlPlacemarksWhereCoordinatesInPolygonListProviderConfigModel) },
            { "proRataCalculationByRiskAndStateObject", typeof(ProRataCalculationObjectProviderConfigModel) },
            { "entityObjectList", typeof(EntityObjectListProviderConfigModel) },
            { "filterListItemsList", typeof(FilterListItemsListProviderConfigModel) },
            { "entityQueryList", typeof(EntityQueryListProviderConfigModel) },
            { "objectPathLookupList", typeof(PathLookupListProviderConfigModel) },
            { "archiveFileContentsList", typeof(ArchiveFileContentsListProviderConfigModel) },
            { "andCondition", typeof(AndConditionConfigModel) },
            { "orCondition", typeof(OrConditionConfigModel) },
            { "notCondition", typeof(NotConditionConfigModel) },
            { "xorCondition", typeof(XOrConditionConfigModel) },
            { "integerIsEqualToCondition", typeof(IntegerIsEqualToConditionConfigModel) },
            { "integerIsGreaterThanCondition", typeof(IntegerIsGreaterThanConditionConfigModel) },
            { "integerIsGreaterThanOrEqualToCondition", typeof(IntegerIsGreaterThanOrEqualToConditionConfigModel) },
            { "integerIsLessThanCondition", typeof(IntegerIsLessThanConditionConfigModel) },
            { "integerIsLessThanOrEqualToCondition", typeof(IntegerIsLessThanOrEqualToConditionConfigModel) },
            { "numberIsEqualToCondition", typeof(NumberIsEqualToConditionConfigModel) },
            { "numberIsGreaterThanCondition", typeof(NumberIsGreaterThanConditionConfigModel) },
            { "numberIsGreaterThanOrEqualToCondition", typeof(NumberIsGreaterThanOrEqualToConditionConfigModel) },
            { "numberIsLessThanCondition", typeof(NumberIsLessThanConditionConfigModel) },
            { "numberIsLessThanOrEqualToCondition", typeof(NumberIsLessThanOrEqualToConditionConfigModel) },
            { "dateIsEqualToCondition", typeof(DateIsEqualToConditionConfigModel) },
            { "dateIsAfterCondition", typeof(DateIsAfterConditionConfigModel) },
            { "dateIsAfterOrEqualToCondition", typeof(DateIsAfterOrEqualToConditionConfigModel) },
            { "dateIsBeforeCondition", typeof(DateIsBeforeConditionConfigModel) },
            { "dateIsBeforeOrEqualToCondition", typeof(DateIsBeforeOrEqualToConditionConfigModel) },
            { "timeIsEqualToCondition", typeof(TimeIsEqualToConditionConfigModel) },
            { "timeIsAfterCondition", typeof(TimeIsAfterConditionConfigModel) },
            { "timeIsAfterOrEqualToCondition", typeof(TimeIsAfterOrEqualToConditionConfigModel) },
            { "timeIsBeforeCondition", typeof(TimeIsBeforeConditionConfigModel) },
            { "timeIsBeforeOrEqualToCondition", typeof(TimeIsBeforeOrEqualToConditionConfigModel) },
            { "dateTimeIsEqualToCondition", typeof(DateTimeIsEqualToConditionConfigModel) },
            { "dateTimeIsAfterCondition", typeof(DateTimeIsAfterConditionConfigModel) },
            { "dateTimeIsAfterOrEqualToCondition", typeof(DateTimeIsAfterOrEqualToConditionConfigModel) },
            { "dateTimeIsBeforeCondition", typeof(DateTimeIsBeforeConditionConfigModel) },
            { "dateTimeIsBeforeOrEqualToCondition", typeof(DateTimeIsBeforeOrEqualToConditionConfigModel) },
            { "textContainsCondition", typeof(TextContainsConditionConfigModel) },
            { "textStartsWithCondition", typeof(TextStartsWithConditionConfigModel) },
            { "textEndsWithCondition", typeof(TextEndsWithConditionConfigModel) },
            { "textIsEqualToCondition", typeof(TextIsEqualToConditionConfigModel) },
            { "textMatchesRegexPatternCondition", typeof(TextMatchesRegexPatternConditionConfigModel) },
            { "objectContainsPropertyCondition", typeof(ObjectContainsPropertyConditionConfigModel) },
            { "objectPathLookupCondition", typeof(PathLookupConditionConfigModel) },
            { "dateTimeIsInPeriodCondition", typeof(DateTimeIsInPeriodConditionConfigModel) },
            { "listContainsValueCondition", typeof(ListContainsValueConditionConfigModel) },
            { "integerToText", typeof(IntegerToTextProviderConfigModel) },
            { "numberToText", typeof(NumberToTextProviderConfigModel) },
            { "dateToText", typeof(DateToTextProviderConfigModel) },
            { "timeToText", typeof(TimeToTextProviderConfigModel) },
            { "dateTimeToText", typeof(DateTimeToTextProviderConfigModel) },
            { "textToInteger", typeof(TextToIntegerProviderConfigModel) },
            { "textToNumber", typeof(TextToNumberProviderConfigModel) },
            { "textToDate", typeof(TextToDateProviderConfigModel) },
            { "textToTime", typeof(TextToTimeProviderConfigModel) },
            { "textToDateTime", typeof(TextToDateTimeProviderConfigModel) },
            { "dataTableQueryList", typeof(DataTableQueryListProviderConfigModel) },
            { "conditionToText", typeof(ConditionToTextProviderConfigModel) },
            { "ipAddressInRangeCondition", typeof(IpAddressInRangeConditionConfigModel) },
            { "contextEntitiesObject", typeof(ContextEntitiesObjectProviderConfigModel) },
            { "textFile", typeof(TextFileProviderConfigModel) },
            { "productFile", typeof(ProductFileProviderConfigModel) },
            { "msWordFile", typeof(MSWordFileProviderConfigModel) },
            { "pdfFile", typeof(PdfFileProviderConfigModel) },
            { "msExcelFile", typeof(MsExcelFileProviderConfigModel) },
            { "entityFile", typeof(EntityFileProviderConfigModel) },
            { "archiveFile", typeof(ArchiveFileProviderConfigModel) },
            { "binaryFile", typeof(BinaryFileProviderConfigModel) },
            { "extractFromArchiveFile", typeof(ExtractFromArchiveFileProviderConfigModel) },
            { "objectPathLookupFile", typeof(PathLookupFileProviderConfigModel) },
            { "fileBinary", typeof(FileBinaryProviderConfigModel) },
            { "base64TextBinary", typeof(Base64TextBinaryProviderConfigModel) },
            { "objectPathLookupBinary", typeof(PathLookupBinaryProviderConfigModel) },
            { "tinyUrlText", typeof(TinyUrlTextProviderConfigModel) },
            { "valueToText", typeof(ValueToTextProviderConfigModel) },
            { "valueToInteger", typeof(ValueToIntegerProviderConfigModel) },
            { "valueToNumber", typeof(ValueToNumberProviderConfigModel) },
            { "objectPathLookupValue", typeof(PathLookupValueProviderConfigModel) },
        };

        Converters = new JsonConverter[]
        {
            // Triggers
            new PropertyDiscriminatorConverter<IBuilder<Trigger>>(
                new TypeMap
                {
                    { "httpTrigger", typeof(HttpTriggerConfigModel) },
                    { "emailTrigger", typeof(EmailTriggerConfigModel) },
                    { "eventTrigger", typeof(EventTriggerConfigModel) },
                    { "periodicTrigger", typeof(PeriodicTriggerConfigModel) },
                    { "extensionPointTrigger", typeof(ExtensionPointTriggerConfigModel) },
                    { "portalPageTrigger", typeof(PortalPageTriggerConfigModel) },
                }),
            new ExtensionPointTriggerConverter(),

            // Actions
            new PropertyDiscriminatorConverter<IBuilder<Action>>(
                new TypeMap
                {
                    { "raiseEventAction", typeof(RaiseEventActionConfigModel) },
                    { "httpRequestAction", typeof(HttpRequestActionConfigModel) },
                    { "sendEmailAction", typeof(SendEmailActionConfigModel) },
                    { "sendSmsAction", typeof(SendSmsActionConfigModel) },
                    { "attachFilesToEntityAction", typeof(AttachFilesToEntityActionConfigModel) },
                    { "attachFilesToEntitiesAction", typeof(AttachFilesToEntitiesActionConfigModel) },
                    { "raiseErrorAction", typeof(RaiseErrorActionConfigModel) },
                    { "groupAction", typeof(GroupActionConfigModel) },
                    { "iterateAction", typeof(IterateActionConfigModel) },
                    { "issuePolicyAction", typeof(IssuePolicyActionConfigModel) },
                    { "renewPolicyAction", typeof(RenewPolicyActionConfigModel) },
                    { "setVariableAction", typeof(SetVariableActionConfigModel) },
                    { "createOrganisationAction", typeof(CreateOrganisationConfigModel) },
                    { "uploadFileAction", typeof(UploadFileActionConfigModel) },
                    { "incrementAdditionalPropertyValueAction", typeof(IncrementAdditionalPropertyValueActionConfigModel) },
                    { "setAdditionalPropertyValueAction", typeof(SetAdditionalPropertyValueActionConfigModel) },
                    { "performQuoteCalculationAction", typeof(PerformQuoteCalculationActionConfigModel) },
                    { "approveQuoteAction", typeof(ApproveQuoteActionConfigModel) },
                    { "returnQuoteAction", typeof(ReturnQuoteActionConfigModel) },
                    { "declineQuoteAction", typeof(DeclineQuoteActionConfigModel) },
                    { "createUserAction", typeof(CreateUserActionConfigModel) },
                    { "createQuoteAction", typeof(CreateQuoteActionConfigModel) },
                }),

            // Providers (Text)
            new AutomationTextProviderConfigModelConverter(
               new TypeMap
               {
                   { "liquidText", typeof(LiquidTextProviderConfigModel) },
                   { "razorText", typeof(RazorTextProviderConfigModel) },
                   { "jsonText", typeof(ObjectToJsonTextProviderConfigModel) },
                   { "objectToJsonText", typeof(ObjectToJsonTextProviderConfigModel) },
                   { "environmentText", typeof(EnvironmentTextProviderConfigModel) },
                   { "objectPathLookupText", typeof(PathLookupTextProviderConfigModel) },
                   { "fileText", typeof(FileTextProviderConfigModel) },
                   { "additionalPropertyValueText", typeof(AdditionalPropertyValueTextProviderConfigModel) },
                   { "binaryBase64Text", typeof(BinaryBase64TextProviderConfigModel) },
                   { "integerToText", typeof(IntegerToTextProviderConfigModel) },
                   { "numberToText", typeof(NumberToTextProviderConfigModel) },
                   { "dateToText", typeof(DateToTextProviderConfigModel) },
                   { "timeToText", typeof(TimeToTextProviderConfigModel) },
                   { "dateTimeToText", typeof(DateTimeToTextProviderConfigModel) },
                   { "conditionToText", typeof(ConditionToTextProviderConfigModel) },
                   { "tinyUrlText", typeof(TinyUrlTextProviderConfigModel) },
                   { "valueToText", typeof(ValueToTextProviderConfigModel) },
               }),
            new LiquidTextSnippetConfigModelConverter(),
            new SystemEventTypeConverter(),
            new ObjectToJsonTextProviderConfigModelConverter(),
            new PathLookupTextProviderConfigModelConverter(),
            new FileTextProviderConfigModelConverter(),
            new BinaryBase64TextProviderConfigModelConverter(),
            new IntegerToTextProviderConfigModelConverter(),
            new NumberToTextProviderConfigModelConverter(),
            new DateToTextProviderConfigModelConverter(),
            new TimeToTextProviderConfigModelConverter(),
            new DateTimeToTextProviderConfigModelConverter(),
            new ConditionToTextProviderConfigModelConverter(),
            new TinyUrlTextProviderConfigModelConverter(),
            new ValueToTextProviderConfigModelConverter(),

            // Providers (Bool/Condition)
            new AutomationConditionConfigModelConverter(
               new TypeMap
               {
                   { "andCondition", typeof(AndConditionConfigModel) },
                   { "orCondition", typeof(OrConditionConfigModel) },
                   { "notCondition", typeof(NotConditionConfigModel) },
                   { "xorCondition", typeof(XOrConditionConfigModel) },
                   { "integerIsEqualToCondition", typeof(IntegerIsEqualToConditionConfigModel) },
                   { "integerIsGreaterThanCondition", typeof(IntegerIsGreaterThanConditionConfigModel) },
                   { "integerIsGreaterThanOrEqualToCondition", typeof(IntegerIsGreaterThanOrEqualToConditionConfigModel) },
                   { "integerIsLessThanCondition", typeof(IntegerIsLessThanConditionConfigModel) },
                   { "integerIsLessThanOrEqualToCondition", typeof(IntegerIsLessThanOrEqualToConditionConfigModel) },
                   { "numberIsEqualToCondition", typeof(NumberIsEqualToConditionConfigModel) },
                   { "numberIsGreaterThanCondition", typeof(NumberIsGreaterThanConditionConfigModel) },
                   { "numberIsGreaterThanOrEqualToCondition", typeof(NumberIsGreaterThanOrEqualToConditionConfigModel) },
                   { "numberIsLessThanCondition", typeof(NumberIsLessThanConditionConfigModel) },
                   { "numberIsLessThanOrEqualToCondition", typeof(NumberIsLessThanOrEqualToConditionConfigModel) },
                   { "dateIsEqualToCondition", typeof(DateIsEqualToConditionConfigModel) },
                   { "dateIsAfterCondition", typeof(DateIsAfterConditionConfigModel) },
                   { "dateIsAfterOrEqualToCondition", typeof(DateIsAfterOrEqualToConditionConfigModel) },
                   { "dateIsBeforeCondition", typeof(DateIsBeforeConditionConfigModel) },
                   { "dateIsBeforeOrEqualToCondition", typeof(DateIsBeforeOrEqualToConditionConfigModel) },
                   { "timeIsEqualToCondition", typeof(TimeIsEqualToConditionConfigModel) },
                   { "timeIsAfterCondition", typeof(TimeIsAfterConditionConfigModel) },
                   { "timeIsAfterOrEqualToCondition", typeof(TimeIsAfterOrEqualToConditionConfigModel) },
                   { "timeIsBeforeCondition", typeof(TimeIsBeforeConditionConfigModel) },
                   { "timeIsBeforeOrEqualToCondition", typeof(TimeIsBeforeOrEqualToConditionConfigModel) },
                   { "dateTimeIsEqualToCondition", typeof(DateTimeIsEqualToConditionConfigModel) },
                   { "dateTimeIsAfterCondition", typeof(DateTimeIsAfterConditionConfigModel) },
                   { "dateTimeIsAfterOrEqualToCondition", typeof(DateTimeIsAfterOrEqualToConditionConfigModel) },
                   { "dateTimeIsBeforeCondition", typeof(DateTimeIsBeforeConditionConfigModel) },
                   { "dateTimeIsBeforeOrEqualToCondition", typeof(DateTimeIsBeforeOrEqualToConditionConfigModel) },
                   { "textContainsCondition", typeof(TextContainsConditionConfigModel) },
                   { "textStartsWithCondition", typeof(TextStartsWithConditionConfigModel) },
                   { "textEndsWithCondition", typeof(TextEndsWithConditionConfigModel) },
                   { "textIsEqualToCondition", typeof(TextIsEqualToConditionConfigModel) },
                   { "textMatchesRegexPatternCondition", typeof(TextMatchesRegexPatternConditionConfigModel) },
                   { "objectContainsPropertyCondition", typeof(ObjectContainsPropertyConditionConfigModel) },
                   { "objectPathLookupCondition", typeof(PathLookupConditionConfigModel) },
                   { "dateTimeIsInPeriodCondition", typeof(DateTimeIsInPeriodConditionConfigModel) },
                   { "listContainsValueCondition", typeof(ListContainsValueConditionConfigModel) },
                   { "ipAddressInRangeCondition", typeof(IpAddressInRangeConditionConfigModel) },
               }),
            new LogicOperationConditionConfigModelConverter(),
            new PathLookupConditionProviderConfigModelConverter(),
            new NotConditionConfigModelConverter(),
            new ListContainsValueConditionConfigModelConverter(),

            // Providers (Int)
            new AutomationIntegerProviderConfigModelConverter(
               new TypeMap
               {
                   { "parseTextInteger", typeof(TextToIntegerProviderConfigModel) },
                   { "countListItemsInteger", typeof(CountListItemsIntegerProviderConfigModel) },
                   { "objectPathLookupInteger", typeof(PathLookupIntegerProviderConfigModel) },
                   { "textToInteger", typeof(TextToIntegerProviderConfigModel) },
                   { "valueToInteger", typeof(ValueToIntegerProviderConfigModel) },
               }),
            new CountListItemsIntegerProviderConfigModelConverter(),
            new PathLookupIntegerProviderConfigModelConverter(),
            new TextToIntegerProviderConfigModelConverter(),
            new ValueToIntegerProviderConfigModelConverter(),

            // Providers (Number)
            new AutomationNumberProviderConfigModelConverter(
                new TypeMap
               {
                    { "parseTextNumber", typeof(TextToNumberProviderConfigModel) },
                    { "objectPathLookupNumber", typeof(PathLookupNumberProviderConfigModel) },
                    { "textToNumber", typeof(TextToNumberProviderConfigModel) },
                    { "valueToNumber", typeof(ValueToNumberProviderConfigModel) },
               }),
            new PathLookupNumberProviderConfigModelConverter(),
            new TextToNumberProviderConfigModelConverter(),
            new ValueToNumberProviderConfigModelConverter(),

            // Providers (DateTime)
            new DateTimeProviderConfigModelConverter(
                new TypeMap
                {
                    { "dateAndTimeDateTime", typeof(DateAndTimeDateTimeProviderConfigModel) },
                    { "parseTextDateTime", typeof(TextToDateTimeProviderConfigModel) },
                    { "objectPathLookupDateTime", typeof(PathLookupDateTimeProviderConfigModel) },
                    { "textToDateTime", typeof(TextToDateTimeProviderConfigModel) },
                    { "dateToDateTime", typeof(DateToDateTimeProviderConfigModel) },
                }),
            new PathLookupDateTimeProviderConfigModelConverter(),
            new TextToDateTimeProviderConfigModelConverter(),
            new DateToDateTimeProviderConfigModelConverter(),

            // Providers (Date)
            new DateProviderConfigModelConverter(
                new TypeMap
                {
                    { "parseTextDate", typeof(TextToDateProviderConfigModel) },
                    { "objectPathLookupDate", typeof(PathLookupDateProviderConfigModel) },
                    { "textToDate", typeof(TextToDateProviderConfigModel) },
                }),
            new PathLookupDateProviderConfigModelConverter(),
            new TextToDateProviderConfigModelConverter(),

            // Provders (Time)
            new TimeProviderConfigModelConverter(
                new TypeMap
                {
                    { "parseTextTime", typeof(TextToTimeProviderConfigModel) },
                    { "objectPathLookupTime", typeof(PathLookupTimeProviderConfigModel) },
                    { "textToTime", typeof(TextToTimeProviderConfigModel) },
                }),
            new PathLookupTimeProviderConfigModelConverter(),
            new TextToTimeProviderConfigModelConverter(),

            // Providers (Object)
            new AutomationObjectProviderConfigModelConverter (
               new TypeMap
               {
                   { "jsonObject", typeof(JsonTextToObjectProviderConfigModel) },
                   { "jsonTextToObject", typeof(JsonTextToObjectProviderConfigModel) },
                   { "objectPathLookupObject", typeof(PathLookupObjectProviderConfigModel) },
                   { "xmlObject", typeof(XmlTextToObjectProviderConfigModel) },
                   { "xmlTextToObject", typeof(XmlTextToObjectProviderConfigModel) },
                   { "entityObject", typeof(EntityObjectProviderConfigModel) },
                   { "contextEntitiesObject", typeof(ContextEntitiesObjectProviderConfigModel) },
                   { "objectPathLookupListObject", typeof(PathLookupListObjectProviderConfigModel) },
                   { "patchObject", typeof(PatchObjectProviderConfigModel) },
                   { "proRataCalculationByRiskAndStateObject", typeof(ProRataCalculationObjectProviderConfigModel) },
               }),
            new JsonTextToObjectProviderConfigModelConverter(),
            new PathLookupObjectProviderConfigModelConverter(),
            new XmlTextToObjectProviderConfigModelConverter(),
            new EntityObjectProviderConfigModelConverter(),
            new ContextEntitiesObjectProviderConfigModelConverter(),
            new PathLookupListObjectProviderConfigModelConverter(),
            new PatchObjectProviderConfigModelConverter(),
            new ProRataCalculationObjectProviderConfigModelConverter(),

            // Providers (List)
            new AutomationListProviderConfigModelConverter(
                new TypeMap
                {
                    { "kmlPlacemarksWhereCoordinatesInPolygonList", typeof(KmlPlacemarksWhereCoordinatesInPolygonListProviderConfigModel) },
                    { "filterListItemsList", typeof(FilterListItemsListProviderConfigModel) },
                    { "entityQueryList", typeof(EntityQueryListProviderConfigModel) },
                    { "objectPathLookupList", typeof(PathLookupListProviderConfigModel) },
                    { "entityObjectList", typeof(EntityObjectListProviderConfigModel) },
                    { "archiveFileContentsList", typeof(ArchiveFileContentsListProviderConfigModel) },
                    { "dataTableQueryList", typeof(DataTableQueryListProviderConfigModel) },
                }),
            new EntityObjectListProviderConfigModelConverter(),
            new PathLookupListProviderConfigModelConverter(),

            // Providers (IData)
            new AutomationGenericObjectProviderConfigModelConverter(GenericDataProviderTypeMap),

            // Providers (Value)
            new PathLookupValueProviderConfigModelConverter(),

            // Providers (Filters)
            new PropertyDiscriminatorConverter<IBuilder<IFilterProvider>>(
                new TypeMap
                {
                    { "andCondition", typeof(AndFilterProviderConfigModel) },
                    { "orCondition", typeof(OrFilterProviderConfigModel) },
                    { "xorCondition", typeof(XOrFilterProviderConfigModel) },
                    { "notCondition", typeof(NotFilterProviderConfigModel) },
                    { "integerIsEqualToCondition", typeof(IntegerEqualToFilterProviderConfigModel) },
                    { "integerIsGreaterThanCondition", typeof(IntegerGreaterThanFilterProviderConfigModel) },
                    { "integerIsGreaterThanOrEqualToCondition", typeof(IntegerGreaterThanOrEqualToFilterProviderConfigModel) },
                    { "integerIsLessThanCondition", typeof(IntegerLessThanFilterProviderConfigModel) },
                    { "integerIsLessThanOrEqualToCondition", typeof(IntegerLessThanOrEqualToFilterProviderConfigModel) },
                    { "numberIsEqualToCondition", typeof(NumberEqualToFilterProviderConfigModel) },
                    { "numberIsGreaterThanCondition", typeof(NumberGreaterThanFilterProviderConfigModel) },
                    { "numberIsGreaterThanOrEqualToCondition", typeof(NumberGreaterThanOrEqualToFilterProviderConfigModel) },
                    { "numberIsLessThanCondition", typeof(NumberLessThanFilterProviderConfigModel) },
                    { "numberIsLessThanOrEqualToCondition", typeof(NumberLessThanOrEqualToFilterProviderConfigModel) },
                    { "textContainsCondition", typeof(TextContainsFilterProviderConfigModel) },
                    { "textEndsWithCondition", typeof(TextEndsWithFilterProviderConfigModel) },
                    { "textIsEqualToCondition", typeof(TextIsEqualToFilterProviderConfigModel) },
                    { "textMatchesRegexPatternCondition", typeof(TextMatchesRegexPatternFilterProviderConfigModel) },
                    { "textStartsWithCondition", typeof(TextStartsWithFilterProviderConfigModel) },
                    { "dateIsEqualToCondition", typeof(DateEqualToFilterProviderConfigModel) },
                    { "dateIsBeforeCondition", typeof(DateBeforeFilterProviderConfigModel) },
                    { "dateIsBeforeOrEqualToCondition", typeof(DateBeforeOrEqualToFilterProviderConfigModel) },
                    { "dateIsAfterCondition", typeof(DateAfterFilterProviderConfigModel) },
                    { "dateIsAfterOrEqualToCondition", typeof(DateAfterOrEqualToFilterProviderConfigModel) },
                    { "timeIsEqualToCondition", typeof(TimeEqualToFilterProviderConfigModel) },
                    { "timeIsAfterCondition", typeof(TimeAfterFilterProviderConfigModel) },
                    { "timeIsAfterOrEqualToCondition", typeof(TimeAfterOrEqualToFilterProviderConfigModel) },
                    { "timeIsBeforeCondition", typeof(TimeBeforeFilterProviderConfigModel) },
                    { "timeIsBeforeOrEqualToCondition", typeof(TimeBeforeOrEqualToFilterProviderConfigModel) },
                    { "dateTimeIsEqualToCondition", typeof(DateTimeEqualToFilterProviderConfigModel) },
                    { "dateTimeIsAfterCondition", typeof(DateTimeAfterFilterProviderConfigModel) },
                    { "dateTimeIsAfterOrEqualToCondition", typeof(DateTimeAfterOrEqualToFilterProviderConfigModel) },
                    { "dateTimeIsBeforeCondition", typeof(DateTimeBeforeFilterProviderConfigModel) },
                    { "dateTimeIsBeforeOrEqualToCondition", typeof(DateTimeBeforeOrEqualToFilterProviderConfigModel) },
                    { "dateTimeIsInPeriodCondition", typeof(DateTimeInPeriodFilterProviderConfigModel) },
                    { "objectContainsPropertyCondition", typeof(ObjectContainsPropertyFilterProviderConfigModel) },
                    { "listContainsValueCondition", typeof(ListContainsValueFilterProviderConfigModel) },
                    { "listCondition", typeof(ListConditionFilterProviderConfigModel) },
                    { "objectPathLookupCondition", typeof(ObjectPathLookupConditionFilterProviderConfigModel) },
                }),
            new AggregateFilterProviderConfigModelConverter(),
            new NotFilterProviderConfigModelConverter(),

            // Expressions (Date/Time)
            new DateTimeExpressionProviderConfigModelConverter(
                new TypeMap
                {
                    { "objectPathLookupDateTime", typeof(ObjectPathLookupExpressionProviderConfigModel<long>) },
                }),
            new DateExpressionProviderConfigModelConverter(
                new TypeMap
                {
                    { "objectPathLookupDate", typeof(ObjectPathLookupExpressionProviderConfigModel<LocalDate>) },
                }),
            new TimeExpressionProviderConfigModelConverter(
                new TypeMap
                {
                    { "objectPathLookupTime", typeof(ObjectPathLookupExpressionProviderConfigModel<LocalTime>) },
                }),

            // Expressions (Object)
            new ObjectExpressionProviderConfigModelConverter(
                new TypeMap
                {
                    { "objectPathLookupObject", typeof(ObjectPathLookupExpressionProviderConfigModel<object>) },
                }),

            new ListExpressionProviderConfigModelConverter(
                new TypeMap
                {
                    { "objectPathLookupList", typeof(ObjectPathLookupExpressionProviderConfigModel<IEnumerable>) },
                }),

            // Expressions (Path Lookups)
            new ObjectPathLookupExpressionProviderConfigModelConverter(),
            new PropertyExpressionProviderConfigModelConverter(),
            new ExpressionProviderConfigModelConverter(
                new TypeMap
                {
                    { "property", typeof(PropertyExpressionProviderConfigModel) },
                    { "objectPathLookupList", typeof(ObjectPathLookupExpressionProviderConfigModel<IEnumerable>) },
                    { "objectPathLookupText", typeof(ObjectPathLookupExpressionProviderConfigModel<string>) },
                    { "objectPathLookupInteger", typeof(ObjectPathLookupExpressionProviderConfigModel<long>) },
                    { "objectPathLookupNumber", typeof(ObjectPathLookupExpressionProviderConfigModel<decimal>) },
                    { "objectPathLookupDateTime", typeof(ObjectPathLookupExpressionProviderConfigModel<long>) },
                    { "objectPathLookupDate", typeof(ObjectPathLookupExpressionProviderConfigModel<LocalDate>) },
                    { "objectPathLookupTime", typeof(ObjectPathLookupExpressionProviderConfigModel<LocalTime>) },
                    { "objectPathLookupObject", typeof(ObjectPathLookupExpressionProviderConfigModel<object>) },
                },
                GenericDataProviderTypeMap),

            // Base Path Lookups
            new ObjectPathLookupConfigModelConverter(),

            // Http Request Objects
            new HttpHeaderConfigModelConverter(),
            new ContentProviderConfigModelConverter(),

            // Providers (Period)
            new PropertyDiscriminatorConverter<IBuilder<IProvider<Data<Interval>>>>(
                new TypeMap
                {
                    { "fromDateTimeToDateTimePeriod", typeof(FromDateTimeToDateTimePeriodProviderConfigModel) },
                    { "lastPeriod", typeof(LastPeriodProviderConfigModel) },
                }),
            new LastPeriodProviderConfigModelConverter(),

            // Providers (Duration)
            new DurationProviderConfigModelConverter(
                new TypeMap
                {
                    { "periodTypeValueDuration", typeof(PeriodTypeValueDurationConfigModel) },
                }),

            // Providers (File)
            new PropertyDiscriminatorConverter<IBuilder<IProvider<Data<FileInfo>>>>(
                new TypeMap
                {
                    { "textFile", typeof(TextFileProviderConfigModel) },
                    { "productFile", typeof(ProductFileProviderConfigModel) },
                    { "msWordFile", typeof(MSWordFileProviderConfigModel) },
                    { "pdfFile", typeof(PdfFileProviderConfigModel) },
                    { "msExcelFile", typeof(MsExcelFileProviderConfigModel) },
                    { "entityFile", typeof(EntityFileProviderConfigModel) },
                    { "archiveFile", typeof(ArchiveFileProviderConfigModel) },
                    { "binaryFile", typeof(BinaryFileProviderConfigModel) },
                    { "extractFromArchiveFile", typeof(ExtractFromArchiveFileProviderConfigModel) },
                    { "objectPathLookupFile", typeof(PathLookupFileProviderConfigModel) },
                }),
            new PdfFileProviderConfigModelConverter(),
            new PathLookupFileProviderConfigModelConverter(),

            // Providers (Entity)
            new PropertyDiscriminatorConverter<IBuilder<BaseEntityProvider>>(
                new TypeMap
                {
                    { "dynamicEntity", typeof(DynamicEntityProviderConfigModel) },
                    { "quote", typeof(QuoteEntityProviderConfigModel) },
                    { "quoteVersion", typeof(QuoteVersionEntityProviderConfigModel) },
                    { "policy", typeof(PolicyEntityProviderConfigModel) },
                    { "policyTransaction", typeof(PolicyTransactionEntityProviderConfigModel) },
                    { "tenant", typeof(TenantEntityProviderConfigModel) },
                    { "product", typeof(ProductEntityProviderConfigModel) },
                    { "customer", typeof(CustomerEntityProviderConfigModel) },
                    { "user", typeof(UserEntityProviderConfigModel) },
                    { "emailMessage", typeof(EmailMessageEntityProviderConfigModel) },
                    { "smsMessage", typeof(SmsMessageEntityProviderConfigModel) },
                    { "document", typeof(DocumentEntityProviderConfigModel) },
                    { "claim", typeof(ClaimEntityProviderConfigModel) },
                    { "claimVersion", typeof(ClaimVersionEntityProviderConfigModel) },
                    { "organisation", typeof(OrganisationEntityProviderConfigModel) },
                    { "portal", typeof(PortalEntityProviderConfigModel) },
                    { "role", typeof(RoleEntityProviderConfigModel) },
                    { "report", typeof(ReportEntityProviderConfigModel) },
                    { "person", typeof(PersonEntityProviderConfigModel) },
                    { "contextEntity", typeof(ContextEntityProviderConfigModel) },
                    { "event", typeof(SystemEventEntityProviderConfigModel) },
                }),
            new QuoteEntityProviderConfigModelConverter(),
            new QuoteVersionEntityProviderConfigModelConverter(),
            new PolicyEntityProviderConfigModelConverter(),
            new PolicyTransactionEntityProviderConfigModelConverter(),
            new TenantEntityProviderConfigModelConverter(),
            new ProductEntityProviderConfigModelConverter(),
            new CustomerEntityProviderConfigModelConverter(),
            new UserEntityProviderConfigModelConverter(),
            new EmailMessageEntityProviderConfigModelConverter(),
            new SmsMessageEntityProviderConfigModelConverter(),
            new DocumentEntityProviderConfigModelConverter(),
            new ClaimEntityProviderConfigModelConverter(),
            new ClaimVersionEntityProviderConfigModelConverter(),
            new OrganisationEntityProviderConfigModelConverter(),
            new PortalEntityProviderConfigModelConverter(),
            new RoleEntityProviderConfigModelConverter(),
            new ReportEntityProviderConfigModelConverter(),
            new PersonEntityProviderConfigModelConverter(),
            new ContextEntityProviderConfigModelConverter(),
            new TimeConfigModelConverter(),
            new DayConfigModelConverter(),
            new PropertyDiscriminatorConverter<IBuilder<IProvider<Data<byte[]>>>>(BinaryProviderTypeMap),
            new FileBinaryProviderConfigModelConverter(),
            new Base64TextBinaryProviderConfigModelConverter(),
            new PathLookupBinaryProviderConfigModelConverter(),
            new SystemEventEntityProviderConfigModelConverter(),

            // Patch Object Operations
            new PropertyDiscriminatorConverter<IBuilder<BaseOperation>>(
                new TypeMap
                {
                    { "add", typeof(AddOperationConfigModel) },
                    { "remove", typeof(RemoveOperationConfigModel) },
                    { "copy", typeof(CopyOperationConfigModel) },
                    { "move", typeof(MoveOperationConfigModel) },
                    { "replace", typeof(ReplaceOperationConfigModel) },
                }),

            // Archive File Operations
            new PropertyDiscriminatorConverter<IBuilder<Providers.File.ArchiveFile.Operation>>(
                new TypeMap
                {
                    { "addFile", typeof(AddFileOperationConfigModel) },
                    { "addFolder", typeof(AddFolderOperationConfigModel) },
                    { "copyFile", typeof(CopyFileOperationConfigModel) },
                    { "copyFolder", typeof(CopyFolderOperationConfigModel) },
                    { "moveFile", typeof(MoveFileOperationConfigModel) },
                    { "moveFolder", typeof(MoveFolderOperationConfigModel) },
                    { "removeEntry", typeof(RemoveEntryOperationConfigModel) },
                    { "replaceFile", typeof(ReplaceFileOperationConfigModel) },
                }),
        };

        DataConverters = new JsonConverter[]
        {
            new GenericConverter<TriggerData>(
                new TypeMap
                {
                    { "httpTrigger", typeof(HttpTriggerData) },
                    { "eventTrigger", typeof(EventTriggerData) },
                    { "periodicTrigger", typeof(PeriodicTriggerData) },
                    { "extensionPointTrigger", typeof(ExtensionPointTriggerData) },
                    { "portalPageTrigger", typeof(PortalPageTriggerData) },
                }),
            new ExtensionPointTriggerDataConverter(),
            new GenericConverter<ActionData>(
                new TypeMap
                {
                    { "httpRequestAction", typeof(HttpRequestActionData) },
                    { "sendEmailAction", typeof(SendEmailActionData) },
                    { "sendSmsAction", typeof(SendSmsActionData) },
                    { "raiseEventAction", typeof(RaiseEventActionData) },
                    { "attachFilesToEntitiesAction", typeof(AttachFilesToEntitiesActionData) },
                    { "raiseErrorAction", typeof(RaiseErrorActionData) },
                    { "groupAction", typeof(GroupActionData) },
                    { "iterateAction", typeof(IterateActionData) },
                    { "issuePolicyAction", typeof(IssuePolicyActionData) },
                    { "renewPolicyAction", typeof(RenewPolicyAction) },
                    { "setVariableAction", typeof(SetVariableActionData) },
                    { "createOrganisationAction", typeof(CreateOrganisationActionData) },
                    { "uploadFileAction", typeof(UploadFileActionData) },
                    { "setAdditionalPropertyValueAction", typeof(SetAdditionalPropertyValueActionData) },
                    { "performQuoteCalculationAction", typeof(PerformQuoteCalculationActionData) },
                    { "approveQuoteAction", typeof(ApproveQuoteActionData) },
                    { "returnQuoteAction", typeof(ReturnQuoteActionData) },
                    { "declineQuoteAction", typeof(DeclineQuoteActionData) },
                    { "createUserAction", typeof(CreateUserActionData) },
                    { "createQuoteAction", typeof(CreateQuoteActionData) },
                }),

            // Creating a new instance of AutomationDataConverter with a TypeMap configuration.
            // The TypeMap associates string keys with corresponding Type instances, defining the mapping for data conversion.
            new AutomationDataConverter(
                new TypeMap
                {
                    // Mapping for individual entities
                    { "user", typeof(ProviderEntity.User) },
                    { "quote", typeof(ProviderEntity.Quote) },
                    { "quoteVersion", typeof(ProviderEntity.QuoteVersion) },
                    { "policy", typeof(ProviderEntity.Policy) },
                    { "policyTransaction", typeof(ProviderEntity.PolicyTransaction) },
                    { "tenant", typeof(ProviderEntity.Tenant) },
                    { "product", typeof(ProviderEntity.Product) },
                    { "customer", typeof(ProviderEntity.Customer) },
                    { "emailMessage", typeof(ProviderEntity.EmailMessage) },
                    { "document", typeof(ProviderEntity.Document) },
                    { "claim", typeof(ProviderEntity.Claim) },
                    { "claimVersion", typeof(ProviderEntity.ClaimVersion) },
                    { "organisation", typeof(ProviderEntity.Organisation) },
                    { "portal", typeof(ProviderEntity.Portal) },
                    { "person", typeof(ProviderEntity.Person) },
                    { "performingUser", typeof(ProviderEntity.User) },
                    { "smsMessage", typeof(ProviderEntity.SmsMessage) },

                    // Mapping for entity lists
                    { "customers", typeof(EntityListReference<ProviderEntity.Customer>) },
                    { "quotes", typeof(EntityListReference<ProviderEntity.Quote>) },
                    { "policies", typeof(EntityListReference<ProviderEntity.Policy>) },
                    { "claims", typeof(EntityListReference<ProviderEntity.Claim>) },
                    { "emailMessages", typeof(EntityListReference<ProviderEntity.EmailMessage>) },
                    { "reports", typeof(EntityListReference<ProviderEntity.Report>) },
                    { "organisations", typeof(EntityListReference<ProviderEntity.Organisation>) },
                    { "users", typeof(EntityListReference<ProviderEntity.User>) },
                    { "roles", typeof(EntityListReference<ProviderEntity.Role>) },
                    { "portals", typeof(EntityListReference<ProviderEntity.Portal>) },
                    { "products", typeof(EntityListReference<ProviderEntity.Product>) },

                    // Mapping for additional entities
                    { "event", typeof(ProviderEntity.Event) },
                    { "productRelease", typeof(ProviderEntity.ProductRelease) },
                }),
        };

        DataSettings = new JsonSerializerSettings
        {
            Converters = DataConverters,
            DateParseHandling = DateParseHandling.None,
            TypeNameHandling = TypeNameHandling.All,
        };

        ModelSettings = new JsonSerializerSettings
        {
            Converters = Converters,
            DateParseHandling = DateParseHandling.None,
        };
    }

    /// <summary>
    /// Gets settings to use during deserialization of automation configuration json.
    /// </summary>
    public static JsonSerializerSettings ModelSettings { get; }

    /// <summary>
    /// Gets settings to use during deserialization of automation data json.
    /// </summary>
    public static JsonSerializerSettings DataSettings { get; }

    /// <summary>
    ///  Gets the type-map used for creating providers for schema objects meant to be supported by the schema reference #binary.
    /// </summary>
    public static TypeMap BinaryProviderTypeMap { get; }

    /// <summary>
    ///  Gets the type-map used for creating providers for schema objects meant to be supported by the schema reference #value.
    /// </summary>
    private static TypeMap GenericDataProviderTypeMap { get; }

    /// <summary>
    /// Gets the converters to be used when deserializing JSON strings of automation objects.
    /// </summary>
    private static JsonConverter[] Converters { get; }

    /// <summary>
    /// Gets the converters to be used when deserializing JSON for automation-data objects.
    /// </summary>
    private static JsonConverter[] DataConverters { get; }
}
