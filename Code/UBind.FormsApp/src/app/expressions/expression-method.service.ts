import { Injectable } from '@angular/core';
import { FormService } from '@app/services/form.service';
import { AttachmentService } from '@app/services/attachment.service';
import { WebhookService } from '@app/services/webhook.service';
import { AttachmentOperation } from '@app/operations/attachment.operation';
import { ApplicationService } from '@app/services/application.service';
import { UserService } from '@app/services/user.service';
import { QuoteState } from '@app/models/quote-state.enum';
import { ResumeApplicationService } from '@app/services/resume-application.service';
import { ExpressionMethodDependencyService } from './expression-method-dependency.service';
import { WorkflowStatusService } from '@app/services/workflow-status.service';
import { TriggerService } from '@app/services/trigger.service';
import { CurrencyHelper } from '@app/helpers/currency.helper';
import { ConfigService } from '@app/services/config.service';
import { ClaimState } from '@app/models/claim-state.enum';
import * as _ from 'lodash-es';
import { QuoteResult } from '@app/models/quote-result';
import { ClaimResult } from '@app/models/claim-result';
import { StringHelper } from '@app/helpers/string.helper';
import { PersonDetailModel } from '@app/models/person-detail.model';
import { TriggerDisplayConfig } from '@app/models/trigger-display-config';
import * as ChangeCase from "change-case";
import { titleCase } from "title-case";
import { Errors } from '@app/models/errors';
import { ExpressionInputSubjectService } from './expression-input-subject.service';
import { FieldFormatterResolver } from '@app/field-formatters/field-formatter-resolver';
import { NumberHelper } from '@app/helpers/number.helper';
import { FieldMetadataService } from '@app/services/field-metadata.service';
import { DataHelper } from '@app/helpers/data.helper';
import { SummaryTable } from '@app/summary-tables/summary-table';
import { SummaryTableHeaderRow } from '@app/summary-tables/summary-table-header-row';
import { SummaryTableRow } from '@app/summary-tables/summary-table-row';
import { SummaryTableCell } from '@app/summary-tables/summary-table-cell';
import { FieldDataType } from '@app/models/field-data-type.enum';
import { SummaryTableRepeatingEntry } from '@app/summary-tables/summary-table-repeating-entry';
import { MatchingFieldsSubjectService } from './matching-fields-subject.service';
import { QuestionMetadata } from '@app/models/question-metadata';
import { FieldPathHelper } from '@app/helpers/field-path.helper';
import { AttachmentFileProperties } from '@app/models/attachment-file-properties';
import { TextCase } from '@app/models/text-case.enum';
import Big from 'big.js';
import { SectionWidgetStatus } from '@app/components/widgets/section/section-widget-status';
import { SectionDisplayMode } from '@app/models/section-display-mode.enum';
import { RepeatingQuestionSetTrackingService } from '@app/services/repeating-question-set-tracking.service';
import { RepeatingFieldDisplayMode } from '@app/components/fields/repeating/repeating-field-display-mode.enum';
import { PremiumFundingHelper } from '@app/helpers/premium-funding.helper';
import { elevatedPermissionMapping, ElevatedPermission } from '@app/helpers/permissions.helper';
import { camelCaseTransformMerge, Options, pascalCaseTransformMerge } from 'change-case';
import { ContextEntityService } from "@app/services/context-entity.service";
import { WebFormEmbedOptions } from '@app/models/web-form-embed-options';
import { AttachmentHelper } from '@app/helpers/attachment.helper';
import { OperationStatusService } from '@app/services/operation-status.service';
import { LocaleService } from '@app/services/locale.service';

/**
 * Export expression method service class.
 * This class maange the expression method functions.
 */
@Injectable()
export class ExpressionMethodService implements SectionWidgetStatus {

    private cardTypes: any = {
        "VISA": {
            cardRegex: /^4[0-9]{12}(?:[0-9]{3})?$/,
            ccvRegex: /^(\d){3}$/,
        },
        "Mastercard": {
            cardRegex: /^(?:5[1-5][0-9]{2}|222[1-9]|22[3-9][0-9]|2[3-6][0-9]{2}|27[01][0-9]|2720)[0-9]{12}$/,
            ccvRegex: /^(\d){3}$/,
        },
        "AMEX": {
            cardRegex: /^3[47][0-9]{13}$/,
            ccvRegex: /^(\d){4}$/,
        },
        // eslint-disable-next-line @typescript-eslint/naming-convention
        "Diners Club": {
            cardRegex: /^3(?:0[0-5]|[68][0-9])[0-9]{11}$/,
            ccvRegex: /^(\d){3}$/,
        },
    };

    private deprecatedMessageMap: Map<string, string> = new Map<string, string>();

    /**
     * Needed only to ensure it's instantiated.
     */
    private expressionMethodDependencyService: ExpressionMethodDependencyService;

    public constructor(
        private applicationService: ApplicationService,
        private formService: FormService,
        private attachmentService: AttachmentService,
        private webhookService: WebhookService,
        private attachmentOperation: AttachmentOperation,
        private userService: UserService,
        private resumeApplicationService: ResumeApplicationService,
        expressionMethodDependencyService: ExpressionMethodDependencyService,
        private workflowStatusService: WorkflowStatusService,
        private triggerService: TriggerService,
        private configService: ConfigService,
        private expressionInputSubjectService: ExpressionInputSubjectService,
        private fieldFornatterResolver: FieldFormatterResolver,
        private fieldMetadataService: FieldMetadataService,
        private matchingFieldsSubjectService: MatchingFieldsSubjectService,
        private repeatingQuestionSetTrackingService: RepeatingQuestionSetTrackingService,
        private contextEntityService: ContextEntityService,
        private operationStatusService: OperationStatusService,
        private localeService: LocaleService,
    ) {
        this.expressionMethodDependencyService = expressionMethodDependencyService;
    }

    /** * String functions ***/

    public stringContains(hay: string, needle: string | RegExp): boolean {
        if (!hay) return false;
        const regexPattern: RegExp = needle instanceof RegExp ? needle : new RegExp(this.escapeRegExp(needle));

        return regexPattern.test(hay);
    }

    public substring(str: string, startPosition: number, endPosition?: number): string {
        if (!str) {
            throw new Error(`Value for 'str' cannot be empty.`);
        }

        return str.substring(startPosition, endPosition);
    }

    public substringNew(str: string, startPosition: number, endPosition?: number): string {
        if (!str) {
            throw new Error(`Value for 'str' cannot be empty.`);
        }

        return str.substring(startPosition, endPosition);
    }

    public indexOf(str: string, searchString: string): number {
        if (!str) {
            throw new Error(`Value for 'str' cannot be empty.`);
        } else if (!searchString) {
            throw new Error(`Value for 'searchString' cannot be empty.`);
        } else if (searchString.length > str.length) {
            throw new Error(`Length of 'searchString' cannot be greater than the length of 'str'.`);
        }
        return str.indexOf(searchString);
    }

    public generatePremiumFundingContractPdfUrl(contractId: string, pdfKey: string): string {
        return PremiumFundingHelper.getPremiumFundingContractPdfUrl(contractId, pdfKey);
    }

    public length(str: string): number {
        return str.length;
    }

    /**
     * @deprecated Please use toLowerCase() instead.
     */
    public lowerCase(str: string): string {
        this.showDeprecatedMessageIfNeverShown('lowerCase', 'Please use toLowerCase() instead.');
        return this.toLowerCase(str);
    }

    /**
     * @deprecated Please use toUpperCase() instead.
     */
    public upperCase(str: string): string {
        this.showDeprecatedMessageIfNeverShown('upperCase', 'Please use toUpperCase() instead.');
        return this.toUpperCase(str);
    }

    public toUpperCase(str: string, inputCase: string = 'default'): string {
        if (!str) {
            return str;
        }

        let input: string = StringHelper.toInputCase(str, inputCase);
        return input.toLocaleUpperCase();
    }

    public toLowerCase(str: string, inputCase: string = 'default'): string {
        if (!str) {
            return str;
        }

        let input: string = StringHelper.toInputCase(str, inputCase);
        return input.toLocaleLowerCase();
    }

    public toTitleCase(str: string, inputCase: string = 'default'): string {
        if (!str) {
            return str;
        }

        let input: string = StringHelper.toInputCase(str, inputCase);
        if (StringHelper.isSingleWord(input)) {
            input = input.toLowerCase();
        }

        return titleCase(input);
    }

    public toCapitalCase(str: string, inputCase: string = 'default'): string {
        if (!str) {
            return str;
        }

        let options: Options = StringHelper.toInputCaseOptions(inputCase);
        let input: string = StringHelper.toInputCase(str, inputCase);
        if (StringHelper.isSingleWord(input)) {
            input = input.toLowerCase();
        }

        return ChangeCase.capitalCase(input, options);
    }

    public toSentenceCase(str: string, inputCase: string = 'default'): string {
        if (!str) {
            return str;
        }

        let options: Options = StringHelper.toInputCaseOptions(inputCase);
        let input: string = StringHelper.toInputCase(str, inputCase);
        if (StringHelper.isSingleWord(input)) {
            input = input.toLowerCase();
        }

        return ChangeCase.sentenceCase(input, options);
    }

    public toCamelCase(str: string, inputCase: string = 'default'): string {
        if (!str) {
            return str;
        }

        let options: Options = StringHelper.toInputCaseOptions(inputCase);
        options.transform = camelCaseTransformMerge;
        let input: string = StringHelper.toInputCase(str, inputCase);
        if (StringHelper.isSingleWord(input)) {
            input = input.toLowerCase();
        }

        return ChangeCase.camelCase(input, options);
    }

    public toPascalCase(str: string, inputCase: string = 'default'): string {
        if (!str) {
            return str;
        }

        let options: Options = StringHelper.toInputCaseOptions(inputCase);
        options.transform = pascalCaseTransformMerge;
        let input: string = StringHelper.toInputCase(str, inputCase);
        if (StringHelper.isSingleWord(input)) {
            input = input.toLowerCase();
        }

        return ChangeCase.pascalCase(input, options);
    }

    public toKebabCase(str: string, inputCase: string = 'default'): string {
        if (!str) {
            return str;
        }

        let options: Options = StringHelper.toInputCaseOptions(inputCase);
        let input: string = StringHelper.toInputCase(str, inputCase);
        if (StringHelper.isSingleWord(input)) {
            input = input.toLowerCase();
        }

        return ChangeCase.paramCase(input, options);
    }

    public toSnakeCase(str: string, inputCase: string = 'default'): string {
        if (!str) {
            return str;
        }

        let options: Options = StringHelper.toInputCaseOptions(inputCase);
        let input: string = StringHelper.toInputCase(str, inputCase);
        if (StringHelper.isSingleWord(input)) {
            input = input.toLowerCase();
        }

        return ChangeCase.snakeCase(input, options);
    }

    public join(strings: Array<string>, glue?: string): string {
        let values: Array<string> = strings.filter((value: string) => !StringHelper.isNullOrWhitespace(value));
        return values.join(glue ? glue : '');
    }

    /** * Number functions ***/

    public number(numberString: string): number {
        if (!numberString) {
            return 0;
        }

        const convertedNumber: number = Number(numberString);
        if (isNaN(convertedNumber)) {
            return Number(numberString.replace(/[^0-9.-]+/g, ''));
        } else {
            return convertedNumber;
        }
    }

    public wholeNumber(str: any): number {
        return parseInt(str, 10);
    }

    public decimalNumber(str: any): number {
        if (str && !isNaN(str)) {
            return parseFloat(str);
        }
        return 0;
    }

    public isWholeNumber(str: any): boolean {
        let regExp: RegExp = /^[0-9]+$/i;
        return regExp.test(str);
    }

    public isDecimalNumber(str: any): boolean {
        return !isNaN(parseFloat(str));
    }

    /**
     * Formats a number according to the users local, e.g. by adding in commas for thousands separators.
     * @param number the number to format.
     */
    public formatNumber(number: number): string {
        let locale: string = this.localeService.getCurrencyLocale();
        return NumberHelper.format(number, null, locale);
    }

    /** * Currency functions ***/
    public currencyAsString(
        value: any,
        includeMinorUnits: boolean | null,
        currencyCode: string | null,
    ): string {
        if (isNaN(value)) {
            return '';
        }
        if (value == null || value == '') {
            value = 0;
        }
        if (!currencyCode) {
            currencyCode = this.getCurrencyCode();
        }

        let isWholeNumber: boolean = value % 1 == 0;
        let digitsInfo: string = includeMinorUnits == null
            ? isWholeNumber ? '1.0-0' : '1.2-2'
            : includeMinorUnits == false ? '1.0-0' : '1.2-2';

        const locale: string = this.localeService.initialiseOrGetCurrencyLocale(currencyCode);

        return CurrencyHelper.format(value, currencyCode, 'symbol-narrow', digitsInfo, locale);
    }

    /**
     * convert number into words.
     * @param value the number to be converted as word.
     * @param includeMinorUnits the indicator if minor units will be included when the value has minor units
     * @param currencyCode the currency code, when null or not specified it will used the default currency code.
     */
    public currencyAsSentence(
        value: number,
        includeMinorUnits: boolean = true,
        currencyCode: string = null,
    ): string {
        if (isNaN(value)) {
            return '';
        }

        value = value ? value : 0;

        if (!currencyCode) {
            currencyCode = this.getCurrencyCode();
        }

        const locale: string = this.localeService.initialiseOrGetCurrencyLocale(currencyCode);

        return new CurrencyHelper().convertNumberToWords(
            value,
            currencyCode,
            includeMinorUnits,
            locale,
        );
    }

