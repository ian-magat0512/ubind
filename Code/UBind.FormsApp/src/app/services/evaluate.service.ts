
import { Injectable } from '@angular/core';

/**
 * Export evaluate service class.
 * TODO: Write a better class header: evaluate service functions.
 */
@Injectable()

export class EvaluateService {
    public evaluate(expression: string, args?: any): any {
        let argsString: string = '';
        let argsArray: Array<any> = [];
        for (let argName in args) {
            argsString += argsString == '' ? argName : ', ' + argName;
            argsArray.push(args[argName]);
        }
        try {
            // eslint-disable-next-line @typescript-eslint/ban-types
            let expressionFunction: Function = new Function(
                argsString,
                'return ' + expression + ';');
            let result: any = expressionFunction.apply(this, argsArray);
            return result;
        } catch (error) {
            throw new Error('Expression failed to evaluate:{' + expression + '}\n' +
                'These were the arguments:{' + argsString + '}\n' +
                'Error: "' + error.message + '"');
        }
    }

    public evaluateNew(
        expression: string,
        objectWithFunctions: object,
        objectVariableName: string,
        additionalArgments: object,
        additionalArgumentsVariableName: string): any {
        try {
            // eslint-disable-next-line @typescript-eslint/ban-types
            let expressionFunction: Function = new Function(
                // the function parameters:
                `${objectVariableName}, ${additionalArgumentsVariableName}`,
                // the function body:
                'return ' + expression + ';',
            );
            let result: any = expressionFunction.apply(this, [objectWithFunctions, additionalArgments]);
            return result;
        } catch (error) {
            throw new Error('Expression failed to evaluate: ' + expression + '\n' +
                'Error message: ' + error.message);
        }
    }

}
