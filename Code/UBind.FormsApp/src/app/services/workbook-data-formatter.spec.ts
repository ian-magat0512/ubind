import { ConfigService } from "./config.service";
import { EventService } from "./event.service";
import { WorkbookDataFormatter } from "./workbook-data-formatter";

describe('WorkbookDataFormatter', () => {
    let eventService: EventService;
    let configService: any;
    let workbookDataFormatter: WorkbookDataFormatter;

    beforeEach(() => {
        eventService = new EventService();
        configService = new ConfigService(eventService, null);
        workbookDataFormatter = new WorkbookDataFormatter(configService);
    });

    it('should format workbook values for the question sets table when configuration is V2 '
        + 'and a standard ubind workbook is being used', () => {
        // Arrange
        configService._configuration = {
            version: '2',
            calculatesUsingStandardWorkbook: true,
            fieldsOrderedByCalculationWorkbookRow: [
                {
                    key: 'field1',
                    calculationWorkbookCellLocation: {
                        sheetIndex: 1,
                        rowIndex: 5,
                        colIndex: 5,
                    },
                    displayable: true,
                    private: false,
                },
                {
                    key: 'field2',
                    calculationWorkbookCellLocation: {
                        sheetIndex: 1,
                        rowIndex: 6,
                        colIndex: 5,
                    },
                    displayable: true,
                    private: false,
                },
            ],
        };
        let formModel: object = {
            field1: 'val1',
            field2: 'val2',
        };

        // Act
        let result: Array<Array<string>> = workbookDataFormatter.generateQuestionAnswers(formModel);

        // Assert
        let expected: Array<Array<string>> = [
            ['Value'],
            ['val1'],
            ['val2'],
        ];
        expect(result).toEqual(expected);
    });

    it('should format workbook values for the question sets table when configuration is V2, '
        + 'a standard ubind workbook is being used and there are gaps in the locations', () => {
        // Arrange
        configService._configuration = {
            version: '2',
            calculatesUsingStandardWorkbook: true,
            fieldsOrderedByCalculationWorkbookRow: [
                {
                    key: 'field1',
                    calculationWorkbookCellLocation: {
                        sheetIndex: 1,
                        rowIndex: 6,
                        colIndex: 5,
                    },
                    displayable: true,
                    private: false,
                },
                {
                    key: 'field2',
                    calculationWorkbookCellLocation: {
                        sheetIndex: 1,
                        rowIndex: 8,
                        colIndex: 5,
                    },
                    displayable: true,
                    private: false,
                },
            ],
        };
        let formModel: object = {
            field1: "val1",
            field2: "val2",
        };

        // Act
        let result: Array<Array<string>> = workbookDataFormatter.generateQuestionAnswers(formModel);

        // Assert
        let expected: Array<Array<string>> = [
            ['Value'],
            [''],
            ['val1'],
            [''],
            ['val2'],
        ];
        expect(result).toEqual(expected);
    });

    it('should format workbook values for the repeating question sets table when configuration is V2', () => {
        // Arrange
        configService._configuration = {
            version: '2',
            calculatesUsingStandardWorkbook: true,
            repeatingInstanceMaxQuantity: 10,
            repeatingFieldsOrderedByCalculationWorkbookRow: [
                {
                    key: 'r1Field1',
                    questionSetKey: 'repeating1',
                    calculationWorkbookCellLocation: {
                        sheetIndex: 2,
                        rowIndex: 7,
                        colIndex: 5,
                    },
                    displayable: true,
                    private: false,
                },
                {
                    key: 'r1Field2',
                    questionSetKey: 'repeating1',
                    calculationWorkbookCellLocation: {
                        sheetIndex: 2,
                        rowIndex: 8,
                        colIndex: 5,
                    },
                    displayable: true,
                    private: false,
                },
                {
                    key: 'r2Field1',
                    questionSetKey: 'repeating2',
                    calculationWorkbookCellLocation: {
                        sheetIndex: 2,
                        rowIndex: 11,
                        colIndex: 5,
                    },
                    displayable: true,
                    private: false,
                },
                {
                    key: 'r2Field2',
                    questionSetKey: 'repeating2',
                    calculationWorkbookCellLocation: {
                        sheetIndex: 2,
                        rowIndex: 12,
                        colIndex: 5,
                    },
                    displayable: true,
                    private: false,
                },
            ],
        };
        let formModel: object = {
            repeating1: [
                {
                    r1Field1: 'r1Val1',
                    r1Field2: 'r1Val2',
                },
                {
                    r1Field1: 'r1Val3',
                    r1Field2: 'r1Val4',
                },
                {
                    r1Field1: 'r1Val5',
                    r1Field2: 'r1Val6',
                },
            ],
            repeating2: [
                {
                    r2Field1: 'r2Val1',
                    r2Field2: 'r2Val2',
                },
                {
                    r2Field1: 'r2Val3',
                    r2Field2: 'r2Val4',
                },
                {
                    r2Field1: 'r2Val5',
                    r2Field2: 'r2Val6',
                },
            ],
        };

        // Act
        let result: Array<Array<string>> = workbookDataFormatter.generateRepeatingQuestionAnswers(formModel);

        // Assert
        let expected: Array<Array<string>> = [
            ['Value 1', 'Value 2', 'Value 3', 'Value 4', 'Value 5', 'Value 6', 'Value 7', 'Value 8', 'Value 9',
                'Value 10'],
            ['', '', '', '', '', '', '', '', '', ''],
            ['', '', '', '', '', '', '', '', '', ''],
            ['r1Val1', 'r1Val3', 'r1Val5', '', '', '', '', '', '', ''],
            ['r1Val2', 'r1Val4', 'r1Val6', '', '', '', '', '', '', ''],
            ['', '', '', '', '', '', '', '', '', ''],
            ['', '', '', '', '', '', '', '', '', ''],
            ['r2Val1', 'r2Val3', 'r2Val5', '', '', '', '', '', '', ''],
            ['r2Val2', 'r2Val4', 'r2Val6', '', '', '', '', '', '', ''],
        ];
        expect(result).toEqual(expected);
    });

});
