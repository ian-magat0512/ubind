import { Injectable } from "@angular/core";
import { Operation } from "../operation";
import { FakeOperation } from "./fake.operation";

/**
 * Fake operation factory class
 */
@Injectable()
export class FakeOperationFactory {
    public formUpdateOperation: FakeOperation = new FakeOperation('formUpdate');
    public otherOperation: FakeOperation = new FakeOperation();

    public create(operationName: string): Operation {
        switch (operationName) {
            case 'formUpdate':
                return this.formUpdateOperation as Operation;
            default:
                return this.otherOperation as Operation;
        }
    }
}