    public getCurrencyCode(): string {
        // TODO: instead of having a hard coded field name "currencyCode" 
        // use a workflow role or QuoteDataLocator.
        let currencyCode: string = 'AUD';
        let model: object = this.formService.getValues();
        if (model['currencyCode']) {
            currencyCode = model['currencyCode'];
        } else if (this.configService.settings?.financial?.defaultCurrency) {
            currencyCode = this.configService.settings.financial.defaultCurrency;
        }
        return currencyCode;
    }

    /** * Date functions ***/

    private getMillis(dateOrMillis: string | number): number {
        return typeof dateOrMillis === 'string'
            ? this.date(dateOrMillis)
            : dateOrMillis;
    }

    /**
     * Checks if the given date string is a valid date in the format "DD/MM/YYYY".
     * @param dateStr The date string to be validated.
     * @returns `true` if the date string is a valid date; otherwise, `false`.
     */
    public isDate(dateStr: string): boolean {
        if (/^[0-9]{2}\/[0-9]{2}\/[0-9]{4}$/.test(dateStr)) {
            let splitValues: any = dateStr.split('/');
            splitValues[1]--; // correct month value
            let dateObject: Date = new Date(splitValues[2], splitValues[1], splitValues[0]);
            let dateString: string = dateObject.toString();
            if (dateString != 'Invalid Date' &&
                dateString != 'NaN' &&
                dateObject.getDate() == splitValues[0] &&
                dateObject.getMonth() == splitValues[1] &&
                dateObject.getFullYear() == splitValues[2]) {
                return true;
            }
        }
        return false;
    }

    /**
     * Checks if the given date string corresponds to a weekday (Monday to Friday).
     * @param millis The input date in milliseconds.
     * @returns `true` if the date string corresponds to a weekday; otherwise, `false`.
     */
    public isWeekday(dateOrMillis: string | number): boolean {
        let weekdays: Array<number> = [1, 2, 3, 4, 5];
        let date: Date = new Date(this.getMillis(dateOrMillis));
        if (weekdays.indexOf(date.getDay()) != -1) {
            return true;
        }
        return false;
    }

    /**
     * Checks if the given date string corresponds to a weekend (Saturday or Sunday).
     * @param millis The input date in milliseconds.
     * @returns `true` if the date string corresponds to a weekend; otherwise, `false`.
     */
    public isWeekend(dateOrMillis: string | number): boolean {
        let weekendDays: Array<number> = [0, 6];
        let date: Date = new Date(this.getMillis(dateOrMillis));
        if (weekendDays.indexOf(date.getDay()) != -1) {
            return true;
        }
        return false;
    }

    /**
     * Converts a date in milliseconds to a date string in the format "DD/MM/YYYY".
     * @param millis The timestamp to be converted to a date string.
     * @returns The date string representation of the timestamp in the format "DD/MM/YYYY".
     */
    public dateAsString(millis: number): string {
        let date: Date = new Date(millis);
        let day: number = date.getDate();
        let month: number = date.getMonth() + 1;
        let dayStr: string = String(day).padStart(2, '0');
        let monthStr: string = String(month).padStart(2, '0');
        let yearStr: string = '' + date.getFullYear();
        return dayStr + '/' + monthStr + '/' + yearStr;
    }

    /**
     * Parses the given date string into milliseconds
     * @param dateStr The input date string in dd/mm/yyyy format.
     * @returns The milliseconds extracted from the input date/milliseconds.
     */
    public date(dateStr: string): number {
        if (!/^[0-9]{2}\/[0-9]{2}\/[0-9]{4}$/.test(dateStr)) {
            dateStr = this.dateAsString(parseInt(dateStr, 10));
            if (!/^[0-9]{2}\/[0-9]{2}\/[0-9]{4}$/.test(dateStr)) {
                throw new Error('Error trying to parse this string as a date: "' + dateStr + '" . '
                    + 'The date string must be in the format "DD/MM/YYYY". '
                    + 'Before attempting to use a field value as a date, in the expression '
                    + 'please first check that the date is valid, e.g. "isDate(fieldValue) && ..."');
            }
        }
        let [day, month, year]: Array<number> = dateStr.split('/').map(Number);
        return new Date(year, month - 1, day).getTime();
    }

    /**
     * Calculates the age based on the given date of birth in milliseconds.
     * @param dateOfBirthMillis The date of birth in milliseconds.
     * @returns The age as a number of years.
     */
    public getAgeFromDateOfBirth(dateOrMillisOfBirth: number | string): number {
        let dateOfBirth: Date = new Date(this.getMillis(dateOrMillisOfBirth));
        let now: Date = new Date();
        let years: number = now.getFullYear() - dateOfBirth.getFullYear();
        if (now.getMonth() < dateOfBirth.getMonth() ||
            (now.getMonth() == dateOfBirth.getMonth() &&
                now.getDate() < dateOfBirth.getDate())) {
            years--;
        }
        return years;
    }

    /**
     * Gets the current date as the number of milliseconds
     */
    public now(): number {
        let now: Date = new Date();
        return now.getTime();
    }

    /**
     * Gets the current date for the start of the day (midnight) as the number of milliseconds.
     */
    public today(): number {
        let now: Date = new Date();
        return new Date(now.getFullYear(), now.getMonth(), now.getDate()).getTime();
    }

    /**
     * Gets the current date for the start of the month as the number of milliseconds.
     */
    public thisMonth(): number {
        let now: Date = new Date();
        return new Date(now.getFullYear(), now.getMonth(), 1).getTime();
    }

    /**
     * Gets the current date for the start of the year as the number of milliseconds.
     */
    public thisYear(): number {
        let now: Date = new Date();
        return new Date(now.getFullYear(), 1, 1).getTime();
    }

    /**
     * Get the year from a date string or milliseconds.
     * @param dateOrMillis The input date, which can be either a date string (dd/mm/yyyy) or milliseconds.
     * @returns The year extracted from the input date.
     */
    public getYear(dateOrMillis: string | number): number {
        let fullDate: Date = new Date(this.getMillis(dateOrMillis));
        return fullDate.getFullYear();
    }

    /**
     * Get the month from a date string or milliseconds.
     * @param dateOrMillis The input date, which can be either a date string (dd/mm/yyyy) or milliseconds.
     * @returns The month extracted from the input date.
     */
    public getMonth(dateOrMillis: string | number): number {
        let fullDate: Date = new Date(this.getMillis(dateOrMillis));
        return fullDate.getMonth() + 1;
    }

    /**
     * Get the day from a date string or milliseconds.
     * @param dateOrMillis The input date, which can be either a date string (dd/mm/yyyy) or milliseconds.
     * @returns The day extracted from the input date.
     */
    public getDate(dateOrMillis: string | number): number {
        let fullDate: Date = new Date(this.getMillis(dateOrMillis));
        return fullDate.getDate();
    }

    /**
     * Evaluates the date if it is in the past.
     * @param dateOrMillis The input date, which can be either a date string (dd/mm/yyyy) or milliseconds.
     * @returns True if the date is in the past, otherwise false.
     */
    public inThePast(dateOrMillis: string | number): boolean {
        let now: Date = new Date();
        let millis: number = this.getMillis(dateOrMillis);
        return millis < new Date(now.getFullYear(), now.getMonth(), now.getDate()).getTime();
    }

    /**
     * Evaluates the date if it is in the future. 
     * @param dateOrMillis The input date, which can be either a date string (dd/mm/yyyy) or milliseconds.
     * @returns True if the date is in the future, otherwise false. 
     */
    public inTheFuture(dateOrMillis: string | number): boolean {
        let now: Date = new Date();
        let millis: number = this.getMillis(dateOrMillis);
        return millis >= new Date(now.getFullYear(), now.getMonth(), now.getDate() + 1).getTime();
    }

    /**
     * Evaluates the date if it is within the next years based on current date + the years provided.
     * @param dateOrMillis The input date, which can be either a date string (dd/mm/yyyy) or milliseconds.
     * @param years The number of years in the future against which to compare the input date.
     * @returns True if the date is within the next years based on current date + the years provided,
     * otherwise false.  
     */
    public inTheNextYears(dateOrMillis: string | number, years: number): boolean {
        let now: Date = new Date();
        let currentDate: number = new Date(now.getFullYear(), now.getMonth(), now.getDate()).getTime();
        let futureDate: number = new Date(now.getFullYear() + years, now.getMonth(), now.getDate()).getTime();
        let millis: number = this.getMillis(dateOrMillis);
        return millis >= currentDate && millis < futureDate;
    }

    /**
     * Evaluates the date if it is within the next months based on current date + months provided.
     * @param dateOrMillis The input date, which can be either a date string (dd/mm/yyyy) or milliseconds.
     * @param months The number of months in the future against which to compare the input date.
     * @returns True if the date is is within the next months based on current date + months provided.
     * otherwise false.
     */
    public inTheNextMonths(dateOrMillis: string | number, months: number): boolean {
        const now: Date = new Date();
        const currentYear: number = now.getFullYear();
        const currentMonth: number = now.getMonth();
        const currentDateMillis: number = new Date(currentYear, currentMonth, now.getDate()).getTime();
        const futureDateMillis: number = new Date(currentYear, currentMonth + months, now.getDate()).getTime();
        const targetMillis: number = this.getMillis(dateOrMillis);
        return targetMillis >= currentDateMillis && targetMillis <= futureDateMillis;
    }

    /**
     * Evaluates the date if it is within the next days based on current date + days provided
     * @param dateOrMillis The input date, which can be either a date string (dd/mm/yyyy) or milliseconds.
     * @param days The number of days in the future against which to compare the input date.
     * @returns True if it is within the next days based on current date + days provided, otherwise false.
     */
    public inTheNextDays(dateOrMillis: string | number, days: number): boolean {
        let now: Date = new Date();
        let currentDate: number = new Date(now.getFullYear(), now.getMonth(), now.getDate()).getTime();
        let futureDate: number = new Date(now.getFullYear(), now.getMonth(), now.getDate() + days).getTime();
        let millis: number = this.getMillis(dateOrMillis);
        return millis >= currentDate && millis < futureDate;
    }

    /**
     * Evaluates the date if it is within the last years based on current date - years provided.
     * @param dateOrMillis The input date, which can be either a date string (dd/mm/yyyy) or milliseconds.
     * @param years The number of years in the past against which to compare the input date.
     * @returns True if it is within the last years based on current date - years provided,
     * otherwise returns false.
     */
    public inTheLastYears(dateOrMillis: string | number, years: number): boolean {
        let now: Date = new Date();
        let currentDate: number = new Date(now.getFullYear(), now.getMonth(), now.getDate()).getTime();
        let pastDate: number = new Date(now.getFullYear() - years, now.getMonth(), now.getDate()).getTime();
        let millis: number = this.getMillis(dateOrMillis);
        return millis <= currentDate && millis > pastDate;
    }

    /**
     * Evaluates the date if it is within the last months based on current date - months provided.
     * @param dateOrMillis The input date, which can be either a date string (dd/mm/yyyy) or milliseconds.
     * @param months The number of months in the past against which to compare the input date.
     * @returns True if it is within the last months based on current date - years provided,
     * otherwise returns false.
     */
    public inTheLastMonths(dateOrMillis: string | number, months: number): boolean {
        let now: Date = new Date();
        let currentDate: number = new Date(now.getFullYear(), now.getMonth(), now.getDate()).getTime();
        let pastDate: number = new Date(now.getFullYear(), now.getMonth() - months, now.getDate()).getTime();
        let millis: number = this.getMillis(dateOrMillis);
        return millis <= currentDate && millis > pastDate;
    }

    /**
     * Evaluates the date if it is witihin the last days based on current date - months provided.
     * @param dateOrMillis The input date, which can be either a date string (dd/mm/yyyy) or milliseconds.
     * @param days The number of days in the past against which to compare the input date.
     * @returns True if the dateOrMillis is within the last years based on current date - years provided,
     * otherwise returns false.
     */
    public inTheLastDays(dateOrMillis: string | number, days: number): boolean {
        let now: Date = new Date();
        let currentDate: number = new Date(now.getFullYear(), now.getMonth(), now.getDate()).getTime();
        let pastDate: number = new Date(now.getFullYear(), now.getMonth(), now.getDate() - days).getTime();
        let millis: number = this.getMillis(dateOrMillis);
        return millis <= currentDate && millis > pastDate;
    }

    /**
     * Determines if a provided date is at least a certain number of years in the future.
     * @param dateOrMillis The input date, which can be either a date string (dd/mm/yyyy) or milliseconds.
     * @param years The number of years in the future against which to compare the input date.
     * @returns Returns true if the input date falls on or after the date exactly 'years' years from now.
     * Otherwise, it returns false.
     */
    public atLeastYearsInTheFuture(dateOrMillis: string | number, years: number): boolean {
        let now: Date = new Date();
        let futureDate: number = new Date(now.getFullYear() + years, now.getMonth(), now.getDate()).getTime();
        return this.getMillis(dateOrMillis) >= futureDate;
    }

    /**
     * Determines if a provided date is at least a certain number of months in the future.
     * @param dateOrMillis The input date, which can be either a date string (dd/mm/yyyy) or milliseconds.
     * @param months The number of months in the future against which to compare the input date.
     * @returns True if the input date falls on or after the date exactly 'months' months from now.
     * Otherwise, it returns false.
     */
    public atLeastMonthsInTheFuture(dateOrMillis: string | number, months: number): boolean {
        let now: Date = new Date();
        let futureDate: number = new Date(now.getFullYear(), now.getMonth() + months, now.getDate()).getTime();
        return this.getMillis(dateOrMillis) >= futureDate;
    }

