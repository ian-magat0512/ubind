import { ExpressionPatterns } from "./expression-patterns";
import matchAll from "string.prototype.matchall";

describe('ExpressionPatterns', (): void => {

    it('The regular expression pattern generateSummaryTableOfFields correctly matches', (): void => {
        let exp: RegExp = ExpressionPatterns.generateSummaryTableOfFields;
        let testString: string = "'aaa' + generateSummaryTableOfFields(['bbb', 'ccc', 'ddd']) + 'eee' + "
        + "generateSummaryTableOfFields(['fff', 'ggg', 'hhh']) + iii";
        const matchesIterator: IterableIterator<RegExpMatchArray> = matchAll(testString, exp);
        const matchesArray: Array<RegExpMatchArray> = Array.from(matchesIterator);
        expect(matchesArray.length).toBe(2);
        expect(matchesArray[0][1]).toBe("'bbb', 'ccc', 'ddd'");
        expect(matchesArray[1][1]).toBe("'fff', 'ggg', 'hhh'");
    });

    it('The regular expression pattern getFieldValues correctly matches', (): void => {
        let exp: RegExp = ExpressionPatterns.getFieldValues;
        let testString: string = "'aaa' + getFieldValues(['bbb', 'ccc', 'ddd']) + 'eee' + "
        + "getFieldValues(['fff', 'ggg', 'hhh'], false) + iii";
        const matchesIterator: IterableIterator<RegExpMatchArray> = matchAll(testString, exp);
        const matchesArray: Array<RegExpMatchArray> = Array.from(matchesIterator);
        expect(matchesArray.length).toBe(2);
        expect(matchesArray[0][1]).toBe("'bbb', 'ccc', 'ddd'");
        expect(matchesArray[1][1]).toBe("'fff', 'ggg', 'hhh'");
    });

    it('The regular expression pattern getFieldValue correctly matches', (): void => {
        const exp: RegExp = ExpressionPatterns.getFieldValue;
        let testString: string = "'aaa' + getFieldValue('bbb') + 'eee' + "
        + "getFieldValue( 'fff' , true ) + iii";
        const matchesIterator: IterableIterator<RegExpMatchArray> = matchAll(testString, exp);
        const matchesArray: Array<RegExpMatchArray> = Array.from(matchesIterator);
        expect(matchesArray.length).toBe(2);
        expect(matchesArray[0][1]).toBe("bbb");
        expect(matchesArray[1][1]).toBe("fff");
    });

    it('The regular expression pattern generateSummaryTablesForRepeatingField correctly matches', (): void => {
        let exp: RegExp = ExpressionPatterns.generateSummaryTablesForRepeatingField;
        let testString: string = "'aaa' + generateSummaryTablesForRepeatingField('testRepeating','People Covered', "
            + "['Item', 'Value'], null, false ) + 'eee' + getFiegenerateSummaryTablesForRepeatingField("
            + "'drivers','Drivers', ['Item', 'Value'], null, false ) + iii";
        const matchesIterator: IterableIterator<RegExpMatchArray> = matchAll(testString, exp);
        const matchesArray: Array<RegExpMatchArray> = Array.from(matchesIterator);
        expect(matchesArray.length).toBe(2);
        expect(matchesArray[0][1]).toBe("testRepeating");
        expect(matchesArray[1][1]).toBe("drivers");
    });

    it('The regular expression pattern generateSummaryTableOfFieldsWithTag matches a range of test cases', (): void => {
        const exp: RegExp = ExpressionPatterns.generateSummaryTableOfFieldsWithTag;
        let testString: string = "'aaaaa' + generateSummaryTableOfFieldsWithTag('bbb', ['Property', 'Value'], '',"
            + " true) + 'ccccc' + generateSummaryTableOfFieldsWithTag('ddd') + 'eeeee'";
        const matchesIterator: IterableIterator<RegExpMatchArray> = matchAll(testString, exp);
        const matchesArray: Array<RegExpMatchArray> = Array.from(matchesIterator);
        expect(matchesArray.length).toBe(2);
        expect(matchesArray[0][1]).toBe('bbb');
        expect(matchesArray[1][1]).toBe('ddd');
    });

    it('The regular expression pattern getFieldValuesWithTag matches a range of test cases', (): void => {
        let exp: RegExp = ExpressionPatterns.getFieldValuesWithTag;
        let testString: string =
            "'aaaaa' + getFieldValuesWithTag('bbb') + 'ccccc' + getFieldValuesWithTag('ddd') + 'eeeee'";
        const matchesIterator: IterableIterator<RegExpMatchArray> = matchAll(testString, exp);
        const matchesArray: Array<RegExpMatchArray> = Array.from(matchesIterator);
        expect(matchesArray.length).toBe(2);
        expect(matchesArray[0][1]).toBe('bbb');
        expect(matchesArray[1][1]).toBe('ddd');
    });

    it('The regular expression pattern methodCallStart matches a range of test cases', (): void => {
        let exp: RegExp = ExpressionPatterns.methodCallStart;
        let testString: string =
            "'aaaaa' + methodCallOne('bbb') + 'ccccc' + methodCallTWO('ddd') + 'eeeee'";
        let matchesArray: RegExpMatchArray = testString.match(exp);
        expect(matchesArray.length).toBe(2);
        expect(matchesArray[0]).toBe('methodCallOne(');
        expect(matchesArray[1]).toBe('methodCallTWO(');
    });

    it('The regular expression pattern fieldsAndArguments matches a range of test cases', (): void => {
        let exp: RegExp = ExpressionPatterns.fieldsAndArguments;
        let testString: string =
            "primaryDriverName + addtionalDrivers[0].name, something, [somethingElse], [[somethingOther]] + "
            + "asdf[0].qwer[1].overture, asdf.overture, one[1].two[2].three[3].four";
        let matchesArray: RegExpMatchArray = testString.match(exp);
        expect(matchesArray.length).toBe(8);
        expect(matchesArray[0]).toBe('primaryDriverName');
        expect(matchesArray[1]).toBe('addtionalDrivers[0].name');
        expect(matchesArray[2]).toBe('something');
        expect(matchesArray[3]).toBe('somethingElse');
        expect(matchesArray[4]).toBe('somethingOther');
        expect(matchesArray[5]).toBe('asdf[0].qwer[1].overture');
        expect(matchesArray[6]).toBe('asdf.overture');
        expect(matchesArray[7]).toBe('one[1].two[2].three[3].four');
    });

    it('The regular expression pattern questionSetOrSetsValidity matches a range of test cases', (): void => {
        let exp: RegExp = ExpressionPatterns.questionSetOrSetsValidity;
        let testString: string =
            "questionSetIsValid('one') || questionSetsAreValid(['two']) || questionSetIsInvalid('three') "
            + "questionSetsAreValidOrHidden(['four']) || questionSetIsInvalidOrHidden('five')";
        let matchesArray: RegExpMatchArray = testString.match(exp);
        expect(matchesArray.length).toBe(5);
        expect(matchesArray[0]).toBe("questionSetIsValid('one')");
        expect(matchesArray[1]).toBe("questionSetsAreValid(['two'])");
        expect(matchesArray[2]).toBe("questionSetIsInvalid('three')");
        expect(matchesArray[3]).toBe("questionSetsAreValidOrHidden(['four'])");
        expect(matchesArray[4]).toBe("questionSetIsInvalidOrHidden('five')");
    });

    it('The regular expression pattern questionSetsValidity matches a range of test cases', (): void => {
        let exp: RegExp = ExpressionPatterns.questionSetsValidity;
        expect("questionSetsAreValid(['two'])".match(exp)).not.toBeNull();
        expect("questionSetsAreInvalid(['three'])".match(exp)).not.toBeNull();
        expect("questionSetsAreValidOrHidden(['four'])".match(exp)).not.toBeNull();
        expect("questionSetsAreInvalidOrHidden(['five'])".match(exp)).not.toBeNull();
    });

    it('The regular expression pattern questionSetValidity matches a range of test cases', (): void => {
        let exp: RegExp = ExpressionPatterns.questionSetValidity;
        expect("questionSetIsValid('one')".match(exp)).not.toBeNull();
        expect("questionSetIsInvalid('three')".match(exp)).not.toBeNull();
        expect("questionSetIsValidOrHidden('four')").not.toBeNull();
        expect("questionSetIsInvalidOrHidden('five')".match(exp)).not.toBeNull();
    });

    it('The regular expression pattern fieldIsValid matches a range of test cases', (): void => {
        let exp: RegExp = ExpressionPatterns.fieldIsValid;
        let testString: string =
            "fieldIsValid('one') || fieldIsValid('two')";
        let matchesArray: RegExpMatchArray = testString.match(exp);
        expect(matchesArray.length).toBe(2);
        expect(matchesArray[0]).toBe("fieldIsValid('one')");
        expect(matchesArray[1]).toBe("fieldIsValid('two')");
    });

    it('The regular expression pattern countRepeating matches a range of test cases', (): void => {
        let exp: RegExp = ExpressionPatterns.countRepeating;
        let testString: string =
            "countRepeating('one') || countRepeating( 'two' )";
        let matchesArray: RegExpMatchArray = testString.match(exp);
        expect(matchesArray.length).toBe(2);
        expect(matchesArray[0]).toBe("countRepeating('one')");
        expect(matchesArray[1]).toBe("countRepeating( 'two' )");
    });

    it('The regular expression pattern getFieldSearchTerm correctly matches', (): void => {
        const exp: RegExp = ExpressionPatterns.getFieldSearchTerm;
        let testString: string = "'aaa' + getFieldSearchTerm('bbb') + 'eee' + "
        + "getFieldSearchTerm( 'fff' ) + iii";
        const matchesIterator: IterableIterator<RegExpMatchArray> = matchAll(testString, exp);
        const matchesArray: Array<RegExpMatchArray> = Array.from(matchesIterator);
        expect(matchesArray.length).toBe(2);
        expect(matchesArray[0][1]).toBe("bbb");
        expect(matchesArray[1][1]).toBe("fff");
    });

});

