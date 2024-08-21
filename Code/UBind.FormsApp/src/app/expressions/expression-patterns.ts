/* eslint-disable @typescript-eslint/naming-convention */

/**
 * Holds regular expression patterns used by the expression parser.
 */
export class ExpressionPatterns {
    public static readonly generateSummaryTableOfFields: RegExp
        = /(?:generateSummaryTableOfFields\( *\[ *)(('[a-zA-Z0-9_\[\]\.\]]+',? *)+)(?:] *)/gs;
    public static readonly getFieldValues: RegExp
        = /(?:getFieldValues\( *\[ *)(('[a-zA-Z0-9_\[\]\.\]]+',? *)+)(?:] *)/gs;
    public static readonly getFieldValue: RegExp
        = /(?:getFieldValue\( *)'([a-zA-Z0-9_\[\]\.\]]+)'(?: *,? *.* *\))??/gs;
    public static readonly getFieldSearchTerm: RegExp
        = /(?:getFieldSearchTerm\( *)'([a-zA-Z0-9_\[\]\.\]]+)'(?: *)\)?/gs;
    public static readonly generateSummaryTablesForRepeatingField: RegExp
        = /(?:generateSummaryTablesForRepeatingField\( *)'([a-zA-Z0-9_\[\]\.\]]+)'(?: *,? *.* *\))??/gs;
    public static readonly generateSummaryTableOfFieldsWithTag: RegExp
        = /(?:generateSummaryTableOfFieldsWithTag\( *)'([a-zA-Z0-9_\[\]\.\]]+)'/gs;
    public static readonly getFieldValuesWithTag: RegExp
        = /(?:getFieldValuesWithTag\( *)'([a-zA-Z0-9_\[\]\.\]]+)'(?: *\))/gs;
    public static readonly methodCallStart: RegExp
        = /[a-zA-Z0-9]+\(/gs;
    public static readonly fieldsAndArguments: RegExp
        = /[a-zA-Z0-9]+[a-zA-Z0-9]?(\[\d\])?(\.[a-zA-Z0-9]+[a-zA-Z0-9]?(\[\d\])?)*/gs;
    public static readonly getRelativeFieldValue: RegExp
        = /getRelativeFieldValue\( *(['a-zA-Z0-9_\/\[\]\.]*) *, *'([a-zA-Z0-9_\/\[\]]*)' *\)/gs;
    public static readonly getRelativeFieldPath: RegExp
        = /getRelativeFieldPath\( *(['a-zA-Z0-9_\/\[\]\.]*) *, *'([a-zA-Z0-9_\/\[\]]*)' *\)/gs;
    public static readonly questionSetOrSetsValidity: RegExp
        = /((?:questionSetIs|questionSetsAre)(?:Valid|Invalid|ValidOrHidden|InvalidOrHidden)\(\s*\[?.*?\]?\s*\))/gs;
    public static readonly questionSetsValidity: RegExp
        = /^(questionSetsAre(?:Valid|Invalid|ValidOrHidden|InvalidOrHidden)\()\s*(\[.*?\])\s*(\))/gs;
    public static readonly questionSetValidity: RegExp
        = /^questionSetIs(?:Valid|Invalid|ValidOrHidden|InvalidOrHidden)\(.*?\)/gs;
    public static readonly fieldIsValid: RegExp
        = /fieldIsValid\(\s*'[a-zA-Z0-9_\[\]\*\.]*'\s*\)/gs;
    public static readonly countRepeating: RegExp
        = /countRepeating\(\s*'[a-zA-Z0-9_]*'\s*\)/gs;
    public static readonly getRegexPattern: RegExp
        = /(stringContains|stringReplace)(.*?)(,|(, *))\/(.*?)\/([a-z]*?( *)[),])/gs;
}