    /**
     * Determines if a provided date is at least a certain number of days in the future.
     * @param dateOrMillis The input date, which can be either a date string (dd/mm/yyyy) or milliseconds.
     * @param days - The number of days in the future against which to compare the input date.
     * @returns True if the input date falls on or after the date exactly 'days' days from now.
     * Otherwise it returns false.
     */
    public atLeastDaysInTheFuture(dateOrMillis: string | number, days: number): boolean {
        let now: Date = new Date();
        let futureDate: number = new Date(now.getFullYear(), now.getMonth(), now.getDate() + days).getTime();
        return this.getMillis(dateOrMillis) >= futureDate;
    }

    /**
     * Determines if a provided date is at most a certain number of years, months or days in the future.
     * @param dateOrMillis The input date, which can be either a date string (dd/mm/yyyy) or milliseconds.
     * @param years - The number of years in the future against which to compare the input date.
     * @returns True if the date provided is at most years in the future based on current date + years provided,
     * otherwise returns false.
     */
    public atMostYearsInTheFuture(dateOrMillis: string | number, years: number): boolean {
        let now: Date = new Date();
        let futureDate: number = new Date(now.getFullYear() + years, now.getMonth(), now.getDate()).getTime();
        return this.getMillis(dateOrMillis) <= futureDate;
    }

    /**
     * Determines if a provided date is at most a certain number of months in the future.
     * @param dateOrMillis The input date, which can be either a date string (dd/mm/yyyy) or milliseconds.
     * @param months - The number of months in the future against which to compare the input date.
     * @returns True if the date provided is at most months in the future based on current date + months provided,
     * otherwise returns false.
     */
    public atMostMonthsInTheFuture(dateOrMillis: string | number, months: number): boolean {
        let now: Date = new Date();
        let futureDate: number = new Date(now.getFullYear(), now.getMonth() + months, now.getDate()).getTime();
        return this.getMillis(dateOrMillis) <= futureDate;
    }

    /**
     * Determines if a provided date is at most a certain number of days in the future.
     * @param dateOrMillis The input date, which can be either a date string (dd/mm/yyyy) or milliseconds.
     * @param days - The number of days in the future against which to compare the input date.
     * @returns true if the date provided is at most days in the future based on current date + days provided,
     * otherwise returns false.
     */
    public atMostDaysInTheFuture(dateOrMillis: string | number, days: number): boolean {
        let now: Date = new Date();
        let futureDate: number = new Date(now.getFullYear(), now.getMonth(), now.getDate() + days).getTime();
        return this.getMillis(dateOrMillis) <= futureDate;
    }

    /**
     * Determines if a provided date is at least a certain number of years in the past.
     * @param dateOrMillis The input date, which can be either a date string (dd/mm/yyyy) or milliseconds.
     * @param years - The number of years in the past against which to compare the input date.
     * @returns True if the date provided is at least years in the past based on current date - years provided,
     * otherwise returns false.
     */
    public atLeastYearsInThePast(dateOrMillis: string | number, years: number): boolean {
        let now: Date = new Date();
        let pastDate: number = new Date(now.getFullYear() - years, now.getMonth(), now.getDate()).getTime();
        return this.getMillis(dateOrMillis) <= pastDate;
    }

    /**
     * Determines if a provided date is at least a certain number of months in the past.
     * @param dateOrMillis The input date, which can be either a date string (dd/mm/yyyy) or milliseconds.
     * @param months The number of months in the past against which to compare the input date.
     * @returns True if the date provided is at least months in the past based on current date - months provided,
     * otherwise returns false.
     */
    public atLeastMonthsInThePast(dateOrMillis: string | number, months: number): boolean {
        let now: Date = new Date();
        let pastDate: number = new Date(now.getFullYear(), now.getMonth() - months, now.getDate()).getTime();
        return this.getMillis(dateOrMillis) <= pastDate;
    }

    /**
     * Determines if a provided date is at least a certain number of days in the past.
     * @param dateOrMillis The input date, which can be either a date string (dd/mm/yyyy) or milliseconds.
     * @param days the number of days in the past against which to compare the input date.
     * @returns True if the date provided is at least days in the past based on current date - days provided,
     * otherwise returns false.
     */
    public atLeastDaysInThePast(dateOrMillis: string | number, days: number): boolean {
        let now: Date = new Date();
        let pastDate: number = new Date(now.getFullYear(), now.getMonth(), now.getDate() - days).getTime();
        return this.getMillis(dateOrMillis) <= pastDate;
    }

    /**
     * Determines if a provided date is at most a certain number of years in the past.
     * @param dateOrMillis The input date, which can be either a date string (dd/mm/yyyy) or milliseconds.
     * @param years the number of years in the past against which to compare the input date.
     * @returns true if the date provided is at most years in the past based on current date - years provided,
     * otherwise returns false.
     */
    public atMostYearsInThePast(dateOrMillis: string | number, years: number): boolean {
        let now: Date = new Date();
        let pastDate: number = new Date(now.getFullYear() - years, now.getMonth(), now.getDate()).getTime();
        return this.getMillis(dateOrMillis) >= pastDate;
    }

    /**
     * Determines if a provided date is at most a certain number of months in the past.
     * @param dateOrMillis The input date, which can be either a date string (dd/mm/yyyy) or milliseconds.
     * @param months the number of months in the past against which to compare the input date.
     * @returns true if the date provided is at most months in the past based on current date - months provided,
     * otherwise returns false.
     */
    public atMostMonthsInThePast(dateOrMillis: string | number, months: number): boolean {
        let now: Date = new Date();
        let pastDate: number = new Date(
            now.getFullYear(), now.getMonth() - months, now.getDate()).getTime();
        return this.getMillis(dateOrMillis) >= pastDate;
    }

    /**
     * Determines if a provided date is at most a certainnumber of days in the past.
     * @param dateOrMillis The input date, which can be either a date string (dd/mm/yyyy) or milliseconds.
     * @param days the number of days in the past against which to compare the input date.
     * @returns true if the date provided is at most days in the past based on current date - days provided,
     * otherwise returns false.
     */
    public atMostDaysInThePast(dateOrMillis: string | number, days: number): boolean {
        let now: Date = new Date();
        let pastDate: number = new Date(
            now.getFullYear(), now.getMonth(), now.getDate() - days).getTime();
        return this.getMillis(dateOrMillis) >= pastDate;
    }

    /**
     * Adds a certain number of years to a provided date and returns the resulting date in milliseconds.
     * @param dateOrMillis The input date, which can be either a date string (dd/mm/yyyy) or milliseconds.
     * @param years the number of years to be added in the input date.
     * @returns The milliseconds value of the date + years provided.
     */
    public addYears(dateOrMillis: string | number, years: number): number {
        let date: Date = new Date(this.getMillis(dateOrMillis));
        return new Date(date.getFullYear() + years, date.getMonth(), date.getDate()).getTime();
    }

    /**
     * Adds a certain number of months to a provided date and returns the resulting date in milliseconds.
     * @param dateOrMillis The input date, which can be either a date string (dd/mm/yyyy) or milliseconds.
     * @param months the number of months to be added in the input date.
     * @returns The milliseconds value of the date + months provided.
     */
    public addMonths(dateOrMillis: string | number, months: number): number {
        let date: Date = new Date(this.getMillis(dateOrMillis));
        return new Date(date.getFullYear(), date.getMonth() + months, date.getDate()).getTime();
    }

    /**
     * Adds a certain number of days to a provided date and returns the resulting date in milliseconds.
     * @param dateOrMillis The input date, which can be either a date string (dd/mm/yyyy) or milliseconds.
     * @param days the number of days to be added in the input date.
     * @returns The milliseconds value of the date + days provided.
     */
    public addDays(dateOrMillis: string | number, days: number): number {
        let date: Date = new Date(this.getMillis(dateOrMillis));
        return new Date(date.getFullYear(), date.getMonth(), date.getDate() + days).getTime();
    }

    /** * Repeating Field Sets ***/

    public sum(values: Array<any>): number {
        let returnValue: Big = new Big(0.0);
        values.forEach((value: any) => {
            if (value && !isNaN(Number(value))) {
                returnValue = new Big(returnValue).add(value);
            }
        });

        return returnValue.toNumber();
    }

    /**
     * @deprecated this will never be called by an expression because the expression parser will replace
     * it with sum(getFieldValuesForFieldPathPattern(\'xxx[*].yyy\'))
     */
    public sumRepeating(repeatingQuestionSetModel: any, property: string): number {
        let returnValue: number = 0;
        try {
            for (let item of repeatingQuestionSetModel) {
                if (item[property] != null) {
                    if (item[property] != null && /^[0-9]+$/.test(item[property])) {
                        returnValue += parseInt(item[property], 10);
                    } else {
                        if (!isNaN(parseFloat(item[property]))) {
                            returnValue += parseFloat(item[property]);
                        }
                    }
                }
            }
        } catch (error) {
            throw new Error('Unable to calculate sum of property "' +
                property + '" on repeating question set "' + repeatingQuestionSetModel);
        }
        return returnValue;
    }

    /**
     * This method would never be called directly since the expression itself would maintain this value directly.
     * @param fieldPath 
     */
    public countRepeating(fieldPath: string): number {
        throw new Error("ExpressionMethodService.countRepeating should not be called directly since "
            + "the expression itself would maintain this value directly.");
        // return this.expressionInputSubjectService.getFieldRepeatingCountSubject(fieldPath).value;
    }

    public getRepeatingIndex(fieldKey: string): number {
        let returnValue: number = null;
        let regex: RegExp = /^([a-zA-Z0-9]+)\[([0-9]+)\](\.([a-zA-Z0-9]+))?$/;
        let match: RegExpMatchArray = fieldKey.match(regex);
        if (match) {
            returnValue = parseInt(match[2], 10);
        }
        return returnValue;
    }

    public getRepeatingSetName(fieldKey: string): string {
        let returnValue: string = '';
        let regex: RegExp = /^([a-zA-Z0-9]+)\[([0-9]+)\]\.([a-zA-Z0-9]+)$/;
        let match: RegExpMatchArray = fieldKey.match(regex);
        if (match) {
            returnValue = match[1];
        }
        return returnValue;
    }

    public getRepeatingValue(fieldKey: string, fieldName: string): number {
        throw new Error("ExpressionMethodService.getRepeatingValue should "
            + "not be called directly since the expression itself should "
            + "replace this with getRelativeFieldValue() as part of a "
            + "backwards compatibility replacement.");
    }

    public getRelativeFieldValue(fieldPath: string, jsonPointer: string): any {
        throw new Error("ExpressionMethodService.getRelativeFieldValue should not be called directly since the "
            + "expression itself should resolve the relativate field path during pre-processing");
    }

    public getRelativeFieldPath(fieldPath: string, jsonPointer: string): string {
        throw new Error("ExpressionMethodService.getRelativeFieldPath should not be called directly since the "
            + "expression itself should resolve the relativate field path during pre-processing");
    }

    /**
     * @returns the value of the property of the fields additional data, or an empty string if not found.
     * @param fieldPath 
     * @param propertyName 
     */
    public getFieldPropertyValue(fieldPath: string, propertyName: string): any {
        let data: any = this.formService.getFieldData(fieldPath);
        if (!data || !(typeof (data) === 'object')
            || data[propertyName] === undefined || data[propertyName] === null
        ) {
            return '';
        }
        return data[propertyName];
    }

    /**
     * 
     * @param fieldPath The field key
     * @returns The data associated with a field, if any.
     */
    public getFieldData(fieldPath: string): any {
        return this.formService.getFieldData(fieldPath);
    }

    /** * Maths ***/

    public max(/* args*/): number {
        if (Array.isArray(arguments[0])) {
            return Math.max(...arguments[0]);
        }
        return Math.max(...arguments);
    }

    public min(/* args*/): number {
        if (Array.isArray(arguments[0])) {
            return Math.min(...arguments[0]);
        }
        return Math.min(...arguments);
    }

    /** * Operations ***/

    public operationStatus(operationName: any): any {
        let status: string = this.applicationService.operationStatuses.get(operationName);
        return status;
    }

    public operationHasError(operationName: any): boolean {
        let status: string = this.applicationService.operationStatuses.get(operationName);
        if (status && status['errors'] && status['errors'].length > 0) {
            return true;
        } else if (status && status['Error'] && status['Error'] == "UBindServerError") {
            return true;
        } else {
            return false;
        }
    }

    public operationErrorCode(operationName: any): string {
        // note, this method might be unnecessary, 
        // as the errors may only be sent as message string, not codes
        let status: string = this.applicationService.operationStatuses.get(operationName);
        if (status && status['errors'] && status['errors'].length > 0) {
            return status['errors'][0];
        } else if (status && status['Error'] && status['Error'] == "UBindServerError") {
            return status['Error'];
        } else {
            return '';
        }
    }

    public operationErrorMessage(operationName: any): string {
        let status: string = this.applicationService.operationStatuses.get(operationName);
        if (status && status['errors'] && status['errors'].length > 0) {
            return status['errors'][0];
        } else if (status && status['Error'] && status['Error'] == "UBindServerError") {
            return 'The server was unable to complete your request. Please try again later.';
        } else {
            return '';
        }
    }

    /** * Calculations ***/

    public getQuoteResult(): QuoteResult {
        return this.applicationService.latestQuoteResult;
    }

    public getClaimResult(): ClaimResult {
        return this.applicationService.latestClaimResult;
    }

    /**
     * @deprecated use getQuoteResult() instead.
     */
    public getCalculationResult(): QuoteResult {
        this.showDeprecatedMessageIfNeverShown('getCalculationResult', 'You can use getQuoteResult() instead.');
        return this.getQuoteResult();
    }

    /**
     * @deprecated use getQuoteResult() instead.
     */
    public quoteStatus(): QuoteResult {
        this.showDeprecatedMessageIfNeverShown('quoteStatus', 'Please use getQuoteResult() instead.');
        return this.getQuoteResult();
    }

    /**
     * Alias: quoteState(): string
     */
    public getCalculationQuoteState(): string {
        this.showDeprecatedMessageIfNeverShown(
            'getCalculationQuoteState', 'Please use getQuoteCalculationState() instead.');
        return this.applicationService.latestQuoteResult?.calculationState;
    }

    public getQuoteCalculationState(): string {
        return this.applicationService.latestQuoteResult?.calculationState;
    }

    public getCalculationClaimState(): string {
        this.showDeprecatedMessageIfNeverShown(
            'getCalculationClaimState', 'Please use getClaimCalculationState() instead.');
        return this.getClaimCalculationState();
    }

    public getClaimCalculationState(): string {
        return this.applicationService.latestClaimResult?.calculationState;
    }


    /**
     * @deprecated please use getCalculationQuoteState() instead.
     */
    public quoteState(): string {
        this.showDeprecatedMessageIfNeverShown('quoteState', 'Please use getQuoteCalculationState() instead.');
        return this.getQuoteCalculationState();
    }

    public getCalculationApplicationState(): string {
        return this.applicationService.latestQuoteResult?.calculationState;
    }

    /**
     * @deprecated please use hasActiveTriggerOfType(triggerType) instead.
     */
    public calculationHasActiveTriggerOfType(triggerType: string): boolean {
        this.showDeprecatedMessageIfNeverShown(
            'calculationHasActiveTriggerOfType',
            'Please use hasActiveTriggerOfType() instead.');
        return this.hasActiveTriggerOfType(triggerType);
    }

    public hasActiveTriggerOfType(triggerType: string): boolean {
        if (triggerType == 'any') {
            return this.hasActiveTrigger();
        }
        const activeTrigger: any = this.getActiveTriggerByType(triggerType);
        if (activeTrigger) {
            return true;
        }
        return false;
    }

    public hasActiveTrigger(): boolean {
        let result: boolean = (this.applicationService.latestQuoteResult != undefined
            && this.applicationService.latestQuoteResult.trigger != null)
            || (this.applicationService.latestClaimResult != undefined
                && this.applicationService.latestClaimResult.trigger != null);
        return result;
    }

    public getActiveTrigger(): TriggerDisplayConfig {
        return this.triggerService.activeTrigger;
    }

    public getActiveTriggerType(): string {
        return this.triggerService.activeTrigger?.type;
    }

    public getActiveTriggerByType(triggerType: string): any {
        return this.triggerService.getFirstActiveTriggerByType(triggerType);
    }

    /**
     * Alias: calculationResult(path: string): any
     * @deprecated use getQuoteResultByPath()
     */
    public getCalculationResultByPath(path: string): any {
        this.showDeprecatedMessageIfNeverShown(
            'getCalculationResultByPath',
            'Please use getQuoteResultByPath(...) instead.');
        return this.getQuoteResultByPath(path);
    }

    /**
     * gets a property at the given json path of the calculation result for a quote
     */
    public getQuoteResultByPath(path: string): any {
        let calculationResult: QuoteResult = this.getQuoteResult();
        if (StringHelper.isNullOrEmpty(path)) {
            return calculationResult;
        }
        let pathNodeArrays: Array<string> = path.split('.');
        let currentReference: QuoteResult = calculationResult;
        try {
            for (let pathNodeArray of pathNodeArrays) {
                currentReference = currentReference[pathNodeArray];
            }
            return currentReference;
        } catch (err) {
            return '';
        }
    }

    public getFinancialTransactionType(): string {
        return this.getQuoteResultByPath('payment.amountPayable') == null
            ? 'none'
            : !this.isCurrency(this.getQuoteResultByPath('payment.amountPayable'))
                ? 'none'
                : (this.currencyAsNumber(this.getQuoteResultByPath('payment.amountPayable')) == 0)
                    ? 'none'
                    : (this.currencyAsNumber(this.getQuoteResultByPath('payment.amountPayable')) > 0)
                        ? 'payment'
                        : 'refund';
    }

    /**
     * gets a property at the given json path of the calculation result for a quote
     */
    public getClaimResultByPath(path: string): any {
        let pathNodeArrays: Array<string> = path.split('.');
        let calculationResult: ClaimResult = this.getClaimResult();
        let currentReference: ClaimResult = calculationResult;
        try {
            for (let pathNodeArray of pathNodeArrays) {
                currentReference = currentReference[pathNodeArray];
            }
            return currentReference;
        } catch (err) {
            return '';
        }
    }

    /**
     * @deprecated Please use getCalculationResultByPath(...) instead.
     */
    public calculationResult(path: string): any {
        this.showDeprecatedMessageIfNeverShown(
            'calculationResult',
            'Please use getCalculationResultByPath(...) instead.');
        return this.getCalculationResultByPath(path);
    }

    /** * File Related ***/

    public fileProperties(attachmentFieldValue: string): any {
        let result: AttachmentFileProperties = <AttachmentFileProperties>{};
        let fileRegExp: RegExp = /^([^\:]+)\:([^\:]+)\:([^\:]*)\:([^\:]*)\:([^\:]*)\:?([^\:]*)?$/;
        if (fileRegExp.test(attachmentFieldValue)) {
            let fieldValueParts: Array<string> = attachmentFieldValue.split(':');
            let attachmentId: string = fieldValueParts[2];
            let fileProperties: AttachmentFileProperties = this.attachmentService.getAttachment(attachmentId);
            if (fileProperties) {
                result = fileProperties;
                result['attachmentId'] = attachmentId;
            } else {
                result = AttachmentHelper.parseFileProperties(attachmentFieldValue);
            }
        }
        return result;
    }

    public imageWidth(attachmentFieldValue: string): number {
        return this.fileProperties(attachmentFieldValue).imageWidth;
    }

    public imageHeight(attachmentFieldValue: string): number {
        return this.fileProperties(attachmentFieldValue).imageHeight;
    }

    public fileSizeBytes(attachmentFieldValue: string): number {
        return this.fileProperties(attachmentFieldValue).fileSizeBytes;
    }

    public fileSizeString(attachmentFieldValue: string): string {
        let fileSizeInBytes: number = this.fileSizeBytes(attachmentFieldValue);
        if (fileSizeInBytes) {
            let i: number = -1;
            let byteUnits: Array<string> = ['kB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];
            do {
                fileSizeInBytes = fileSizeInBytes / 1024;
                i++;
            } while (fileSizeInBytes > 1024);
            return Math.max(fileSizeInBytes, 0.1).toFixed(1) + byteUnits[i];
        } else {
            return '';
        }
    }

    public fileMimeType(attachmentFieldValue: string): number {
        return this.fileProperties(attachmentFieldValue).mimeType;
    }

    public fileData(attachmentFieldValue: string): number {
        return this.fileProperties(attachmentFieldValue).fileData;
    }

    public fileName(attachmentFieldValue: string): number {
        return this.fileProperties(attachmentFieldValue).fileName;
    }

    public isImage(attachmentFieldValue: string): boolean {
        let fileProperties: any = this.fileProperties(attachmentFieldValue);
        if (fileProperties && fileProperties.mimeType) {
            return fileProperties.mimeType.indexOf('image/') != -1;
        }
        return false;
    }

    /** * Other functions ***/

    public calculationsAreActive(): boolean {
        return this.applicationService.calculationInProgress;
    }

    public attachmentUploadsAreActive(): boolean {
        return (this.attachmentOperation.operationInProgress);
    }

    public webhookRequestIsActive(webhookFieldPath: string): boolean {
        return this.webhookService.getWebhookFieldIsActive(webhookFieldPath);
    }

    public webhookRequestsAreActive(): boolean { // TODO: refactor this code into a new operation for webhooks
        return (this.webhookService.getActiveWebhookCount() > 0);
    }

    public calculationInProgress(): boolean {
        return this.applicationService.calculationInProgress;
    }

    public backgroundCalculationInProgress(): boolean {
        const hasBackgroundCalculationInProgress: boolean = this.applicationService.backgroundCalculationInProgress;
        return hasBackgroundCalculationInProgress;
    }

    public actionInProgress(actionName?: string): boolean {
        return this.workflowStatusService.isActionInProgress(actionName);
    }

    public navigationInProgress(actionName?: string): boolean {
        return this.workflowStatusService.isNavigationInProgress(actionName);
    }

    public operationInProgress(operationName?: string): boolean {
        return this.operationStatusService.isOperationInProgress(operationName);
    }

    public attachmentUploadsInProgress(): boolean {
        return (this.attachmentOperation.operationInProgress);
    }

    public webhookRequestsInProgress(): boolean { // TODO: refactor this code into a new operation for webhooks
        return (this.webhookService.getActiveWebhookCount() > 0);
    }

    public getDefaultLandingPageUrl(): string {
        let applicationUrl: string = this.getApplicationUrl();
        let organisation: string = this.applicationService.organisationAlias ?
            `&organisation=${this.applicationService.organisationAlias}` : '';
        let tenantAlias: string = this.applicationService.tenantAlias;
        let productAlias: string = this.applicationService.productAlias;
        return `${applicationUrl}/assets/landing-page.html?tenant=${tenantAlias}${organisation}`
            + `&productId=${productAlias}&environment=${this.applicationService.environment}`;
    }

    /** ********* URLS *********/

    /**
     * Alias: referrerUrl(): string
     */
    public getReferrerUrl(): string {
        return document.referrer;
    }

    /**
     * @deprecated please use getReferrerUrl() instead.
     */
    public referrerUrl(): string {
        this.showDeprecatedMessageIfNeverShown('referrerUrl', 'Please use getReferrerUrl() instead.');
        return this.getReferrerUrl();
    }

    /**
     * Alias: baseUrl(): string
     */
    public getBaseUrl(url: string): string {

        // We cannot use javascript URL() here yet, since it is not supported in IE11.

        const domain: string = url.replace(/^http:\/\//i, '').replace(/^https:\/\//i, '').split(/[/?#]/)[0];
        return /^http:\/\//i.test(url) ? 'http://' + domain : 'https://' + domain;
    }

    /**
     * @deprecated Please use getBaseUrl(...) instead.
     */
    public baseUrl(url: string): string {
        this.showDeprecatedMessageIfNeverShown('baseUrl', 'Please use getBaseUrl(...) instead.');
        return this.getBaseUrl(url);
    }

    public hostName(url: string): string {
        if (url.indexOf('/', 7) != -1) {
            return url.substring(url.indexOf('//', 0) + 2, url.indexOf('/', 8));
        } else {
            return url.substring(url.indexOf('//', 0) + 2, url.length);
        }
    }

    /**
     * Replaces each instance of certain characters by one, two, three, or four escape sequences representing the UTF-8
     * encoding of the character (will only be four escape sequences for characters composed of two "surrogate"
     * characters).
     * 
     * see https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/encodeURI
     * 
     */
    public encodeUrl(url: string): string {
        return encodeURI(url);
    }

    /**
     * Replaces each instance of certain characters by one, two, three, or four escape sequences representing the UTF-8
     * encoding of the character (will only be four escape sequences for characters composed of two "surrogate"
     * characters).
     * 
     * see https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/encodeURIComponent
     */
    public encodeUrlComponent(urlComponent: string): string {
        return encodeURIComponent(urlComponent);
    }

    /** ********* Postcode *********/

    public postcodeInState(postcode: string, state: string, includePOBoxes: boolean = true): boolean {
        let statePostcodes: any = {
            "NSW": [{ "start": 2000, "end": 2599 }, { "start": 2620, "end": 2899 }, { "start": 2921, "end": 2999 }],
            "ACT": [{ "start": 2600, "end": 2619 }, { "start": 2900, "end": 2920 }],
            "VIC": [{ "start": 3000, "end": 3999 }],
            "QLD": [{ "start": 4000, "end": 4999 }],
            "SA": [{ "start": 5000, "end": 5799 }],
            "WA": [{ "start": 6000, "end": 6797 }],
            "TAS": [{ "start": 7000, "end": 7799 }],
            "NT": [{ "start": 800, "end": 899 }],
        };
        let statePOBoxPostcodes: any = {
            "NSW": [{ "start": 1000, "end": 1999 }],
            "ACT": [{ "start": 200, "end": 299 }],
            "VIC": [{ "start": 8000, "end": 8999 }],
            "QLD": [{ "start": 9000, "end": 9999 }],
            "SA": [{ "start": 5800, "end": 5999 }],
            "WA": [{ "start": 6800, "end": 6999 }],
            "TAS": [{ "start": 7800, "end": 7999 }],
            "NT": [{ "start": 900, "end": 999 }],
        };

        /* eslint-disable @typescript-eslint/naming-convention */
        let exceptions: any = {
            "4825": ["NT"],
            "0872": ["SA", "WA"],
            "2406": ["QLD"],
            "2611": ["NSW"],
            "2620": ["ACT"],
            "3500": ["NSW"],
            "3585": ["NSW"],
            "3586": ["NSW"],
            "3644": ["NSW"],
            "3691": ["NSW"],
            "3707": ["NSW"],
            "4380": ["NSW"],
            "4377": ["NSW"],
        };

        if (exceptions['' + postcode] && exceptions['' + postcode].indexOf(state.toUpperCase()) != -1) {
            return true;
        }
        let states: Array<string> = Object.keys(statePostcodes);
        if (states.indexOf(state) != -1) {
            for (let range of statePostcodes[state]) {
                if (range.start <= postcode && range.end >= postcode) {
                    return true;
                }
            }
            if (includePOBoxes) {
                for (let range of statePOBoxPostcodes[state]) {
                    if (range.start <= postcode && range.end >= postcode) {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    // check if ccvNumber matches the length of its creditCardNumber
    public ccvMatchCCType(creditCardNumber: string, ccvNumber: string): boolean {

        // retrieve type
        let type: string = this.getCreditCardType(creditCardNumber);

        // get the ccv validation then test out ccvNumber on it.
        if (this.getCCVValidation(type).test(ccvNumber)) {
            return true;
        }

        return false;
    }

    // ccType: creditcard type
    // return regex
    public getCCVValidation(ccType: string): RegExp {
        try {
            return this.cardTypes[ccType].ccvRegex;
        } catch (e) {
            return /^(\d){3,4}$/;
        }
    }

    // will depricate in the future
    public creditCardType(creditCardNumber: string): string {
        return this.getCreditCardType(creditCardNumber);
    }

    // retieves the creditcard type of the card number
    public getCreditCardType(creditCardNumber: string): string {
        let cardType: string;

        for (let type in this.cardTypes) {
            if (this.cardTypes[type].cardRegex.test(creditCardNumber)) {
                cardType = type;
            }
        }
        return cardType;
    }

    public jsonStringify(object: any): string {
        return JSON.stringify(object);
    }

    /**
     * Made private
     * @deprecated
     */
    private model(): string {
        return this.formService.getValues();
    }

    /**
     * Currently this function will return true if the question set is valid or hidden,
     * but soon it will be changed to only return true if the question set is known to be valid.
     */
    public questionSetsAreValid(questionSetNames: Array<string>): boolean {
        return this.formService.questionSetsAreValid(questionSetNames);
    }

    public questionSetsAreValidOrHidden(questionSetNames: Array<string>): boolean {
        return this.formService.questionSetsAreValidOrHidden(questionSetNames);
    }

    public questionSetsAreInvalid(questionSetNames: Array<string>): boolean {
        return this.formService.questionSetsAreInvalid(questionSetNames);
    }

    public questionSetsAreInvalidOrHidden(questionSetNames: Array<string>): boolean {
        return this.formService.questionSetsAreInvalidOrHidden(questionSetNames);
    }

    public questionSetIsValid(questionSetName: string): boolean {
        return this.formService.questionSetIsValid(questionSetName);
    }

    public questionSetIsValidOrHidden(questionSetName: string): boolean {
        return this.formService.questionSetIsValidOrHidden(questionSetName);
    }

    public questionSetIsInvalid(questionSetName: string): boolean {
        return this.formService.questionSetIsInvalid(questionSetName);
    }

    public questionSetIsInvalidOrHidden(questionSetName: string): boolean {
        return this.formService.questionSetIsInvalidOrHidden(questionSetName);
    }

    /**
     * @deprecated use getUserType() instead.
     */
    public getUserRole(): string {
        return this.userService.userType;
    }

    /**
     * @deprecated use getEnvironment() instead
     */
    public getProductEnvironment(): string {
        this.showDeprecatedMessageIfNeverShown('getProductEnvironment', 'Please use getEnvironment() instead.');
        return this.getEnvironment();
    }

    public getEnvironment(): string {
        let result: string = this.applicationService.environment || '';
        return result.toLowerCase();
    }

    public getCurrentWorkflowStep(): string {
        return this.applicationService.currentWorkflowDestination.stepName;
    }

    public getPreviousWorkflowStep(index: number = 1): string {
        const historyOrderedByMostRecent: Array<string> =
            this.workflowStatusService.workflowStepHistory.map((s: string) => s).reverse();
        return historyOrderedByMostRecent[index] ? historyOrderedByMostRecent[index] :
            historyOrderedByMostRecent.length == 1 && index == 1 ? historyOrderedByMostRecent[0] : '';
    }

    public getQuoteState(): string {
        return this.applicationService.quoteState ?
            this.applicationService.quoteState.toLowerCase() : QuoteState.Nascent;
    }

    public getClaimState(): string {
        return this.applicationService.claimState ?
            this.applicationService.claimState.toLowerCase() : ClaimState.Nascent;
    }

    /**
     * @deprecated use either getClaimState() or getQuoteState().
     */
    public getApplicationState(): string {
        this.showDeprecatedMessageIfNeverShown(
            'getApplicationState',
            'Please use getQuoteState() or getClaimState() instead.');
        if (this.applicationService.claimId) {
            return this.getClaimState();
        } else if (this.applicationService.quoteId) {
            return this.getQuoteState();
        }
    }

    public getApplicationValues(property: string): string {
        return this.applicationService[property];
    }

    public getQuoteType(): string {
        return this.applicationService.quoteType;
    }

    /**
     * @deprecated please use getQuoteType() instead.
     */
    public quoteType(): any {
        this.showDeprecatedMessageIfNeverShown('quoteType', 'You can use getQuoteType() instead.');
        return this.getQuoteType();
    }

    public getPolicyId(): any {
        return this.applicationService.policyId;
    }

    public getQuoteId(): any {
        return this.applicationService.quoteId;
    }

    public getClaimId(): any {
        return this.applicationService.claimId;
    }

    public getTenantId(): any {
        return this.applicationService.tenantId;
    }

    public getTenantAlias(): any {
        return this.applicationService.tenantAlias;
    }

    public getPortalId(): string {
        return this.applicationService.portalId;
    }

    public getPortalAlias(): string {
        return this.applicationService.portalAlias;
    }

    public getOrganisationAlias(): string {
        return this.applicationService.organisationAlias;
    }

    public getOrganisationId(): string {
        return this.applicationService.organisationId;
    }

    public isDefaultOrganisation(): boolean {
        return this.applicationService.isDefaultOrganisation;
    }

    public getProductId(): any {
        return this.applicationService.productId;
    }

    public getProductAlias(): any {
        return this.applicationService.productAlias;
    }

    public userHasPermission(permission: string): boolean {
        if (!permission) {
            return false;
        }

        // check elevated privileges.
        let elevatedPermissions: ElevatedPermission = elevatedPermissionMapping[permission];
        if (elevatedPermissions) {
            return this.userService.permissions.indexOf(elevatedPermissions.tenantBoundPermission) > -1
                || this.userService.permissions.indexOf(elevatedPermissions.organisationBoundPermission) > -1
                || this.userService.permissions.indexOf(elevatedPermissions.ownershipBoundPermission) > -1;
        } else {
            return this.userService.permissions.indexOf(permission) > -1;
        }
    }

    public getUserType(): string {
        return this.userService.isCustomer ? 'customer' : 'client';
    }

    public getApplicationMode(): string {
        return this.applicationService.mode;
    }

    public quoteHasCustomer(): boolean {
        let result: boolean = !!this.applicationService.customerId;
        return result;
    }

    public getCustomerId(): string {
        return this.applicationService.customerId;
    }

    /**
     * So that we know whether to show or hide the customer contact fields,
     * if at the time the quote/claim was created there was no customer, we can
     * show them. This is so that even if the broker comes back to edit it
     * after the customer was created (from the quote) it will continue to
     * show the customer contact fields, because otherwise it would be unexpected
     * for them to disappear.
     */
    public hadCustomerOnCreation(): boolean {
        return this.applicationService.hadCustomerOnCreation;
    }

    public currencyAsNumber(currencyString: string): number {
        if (currencyString) {
            return CurrencyHelper.parse(currencyString);
        }
        return 0;
    }

    public getApplicationUrl(): string {
        return document.location.origin;
    }

    public getApiOrigin(): string {
        return this.applicationService.apiOrigin;
    }

    public isLoadedWithinPortal(): boolean {
        return this.applicationService.isLoadedWithinPortal;
    }

    public getAssetsFolderUrl(): string {
        return `${this.getApplicationUrl()}/api/v1`
            + `/tenant/${this.applicationService.tenantAlias}`
            + `/product/${this.applicationService.productAlias}`
            + `/environment/${this.applicationService.environment}`
            + `/form-type/${this.applicationService.formType}`
            + `/release/${this.applicationService.productReleaseId}`
            + `/asset`;
    }

    public getProductReleaseId(): string {
        return this.applicationService.productReleaseId;
    }

    public getProductReleaseNumber(): string {
        return this.applicationService.productReleaseNumber;
    }

    public isCurrency(currencyString: string): boolean {
        return CurrencyHelper.isCurrency(currencyString);
    }

    public userHasAccount(): boolean {
        return this.userService.isCustomerOrClientLoggedIn && this.workflowStatusService.isApplicationLoaded
            ? true : this.applicationService.userHasAccount;
    }

    public customerHasAccount(): boolean {
        return this.quoteHasCustomer() && this.userService.isLoadedCustomerHasUser
            ? true
            : this.applicationService.userHasAccount;
    }

    public isName(name: string): boolean {
        return !!name.match(/^[a-z ,.'-]+$/i);
    }

    public getQuoteReference(): string {
        return this.applicationService.quoteReference;
    }

    public getPreviousQuoteId(): string {
        return this.resumeApplicationService.loadExistingQuoteId();
    }

    public getPreviousQuoteState(): string {
        return this.applicationService.previousQuoteState;
    }

    public stringReplace(str: string, search: string | RegExp, replace: string): string {
        let regexPattern: RegExp = search instanceof RegExp ? search : new RegExp(search, 'g');

        return str.replace(regexPattern, replace);
    }

    public getPreviousClaimId(): string {
        return this.resumeApplicationService.loadExistingClaimId();
    }

    /**
     * @deprecated Please use isPaymentCardExpiryDate() instead.
     */
    public isExpiryDate(expiryDate: any): boolean {
        this.showDeprecatedMessageIfNeverShown('isExpiryDate', 'Please use isPaymentCardExpiryDate() instead.');
        return this.isPaymentCardExpiryDate(expiryDate);
    }

    public isPaymentCardExpiryDate(expiryDate: any): boolean {
        const regExp: RegExp = /^(0[1-9]|1[0-2])\/([0-9]{2})$/;

        if (!expiryDate || expiryDate == '') return false;

        if (regExp.test(expiryDate)) {
            const month: any = expiryDate.split('/')[0];
            let year: any = expiryDate.split('/')[1];

            if (year.length == 2) {
                year = '20' + year;
            }

            const firstDayOfMonth: Date = new Date(year, month - 1, 1);
            const lastDayOfMonth: Date
                = new Date(firstDayOfMonth.getFullYear(), firstDayOfMonth.getMonth() + 1, 0, 23, 59, 59);

            const dateString: string = lastDayOfMonth.toString();

            if (dateString != 'Invalid Date' && dateString != 'NaN' && lastDayOfMonth >= new Date()) {
                return true;
            }
        }

        return false;
    }

    public getCurrentUserPersonDetails(): PersonDetailModel {
        return this.applicationService.currentUser;
    }

    /** HTML table generation */

    /**
     * Generates a HTML table of the active triggers of the specified type.
     * @param triggerType the type of trigger, e.g. "review", "endorsement", "decline"
     * @param headers the column headers
     * @param cssClasses a string or an array of strings which are the names of the ccs classes to be added to the
     * table.
     * @param nameCase a string with text case e.g. "Sentence", "Title", "Case"
     */
    public generateSummaryTableOfActiveTriggersOfType(
        triggerType: string,
        headers: Array<string> = ['Trigger', 'Description'],
        cssClasses?: Array<string> | string,
        nameCase: TextCase = TextCase.Sentence,
    ): string {
        cssClasses = !cssClasses ? new Array<string>() : Array.isArray(cssClasses)
            ? cssClasses
            : cssClasses.split(' ');
        cssClasses.push(`${triggerType}-trigger`);
        let triggers: Array<TriggerDisplayConfig> = this.triggerService.getActiveTriggersByType(triggerType);
        if (triggers.length == 0) {
            return '';
        }
        let rows: Array<Array<string>> = new Array<Array<string>>();
        for (let trigger of triggers) {
            let row: Array<string> = new Array<string>();
            let name: string = nameCase ? StringHelper.toCase(trigger.name, nameCase) : trigger.name;
            row.push(name);
            row.push(trigger.reviewerExplanation);
            rows.push(row);
        }
        return this.generateSummaryTable(rows, true, headers, cssClasses);
    }

    /**
     * Generates a HTML table of the of the fields with a given tag
     * Ignores empty field values.     
     * @param tag the tag, which is a single word
     * @param headers the column headers (optional)
     * @param cssClasses a string or an array of strings which are the names of the ccs classes to be added to the
     * table.
     * @param skipRowWhenCellEmpty if true, skips rendering the entire row if a cell in that row is empty.
     * @param emptyCellContent if the cell is empty, render this content instead of just leaving it blank.
     * Defaults to "-".
     * @param nameCase a string with text case e.g. "Sentence", "Title", "Case" (Optional)
     */
    public generateSummaryTableOfFieldsWithTag(
        tag: string,
        headers?: Array<string>,
        cssClasses?: Array<string> | string,
        skipRowWhenCellEmpty: boolean = true,
        emptyCellContent: string = "-",
        nameCase?: TextCase,
    ): string {
        cssClasses = !cssClasses ? new Array<string>() : Array.isArray(cssClasses)
            ? cssClasses
            : cssClasses.split(' ');
        cssClasses.push(`${tag}-tag`);
        let fieldPaths: Array<string> = this.getFieldPathsWithTag(tag);
        return this.generateSummaryTableOfFields(
            fieldPaths,
            headers,
            cssClasses,
            skipRowWhenCellEmpty,
            emptyCellContent,
            nameCase);
    }

    /**
     * Generates a HTML table of the specified fields
     * @param fieldPaths the field paths array of strings.
     * @param headers the column headers (optional)
     * @param cssClasses a string or an array of strings which are the names of the ccs classes to be added to the
     * table.
     * @param skipRowWhenCellEmpty if true, skips rendering the entire row if a cell in that row is empty.
     * @param emptyCellContent if the cell is empty, render this content instead of just leaving it blank.
     * Defaults to "-".
     * @param nameCase a string with text case e.g. "Sentence", "Title", "Case" (Optional)
     */
    public generateSummaryTableOfFields(
        fieldPaths: Array<string>,
        headers?: Array<string>,
        cssClasses?: Array<string> | string,
        skipRowWhenCellEmpty: boolean = true,
        emptyCellContent: string = "-",
        nameCase?: TextCase,
    ): string {
        if (fieldPaths.length == 0) {
            return '';
        }
        cssClasses = !cssClasses ? new Array<string>() : Array.isArray(cssClasses)
            ? cssClasses
            : cssClasses.split(' ');
        let summaryTable: SummaryTable = new SummaryTable(null, new Set<string>(cssClasses));
        if (headers != null) {
            if (!Array.isArray(headers) || headers.length != 2) {
                throw Errors.Product.Configuration(
                    'The expression method generateSummaryTableOfFields takes an optional parameter "headers", '
                    + 'which is expected to be an array of 2 strings. What was passed in was not.');
            }
            summaryTable.entries.push(new SummaryTableHeaderRow(headers));
        }
        for (let fieldPath of fieldPaths) {
            let isRepeatingInstance: boolean = fieldPath.endsWith(']');
            let metadata: QuestionMetadata = null;
            if (!isRepeatingInstance) {
                metadata = this.fieldMetadataService.getMetadataForField(fieldPath);
            }
            if (isRepeatingInstance || (metadata && metadata.dataType == FieldDataType.Repeating)) {
                summaryTable.entries.push(
                    new SummaryTableRepeatingEntry(
                        fieldPath,
                        this.matchingFieldsSubjectService,
                        this.expressionInputSubjectService,
                        this.fieldMetadataService,
                        this.fieldFornatterResolver,
                        skipRowWhenCellEmpty,
                        emptyCellContent,
                        nameCase));
            } else {
                let value: any = this.expressionInputSubjectService.getLatestFieldValue(fieldPath);
                if (!skipRowWhenCellEmpty || !StringHelper.isNullOrWhitespace(value)) {
                    if (StringHelper.isNullOrWhitespace(value) && !StringHelper.isNullOrEmpty(emptyCellContent)) {
                        value = emptyCellContent;
                    } else if (metadata) {
                        value = this.fieldFornatterResolver.resolve(metadata.dataType).format(value, metadata);
                    }
                    let fieldKey: string = FieldPathHelper.getFieldKey(fieldPath);
                    let name: string = metadata
                        ? (metadata.name ? metadata.name : fieldKey)
                        : fieldKey;
                    name = nameCase ? StringHelper.toCase(name, nameCase) : name;
                    let nameCell: SummaryTableCell = new SummaryTableCell(name, new Set<string>(['summary-name']));
                    let valueCell: SummaryTableCell = new SummaryTableCell(value, new Set<string>(['summary-value']));
                    summaryTable.entries.push(new SummaryTableRow([nameCell, valueCell]));
                }
            }
        }
        return summaryTable.toHtml();
    }

    /**
     * Retrieve all the field paths
     * @param excludeNonDisplayableFields as the name suggests (optional).
     * @param excludePrivateFields as the name suggests (optional)
     */
    public getAllFieldPaths(
        excludeNonDisplayableFields: boolean = false,
        excludePrivateFields: boolean = false,
    ): Array<string> {
        let allFieldPaths: Array<string> = this.expressionInputSubjectService.getAllFieldPaths();
        return this.excludeFieldPath(allFieldPaths, excludeNonDisplayableFields, excludePrivateFields);
    }

    /**
     * Get the field value changes since last transaction of all the field paths
     * @param fieldPaths the field paths array of strings.
     * @param emptyCellContent if the cell is empty, render this content instead of just leaving it blank.
     * Defaults to "-".
     * @param nameCase a string with text case e.g. "Sentence", "Title", "Case" (Optional)
     * @param formatted a boolean value that suggests that the value is formatted (Optional)
     * @param excludeNonDisplayableFields as the name suggests (optional).
     * @param excludePrivateFields as the name suggests (optional).
     * @returns an array of tuple<string,string,string> 
     * where it is fieldPath, last transaction value and current value respectively.
     * e.g. [["Field name 1", "old value", "new value"],["Field name 2", "old value", "new value"]]
     */
    public getFieldValueChangesSinceLastTransaction(
        fieldPaths: Array<string>,
        emptyCellContent: string = "-",
        nameCase: TextCase = TextCase.Sentence,
        formatted: boolean = true,
        excludeNonDisplayableFields: boolean = false,
        excludePrivateFields: boolean = false,
    ): Array<[string, string, string]> {
        this.validateForQuote(this.getFieldValueChangesSinceLastTransaction.name);
        let result: Array<[string, string, string]> = Array<[string, string, string]>();
        fieldPaths = this.excludeFieldPath(fieldPaths, excludeNonDisplayableFields, excludePrivateFields);
        fieldPaths.forEach((fieldPath: string) => {
            let currentValue: any = this.getFieldValue(fieldPath, formatted) || "";
            let lastTransactionValue: any = this.getFieldValueFromLastTransaction(fieldPath, formatted) || "";
            lastTransactionValue = this.cleanFieldValueToString(lastTransactionValue, emptyCellContent);
            currentValue = this.cleanFieldValueToString(currentValue, emptyCellContent);
            if (currentValue != lastTransactionValue) {
                fieldPath = nameCase ? StringHelper.toCase(fieldPath, nameCase) : fieldPath;
                let values: [string, string, string] = [fieldPath, lastTransactionValue, currentValue];
                result.push(values);
            }
        });

        return result;
    }

    /**
     * generates the summary table of field values that did change since the last transaction
     * @param headers the 3 column headers (optional)
     * @param cssClasses a string or an array of strings which are the names of the ccs classes to be added to the
     * table.
     * @param emptyCellContent if the cell is empty, render this content instead of just leaving it blank.
     * Defaults to "-".
     * @param nameCase a string with text case e.g. "Sentence", "Title", "Case" (Optional)
     * @param excludeNonDisplayableFields as the name suggests (optional).
     * @param excludePrivateFields as the name suggests (optional).
     */
    public generateSummaryTableOfFieldValueChangesSinceLastTransaction(
        headers: [string, string, string] = ['Property', 'Old Value', 'New Value'],
        cssClasses?: Array<string> | string,
        emptyCellContent: string = "-",
        nameCase: TextCase = TextCase.Sentence,
        excludeNonDisplayableFields: boolean = true,
        excludePrivateFields: boolean = true,
    ): string {
        this.validateForQuote(this.generateSummaryTableOfFieldValueChangesSinceLastTransaction.name);
        let data: Array<[string, string, string]> =
            this.getComparisonOfFieldValueChangesSinceLastTransaction(
                excludeNonDisplayableFields,
                excludePrivateFields,
                true,
                nameCase);
        const headersArray: Array<string> = headers.reduce((acc: Array<any>, val: string) => acc.concat(val), []);
        return this.generateSummaryTable(data, undefined, headersArray, cssClasses, false, emptyCellContent);
    }

    /**
     * get the field value of a field path from the last transaction
     * @param fieldPath the field path
     * @param formatted formats the value coming from the metadata
     **/
    public getFieldValueFromLastTransaction(
        fieldPath: string,
        formatted: boolean = true,
    ): any {
        this.validateForQuote(this.getFieldValueFromLastTransaction.name);
        let fieldValue: any = this.getValueFromLastTransaction(
            fieldPath,
            this.getFieldValueFromLastTransaction.name);
        let fieldPathWithoutDelimiter: string = fieldPath.startsWith('/')
            ? fieldPath.slice(1)
            : fieldPath;
        if (formatted) {
            let metadata: QuestionMetadata =
                this.fieldMetadataService.getMetadataForField(fieldPathWithoutDelimiter);
            if (metadata) {
                let formattedValue: any =
                    this.fieldFornatterResolver.resolve(metadata.dataType).format(fieldValue, metadata);
                return formattedValue;
            } else {
                return fieldValue;
            }
        } else {
            return fieldValue;
        }
    }

    /**
     * check if field value of a field path from the last transaction changed
     * @param fieldPath the field path
     **/
    public hasFieldValueChangedSinceLastTransaction(
        fieldPath: string,
    ): boolean {
        this.validateForQuote(this.hasFieldValueChangedSinceLastTransaction.name);
        let previousFieldValue: any = this.getValueFromLastTransaction(
            fieldPath,
            this.hasFieldValueChangedSinceLastTransaction.name);
        let fieldPathWithoutDelimiter: string = fieldPath.startsWith('/') ? fieldPath.slice(1) : fieldPath;
        let currentFieldValue: any = this.getFieldValue(fieldPathWithoutDelimiter, false);
        return previousFieldValue != currentFieldValue;
    }

    private escapeRegExp(pattern: string): string {
        return pattern.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    }

    /**
     * validates the quote.
     **/
    private validateForQuote(functionName: string): void {
        if (this.applicationService.claimId) {
            throw new Error("The \"" + functionName + "()\" method failed to execute because the primary entity "
                + "operated on by this form is a \"claim\". "
                + "To resolve this issue, please ensure that this expression method is only used when "
                + "operating on a \"quote\" entity.");
        }

        if (this.applicationService.quoteId && this.getQuoteType().toLowerCase() == "newbusiness") {
            throw new Error("The \"" + functionName + "()\" method failed to execute because the primary "
                + "entity operated on by this form is a quote of type \"newBusiness\". "
                + "Because they do not relate to an existing policy, quotes of this type "
                + "cannot have a history of previous policy transactions. "
                + "To resolve this issue, please ensure that this expression method is only "
                + "used when operating on a quote that is of type \"adjustment\", \"renewal\" or \"cancellation\". ");
        }
    }

    /**
     * remove unecessary field values, and will use a default value for empty cell content.
     **/
    private cleanFieldValueToString(fieldValue: any, emptyCellContent: string = "-"): string {
        fieldValue =
            (fieldValue != null
                && (fieldValue.toString().indexOf("__zone_symbol__value") == 30
                    || fieldValue.toString().indexOf("[object Object]") > -1))
                ? ""
                : fieldValue;
        fieldValue =
            StringHelper.isNullOrEmpty(fieldValue)
                || StringHelper.isNullOrWhitespace(fieldValue)
                ? emptyCellContent
                : fieldValue;

        return fieldValue;
    }

    /**
     * gets the comparison of field values since last transaction.
     **/
    private getComparisonOfFieldValueChangesSinceLastTransaction(
        excludeNonDisplayableFields: boolean = true,
        excludePrivateFields: boolean = true,
        formatted: boolean = true,
        nameCase: TextCase,
    ): Array<[string, string, string]> {
        let result: Array<[string, string, string]> = Array<[string, string, string]>();
        let fieldPaths: Array<string> = this.getAllFieldPaths(
            excludeNonDisplayableFields,
            excludePrivateFields);
        fieldPaths.forEach((fieldPath: string) => {
            let lastTransactionValue: any = this.getFieldValueFromLastTransaction(fieldPath, formatted);
            let currentValue: any = this.getFieldValue(fieldPath, formatted);
            lastTransactionValue = this.cleanFieldValueToString(lastTransactionValue, "");
            currentValue = this.cleanFieldValueToString(currentValue, "");
            if (lastTransactionValue != currentValue
                &&
                (!StringHelper.isNullOrEmpty(lastTransactionValue)
                    || !StringHelper.isNullOrEmpty(currentValue))) {
                fieldPath = nameCase ? StringHelper.toCase(fieldPath, nameCase) : fieldPath;
                result.push([fieldPath, lastTransactionValue, currentValue]);
            }
        });

        return result;
    }

    /**
     * the form data from the last transaction, unformatted form data.
     **/
    private retrieveAndVerifyFormDataFromLastTransactions(functionName: string): any {
        let entities: any = this.getContextEntities();
        if (!(entities?.policy?.policyTransactions?.length ?? 0)
            && !(entities?.policy?.policyTransactions[0]?.formData)) {
            const message: string = (functionName ? "The \"" + functionName + "()\" method failed to execute "
                + "because the form context did not contain a history of previous policy transactions. " : "")
                + "To resolve this issue, please ensure that the path \"/policy/policyTransactions/formData\" "
                + "is included in the list of contextEntities included for quotes in product.json.";
            throw new Error(message);
        }

        return entities.policy.policyTransactions[0].formData;
    }

    /**
     *  retrieves the value from the form data.
     **/
    private getValueFromLastTransaction(
        fieldPath: string,
        callerFunctionName: string): any {
        let formData: any = this.retrieveAndVerifyFormDataFromLastTransactions(callerFunctionName);
        // Split the JSON path by slashes
        const pathParts: Array<string> = fieldPath.split('/');
        let value: any = formData;
        for (const part of pathParts) {
            if (part) { // Ignore empty parts, e.g., leading slash
                if (value && typeof value === 'object') {
                    if (/\[\d\]/.test(part)) {
                        value = this.getArrayValueByPath(formData, part);
                    } else {
                        value = value[part];
                    }
                } else {
                    return null;
                }
            }
        }
        return value;
    }

    private getArrayValueByPath(obj: any, path: string): any {
        const parts: Array<string> = path.split(/[\.\[\]]+/).filter(Boolean);
        let value: any = obj;

        for (const part of parts) {
            if (value && typeof value === 'object') {
                value = value[part];
            } else {
                return null;
            }
        }

        return value;
    }

    /**
     * Generates a set of HTML tables for a Repeating Question Set
     * @param repeatingFieldPath the path the the repeating field.
     * @param headingTagName the html tag name to use for headings. Defaults to h3.
     * @param headers the column headers (optional)
     * @param cssClasses a string or an array of strings which are the names of the ccs classes to be added to the
     * table.
     * @param skipRowWhenCellEmpty if true, skips rendering the entire row if a cell in that row is empty.
     * @param emptyCellContent if the cell is empty, render this content instead of just leaving it blank.
     * Defaults to "-".
     * @param nameCase a string with text case e.g. "Sentence", "Title", "Case" (Optional)
     * @param repeatingInstanceName the heading text to use for each repeating instance. It will have the instance
     * number automatically appended to it e.g. "Additional Driver 1". Defaults to the repeating field name/key.
     */
    public generateSummaryTablesForRepeatingField(
        repeatingFieldPath: string,
        headingTagName: string = 'h3',
        headers?: Array<string>,
        cssClasses?: Array<string> | string,
        skipRowWhenCellEmpty: boolean = true,
        emptyCellContent: string = "-",
        nameCase?: TextCase,
        repeatingInstanceName?: string,
    ): string {
        cssClasses = !cssClasses ? new Array<string>() : Array.isArray(cssClasses)
            ? cssClasses
            : cssClasses.split(' ');
        let headerRow: SummaryTableHeaderRow = null;
        if (headers != null) {
            if (!Array.isArray(headers) || headers.length != 2) {
                throw Errors.Product.Configuration(
                    'The expression method generateSummaryTablesForRepeatingQuestionSet takes an optional parameter '
                    + '"headers", which is expected to be an array of 2 strings. What was passed in was not.');
            }
            headerRow = new SummaryTableHeaderRow(headers);
        }
        let fieldPaths: Array<string>
            = this.matchingFieldsSubjectService.getFieldPathsMatchingPattern(repeatingFieldPath);
        fieldPaths = _.difference(fieldPaths, [repeatingFieldPath]);
        let html: string = '';
        let repeatingIndex: number = null;
        let currentSummaryTable: SummaryTable;
        let isANewRepeatingInstance: boolean = false;
        for (let fieldPath of fieldPaths) {
            if (!this.fieldMetadataService.isFieldDisplayable(fieldPath)) {
                continue;
            }
            let isRepeatingInstance: boolean = fieldPath.endsWith(']');
            let metadata: QuestionMetadata = null;
            if (!isRepeatingInstance) {
                metadata = this.fieldMetadataService.getMetadataForField(fieldPath);
            }

            let thisRepeatingIndex: number = FieldPathHelper.getRepeatingIndex(fieldPath);
            isANewRepeatingInstance = thisRepeatingIndex != repeatingIndex;
            if (isANewRepeatingInstance) {
                if (currentSummaryTable) {
                    html += currentSummaryTable.toHtml();
                }
                currentSummaryTable = new SummaryTable(null, new Set<string>(cssClasses));
                if (headerRow) {
                    currentSummaryTable.entries.push(headerRow);
                }
                repeatingIndex = thisRepeatingIndex;
                let repeatingFieldHeading: string = repeatingInstanceName;
                if (!repeatingInstanceName) {
                    let repeatingFieldPathBase: string = repeatingFieldPath.replace(/\[\d*\]/, '');
                    repeatingFieldHeading = ChangeCase.capitalCase(repeatingFieldPathBase);
                }
                repeatingFieldHeading += ` ${repeatingIndex + 1}`;
                html += `<${headingTagName}>${repeatingFieldHeading}</${headingTagName}>`;
            }
            let value: any = this.getFieldValue(fieldPath);
            if (!skipRowWhenCellEmpty || !StringHelper.isNullOrWhitespace(value)) {
                if (StringHelper.isNullOrWhitespace(value) && !StringHelper.isNullOrEmpty(emptyCellContent)) {
                    value = emptyCellContent;
                }
                let fieldKey: string = FieldPathHelper.getFieldKey(fieldPath);
                let name: string = metadata
                    ? (metadata.name ? metadata.name : fieldKey)
                    : fieldKey;
                name = nameCase ? StringHelper.toCase(name, nameCase) : name;
                let nameCell: SummaryTableCell = new SummaryTableCell(name, new Set<string>(['summary-name']));
                let valueCell: SummaryTableCell = new SummaryTableCell(value, new Set<string>(['summary-value']));
                let row: SummaryTableRow = new SummaryTableRow([nameCell, valueCell], new Set<string>(cssClasses));
                currentSummaryTable.entries.push(row);
            }
        }
        if (currentSummaryTable) {
            html += currentSummaryTable.toHtml();
        }
        return html;
    }

    /**
     * Generates a custom HTML table
     * @param data the table data, being an array of arrays of strings
     * @param isDataInRows if the data passed in is an array of rows, this should be set to true. 
     * If it's an array of columns, this should be set to false.
     * @param headers the column headers (optional)
     * @param cssClasses a string or an array of strings which are the names of the ccs classes to be added to the
     * table.
     * @param skipRowWhenCellEmpty if true, skips rendering the entire row if a cell in that row is empty.
     * @param emptyCellContent if the cell is empty, render this content instead of just leaving it blank.
     * Defaults to "-".
     */
    public generateSummaryTable(
        data: Array<Array<string>>,
        isRows: boolean = true,
        headers?: Array<string>,
        cssClasses?: Array<string> | string,
        skipRowWhenCellEmpty: boolean = true,
        emptyCellContent: string = "-",
    ): string {
        if (data.length == 0) {
            return '';
        }
        cssClasses = !cssClasses ? new Array<string>() : Array.isArray(cssClasses)
            ? cssClasses
            : cssClasses.split(' ');
        let summaryTable: SummaryTable = new SummaryTable(null, new Set<string>(cssClasses));
        if (headers) {
            if (!isRows && data.length !== headers.length) {
                throw Errors.Product.Configuration(
                    'The expression method generateSummaryTable takes an optional parameter "headers", which is '
                    + 'expected to be an array of strings, with the same number of items as the number of columns '
                    + `of data (${data.length}). However the headers array had ${headers.length} `
                    + 'items which does not match.');
            }

            if (isRows && Math.max(...data.map((d: Array<string>) => d.length)) !== headers.length) {
                throw Errors.Product.Configuration(
                    'The expression method generateSummaryTable takes an optional parameter "headers", which is '
                    + 'expected to be an array of strings, with the same number of items as the number of columns '
                    + `of data (${Math.max(...data.map((d: Array<string>) => d.length))}). However the headers `
                    + `array had ${headers.length} items which does not match.`);
            }

            summaryTable.entries.push(new SummaryTableHeaderRow(headers));
        }

        let rows: Array<Array<string>> = data;
        if (!isRows) {
            rows = DataHelper.convertColumnsToRows(data);
        }

        for (let row of rows) {
            let skipRow: boolean = false;
            if (skipRowWhenCellEmpty == true) {
                for (let cell of row) {
                    if (StringHelper.isNullOrWhitespace(cell)) {
                        skipRow = true;
                        break;
                    }
                }
            }
            if (!skipRow) {
                let cells: Array<SummaryTableCell> = new Array<SummaryTableCell>();
                for (let i: number = 0; i < row.length; i++) {
                    let cellContent: string = row[i];
                    if (StringHelper.isNullOrWhitespace(cellContent) && !StringHelper.isNullOrEmpty(emptyCellContent)) {
                        cellContent = emptyCellContent;
                    }
                    let cellClass: string = i == 0 ? "summary-name" : "summary-value";
                    let cell: SummaryTableCell = new SummaryTableCell(cellContent, new Set<string>([cellClass]));
                    cells.push(cell);
                }
                summaryTable.entries.push(new SummaryTableRow(cells));
            }
        }
        return summaryTable.toHtml();
    }

    /**
     * Returns an array of field paths for fields which have the given tag
     * @param tag the tags on the fields
     */
    public getFieldPathsWithTag(
        tag: string,
        excludeNonDisplayableFields: boolean = false,
        excludePrivateFields: boolean = false,
    ): Array<string> {
        let fields: Array<string> = Array.from(this.fieldMetadataService.getFieldPathsWithTag(tag));
        return this.excludeFieldPath(fields, excludeNonDisplayableFields, excludePrivateFields);
    }

    public getFieldPathsWithoutTag(
        tag: string,
        excludeNonDisplayableFields: boolean = false,
        excludePrivateFields: boolean = false,
    ): Array<string> {
        let fields: Array<string> = Array.from(this.fieldMetadataService.getFieldPathsWithoutTag(tag));
        return this.excludeFieldPath(fields, excludeNonDisplayableFields, excludePrivateFields);
    }

    private excludeFieldPath(
        fieldPaths: Array<string>,
        excludeNonDisplayableFields: boolean = false,
        excludePrivateFields: boolean = false,
    ): Array<string> {
        let result: Array<string> = Array<string>();
        fieldPaths.forEach((fieldPath: string) => {
            let metadata: QuestionMetadata = this.fieldMetadataService.getMetadataForField(fieldPath);
            const hasDataType: boolean = metadata && !StringHelper.isNullOrEmpty(metadata.dataType);
            const isNotDataTypeNone: boolean = hasDataType && metadata.dataType.toLowerCase() !== "none";
            const includededFields: boolean =
                metadata
                && (!((excludeNonDisplayableFields || excludePrivateFields)
                    && (!metadata.displayable || metadata.private)));
            if (hasDataType
                && isNotDataTypeNone
                && includededFields) {
                result.push(fieldPath);
            }
        });
        return result;
    }

    /**
     * @returns an array of field values for the given tag
     */
    public getFieldValuesWithTag(tag: string, formatted: boolean = true): Array<any> {
        let fieldPaths: Array<string> = this.getFieldPathsWithTag(tag);
        return this.getFieldValues(fieldPaths, formatted);
    }

    public getFieldValues(fieldPaths: Array<string>, formatted: boolean = true): Array<any> {
        let values: Array<any> = new Array<any>();
        for (let fieldPath of fieldPaths) {
            let fieldValue: any = this.getFieldValue(fieldPath, formatted);
            values.push(fieldValue);
        }
        return values;
    }

    /**
     * Gets the field value for a given field path, formatted by default
     * @param fieldPath 
     * @param formatted whether to format the field using the question metadata, e.g. currency, date, 
     */
    public getFieldValue(fieldPath: string, formatted: boolean = true): any {
        if (fieldPath.startsWith("/")) {
            fieldPath = fieldPath.slice(1);
        }
        let fieldValue: any = this.expressionInputSubjectService.getLatestFieldValue(fieldPath);
        if (formatted) {
            let metadata: QuestionMetadata = this.fieldMetadataService.getMetadataForField(fieldPath);
            if (metadata) {
                return this.fieldFornatterResolver.resolve(metadata.dataType).format(fieldValue, metadata);
            }
        }
        return fieldValue;
    }

    /**
     * Gets the search term for a given field that supports having a search term.
     * This is currently only supported by search select fields.
     * @returns the search term, or an empty string if not supported or the field is not found.
     */
    public getFieldSearchTerm(fieldPath: string): string {
        let fieldSearchTerm: string = this.expressionInputSubjectService.getLatestFieldSearchTerm(fieldPath);
        return fieldSearchTerm;
    }

    /**  DEBUG  */

    public isDebugEnabled(): boolean {
        return this.applicationService.debug;
    }

    public getDebugLevel(): number {
        return this.applicationService.debugLevel;
    }

    private showDeprecatedMessageIfNeverShown(key: string, message: string = null): void {
        if (!this.deprecatedMessageMap.get(key)) {
            message = `The expression method ${key}() has been deprecated. Please change your `
                + 'configuration to no longer use this method as it will be removed in a future version. ' + message;
            this.deprecatedMessageMap.set(key, message);
            console.warn(message);
        }
    }

    /** * Display mode related expressions for navigation within a workflow step ***/

    /**
     * Gets the display mode for the current workflow step, which is one of the following:
     * page - all articles, question sets and content from the step are displayed on the page at once
     * article - only one article is shown at a time.
     * articleElement - only one question set or content block is shown at a time.
     */
    public getCurrentWorkflowStepDisplayMode(): SectionDisplayMode {
        return this.workflowStatusService.currentSectionWidgetStatus.getCurrentWorkflowStepDisplayMode();
    }

    /**
     * @returns true if the current article is the first "renderable" article for the workflow step.
     */
    public isFirstArticle(): boolean {
        return this.workflowStatusService.currentSectionWidgetStatus.isFirstArticle();
    }

    /**
     * @returns true if the current article is the last "renderable" article for the workflow step.
     */
    public isLastArticle(): boolean {
        return this.workflowStatusService.currentSectionWidgetStatus.isLastArticle();
    }

    /**
     * @returns true if there is another "renderable" article to display after this one
     */
    public hasNextArticle(): boolean {
        return this.workflowStatusService.currentSectionWidgetStatus.hasNextArticle();
    }

    /**
     * @returns true if there is a prior "renderable" article to display
     */
    public hasPreviousArticle(): boolean {
        return this.workflowStatusService.currentSectionWidgetStatus.hasPreviousArticle();
    }

    /**
     * @returns a 0 based index of the current article being displayed. Returns -1 if there isn't one.
     */
    public getCurrentArticleIndex(): number {
        return this.workflowStatusService.currentSectionWidgetStatus.getCurrentArticleIndex();
    }

    /**
     * @returns the index of the first "renderable" article within this step.
     */
    public getFirstArticleIndex(): number {
        return this.workflowStatusService.currentSectionWidgetStatus.getFirstArticleIndex();
    }

    /**
     * @returns the index of the last "renderable" article within this step.
     */
    public getLastArticleIndex(): number {
        return this.workflowStatusService.currentSectionWidgetStatus.getLastArticleIndex();
    }

    /**
     * @returns the index of the next "renderable" article within this step.
     * Returns -1 if there is no next article.
     */
    public getNextArticleIndex(): number {
        return this.workflowStatusService.currentSectionWidgetStatus.getNextArticleIndex();
    }

    /**
     * @returns the index of the previous "renderable" article of the step.
     * Returns -1 if there is no previous article.
     */
    public getPreviousArticleIndex(): number {
        return this.workflowStatusService.currentSectionWidgetStatus.getPreviousArticleIndex();
    }

    /**
     * @returns true if the current question set or content block is the first "renderable"
     * article element for the workflow step
     */
    public isFirstArticleElement(): boolean {
        return this.workflowStatusService.currentSectionWidgetStatus.isFirstArticleElement();
    }

    /**
     * @returns true if the current question set or content block is the last "renderable"
     * article element for the workflow step
     */
    public isLastArticleElement(): boolean {
        return this.workflowStatusService.currentSectionWidgetStatus.isLastArticleElement();
    }

    /**
     * @returns true if there is another "renderable" question set or content block to display after this one.
     */
    public hasNextArticleElement(): boolean {
        return this.workflowStatusService.currentSectionWidgetStatus.hasNextArticleElement();
    }

    /**
     * @returns true if there is a prior "renderable" question set or content block to display.
     */
    public hasPreviousArticleElement(): boolean {
        return this.workflowStatusService.currentSectionWidgetStatus.hasPreviousArticleElement();
    }

    /**
     * @returns a 0 based index of the article element currently being displayed
     */
    public getCurrentArticleElementIndex(): number {
        return this.workflowStatusService.currentSectionWidgetStatus.getCurrentArticleElementIndex();
    }

    /**
     * @returns a 0 based index of the first "renderable" article element within the step.
     */
    public getFirstArticleElementIndex(): number {
        return this.workflowStatusService.currentSectionWidgetStatus.getFirstArticleElementIndex();
    }

    /**
     * @returns a 0 based index of the last "renderable" article element within the step.
     */
    public getLastArticleElementIndex(): number {
        return this.workflowStatusService.currentSectionWidgetStatus.getLastArticleElementIndex();
    }

    /**
     * @returns a 0 based index of the next "renderable" article element within this step. Returns -1 if there is none.
     */
    public getNextArticleElementIndex(): number {
        return this.workflowStatusService.currentSectionWidgetStatus.getNextArticleElementIndex();
    }

    /**
     * @returns a 0 based index of the previous "renderable" article element within this step.
     */
    public getPreviousArticleElementIndex(): number {
        return this.workflowStatusService.currentSectionWidgetStatus.getPreviousArticleElementIndex();
    }

    /**
     * @returns the article index for a given article element index.
     * 
     * E.g. if there are two articles with 3 article elements and we're displaying article element with index 4
     * (the 5th article element), then it will return 1 (representing the second article)
     */
    public getArticleIndexForArticleElementIndex(articleElementIndex: number): number {
        return this.workflowStatusService.currentSectionWidgetStatus
            .getArticleIndexForArticleElementIndex(articleElementIndex);
    }

    /** ** Repeating field navigation ****/

    /**
     * @returns the display mode for the repeating field, which is one of the following:
     * 
     * list - shows all repeating instances together on the same page
     * instance - shows only one repeating instance on the screen at a time
     * 
     * The fieldPath parameter is optional, and if left out, it will use the fieldPath of current repeating field.
     */
    public getRepeatingFieldDisplayMode(fieldPath?: string): RepeatingFieldDisplayMode {
        return this.repeatingQuestionSetTrackingService.getRepeatingFieldDisplayMode(fieldPath);
    }

    /**
     * @returns the fieldPath of the repeating field that is currently being navigated through on screen.
     */
    public getCurrentRepeatingFieldPath(): string {
        return this.repeatingQuestionSetTrackingService.getCurrentRepeatingFieldPath();
    }

    /**
     * @returns the number of repeating instances that have been created for this repeating field.
     * The fieldPath parameter is optional, and if left out, it will use the fieldPath of current repeating field.
     */
    public getRepeatingInstanceCount(fieldPath?: string): number {
        return this.repeatingQuestionSetTrackingService.getRepeatingInstanceCount(fieldPath);
    }

    /**
     * @returns a 0 based index representing the current repeating instance being displayed on screen.
     * The fieldPath parameter is optional, and if left out, it will use the fieldPath of current repeating field.
     */
    public getCurrentRepeatingInstanceIndex(fieldPath?: string): number {
        return this.repeatingQuestionSetTrackingService.getCurrentRepeatingInstanceIndex(fieldPath);
    }

    /**
     * @returns the maximum number of repeating instances that are allowed to be added.
     * The fieldPath parameter is optional, and if left out, it will use the fieldPath of current repeating field.
     */
    public getRepeatingFieldMaxQuantity(fieldPath?: string): number {
        return this.repeatingQuestionSetTrackingService.getRepeatingFieldMaxQuantity(fieldPath);
    }

    /**
     * @returns the minimum number of repeating instances to be rendered, and that the user would be required
     * to fill out.
     * 
     * The fieldPath parameter is optional, and if left out, it will use the fieldPath of current repeating field.
     */
    public getRepeatingFieldMinQuantity(fieldPath?: string): number {
        return this.repeatingQuestionSetTrackingService.getRepeatingFieldMinQuantity(fieldPath);
    }

    /**
     * @returns true if the current repeating instance being displayed is the first one.
     * 
     * The fieldPath parameter is optional, and if left out, it will use the fieldPath of current repeating field.
     */
    public isFirstRepeatingInstance(fieldPath?: string): boolean {
        return this.repeatingQuestionSetTrackingService.isFirstRepeatingInstance(fieldPath);
    }

    /**
     * @returns true if the current repeating instance being displayed is the last one.
     * 
     * The fieldPath parameter is optional, and if left out, it will use the fieldPath of current repeating field.
     */
    public isLastRepeatingInstance(fieldPath?: string): boolean {
        return this.repeatingQuestionSetTrackingService.isLastRepeatingInstance(fieldPath);
    }

    /**
     * @returns true if there is another repeating instance to display after this one.
     * 
     * The fieldPath parameter is optional, and if left out, it will use the fieldPath of current repeating field.
     */
    public hasNextRepeatingInstance(fieldPath?: string): boolean {
        return this.repeatingQuestionSetTrackingService.hasNextRepeatingInstance(fieldPath);
    }

    /**
     * @returns rue if there is a prior repeating instance before this one.
     * 
     * The fieldPath parameter is optional, and if left out, it will use the fieldPath of current repeating field.
     */
    public hasPreviousRepeatingInstance(fieldPath?: string): boolean {
        return this.repeatingQuestionSetTrackingService.hasPreviousRepeatingInstance(fieldPath);
    }

    /**
     * @returns the index of the repeating instance after this one.
     * 
     * The fieldPath parameter is optional, and if left out, it will use the fieldPath of current repeating field.
     */
    public getNextRepeatingInstanceIndex(fieldPath?: string): number {
        return this.repeatingQuestionSetTrackingService.getNextRepeatingInstanceIndex(fieldPath);
    }

    /**
     * @returns returns the index of the repeating instance before this one.
     * 
     * The fieldPath parameter is optional, and if left out, it will use the fieldPath of current repeating field.
     */
    public getPreviousRepeatingInstanceIndex(fieldPath?: string): number {
        return this.repeatingQuestionSetTrackingService.getPreviousRepeatingInstanceIndex(fieldPath);
    }

    /**
     * @returns the index of the last repeating instance.
     * 
     * The fieldPath parameter is optional, and if left out, it will use the fieldPath of current repeating field.
     */
    public getLastRepeatingInstanceIndex(fieldPath?: string): number {
        return this.repeatingQuestionSetTrackingService.getLastRepeatingInstanceIndex(fieldPath);
    }

    /**
     * @returns the index of the first repeating instance, e.g. 0, or -1 if not found.
     * 
     * The fieldPath parameter is optional, and if left out, it will use the fieldPath of current repeating field.
     */
    public getFirstRepeatingInstanceIndex(fieldPath?: string): number {
        return this.repeatingQuestionSetTrackingService.getFirstRepeatingInstanceIndex(fieldPath);
    }

    /** *** Navigation shortcuts */

    /**
     * @returns true if it makes sense to navigate to the next step.
     * If the step's displayMode is "page", this will always return true.
     * If the step's displayMode is "article", this will return true if hasNextArticle() returns false.
     * If the step's displayMode is "articleElement", this will return true if both
     * hasNextArticleElement() and hasNextRepeatingInstance() return false.
     */
    public canNavigateToNextStep(): boolean {
        if (this.getCurrentWorkflowStepDisplayMode() == SectionDisplayMode.Page) {
            return true;
        }

        if (this.getCurrentWorkflowStepDisplayMode() == SectionDisplayMode.Article
            && !this.hasNextArticle()
        ) {
            return true;
        }

        if (this.getCurrentWorkflowStepDisplayMode() == SectionDisplayMode.ArticleElement
            && !this.hasNextArticleElement()
            && !this.hasNextRepeatingInstance()
        ) {
            return true;
        }

        return false;
    }

    /**
     * @returns true if it makes sense to navigate to the previous step.
     * If the step's displayMode is "page", this will always return true.
     * If the step's displayMode is "article", this will return true if hasPreviousArticle() returns false.
     * If the step's displayMode is "articleElement", this will return true if both
     * hasPreviousArticleElement() and hasPreviousRepeatingInstance() return false.
     */
    public canNavigateToPreviousStep(): boolean {
        if (this.getCurrentWorkflowStepDisplayMode() == SectionDisplayMode.Page) {
            return true;
        }

        if (this.getCurrentWorkflowStepDisplayMode() == SectionDisplayMode.Article
            && !this.hasPreviousArticle()
        ) {
            return true;
        }

        if (this.getCurrentWorkflowStepDisplayMode() == SectionDisplayMode.ArticleElement
            && !this.hasPreviousArticleElement()
            && !this.hasPreviousRepeatingInstance()
        ) {
            return true;
        }

        return false;

    }

    /**
     * @returns true if it makes sense to navigate to the next article.
     * If the step's displayMode is "article", this will return true if hasNextArticle() returns true.
     */
    public canNavigateToNextArticle(): boolean {
        return this.getCurrentWorkflowStepDisplayMode() == SectionDisplayMode.Article
            && this.hasNextArticle();
    }

    /**
     * @returns true if it makes sense to navigate to the previous article.
     * If the step's displayMode is "article", this will return true if hasPreviousArticle() returns true.
     */
    public canNavigateToPreviousArticle(): boolean {
        return this.getCurrentWorkflowStepDisplayMode() == SectionDisplayMode.Article
            && this.hasPreviousArticle();
    }

    /**
     * @returns true if it makes sense to navigate to the next article element (e.g. Question Set or Content Block)
     * If the step's displayMode is "articleElement", this will return true if
     * hasNextArticleElement() is true and hasNextRepeatingInstance() is false.
     */
    public canNavigateToNextArticleElement(): boolean {
        return this.getCurrentWorkflowStepDisplayMode() == SectionDisplayMode.ArticleElement
            && !this.hasNextRepeatingInstance()
            && this.hasNextArticleElement();
    }

    /**
     * @returns true if it makes sense to navigate to the previous article element
     * (e.g. Question Set or Content Block)
     * 
     * If the step's displayMode is "articleElement", this will return true if
     * hasPreviousArticleElement() is true and hasPreviousRepeatingInstance() is false.
     */
    public canNavigateToPreviousArticleElement(): boolean {
        return this.getCurrentWorkflowStepDisplayMode() == SectionDisplayMode.ArticleElement
            && !this.hasPreviousRepeatingInstance()
            && this.hasPreviousArticleElement();
    }

    /**
     * @returns true if it makes sense to navigate to the next repeating instance.
     * If the step's displayMode is "articleElement", this will return true if hasNextRepeatingInstance() is true.
     */
    public canNavigateToNextRepeatingInstance(): boolean {
        return this.getCurrentWorkflowStepDisplayMode() == SectionDisplayMode.ArticleElement
            && this.hasNextRepeatingInstance();
    }

    /**
     * @returns True if it makes sense to navigate to the previous repeating instance.
     * If the step's displayMode is "articleElement", this will return true if hasPreviousRepeatingInstance() is true.
     */
    public canNavigateToPreviousRepeatingInstance(): boolean {
        return this.getCurrentWorkflowStepDisplayMode() == SectionDisplayMode.ArticleElement
            && this.hasPreviousRepeatingInstance();
    }

    /**
     * @returns a list of the question set paths for question sets that are visible on screen.
     * This can be used in conjunction with the action property "requiresValidQuestionSetsExpression" to require that
     * all visible question sets are valid before navigating to the next step.
     */
    public getVisibleQuestionSets(): Array<string> {
        return Array.from(this.formService.visibleQuestionSetPaths);
    }

    public getSidebarOffsetConfiguration(): string {
        return this.applicationService.sidebarOffsetConfiguration;
    }

    public getContextEntities(): any {
        return this.contextEntityService.getContextEntities();
    }

    public getEmbedOptions(): WebFormEmbedOptions {
        return this.applicationService.embedOptions;
    }

    public getSeedFormData(): any {
        return this.applicationService.embedOptions?.seedFormData;
    }

    public getOverwriteFormData(): any {
        return this.applicationService.embedOptions?.overwriteFormData;
    }

    public getEmbedFormData(): any {
        if (this.getSeedFormData() == null && this.getOverwriteFormData() == null) {
            return null;
        }
        return _.merge({}, this.getSeedFormData(), this.getOverwriteFormData());
    }
}
