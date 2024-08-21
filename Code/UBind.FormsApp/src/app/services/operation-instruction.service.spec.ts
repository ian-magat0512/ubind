import { HttpResponse } from '@angular/common/http';
import { fakeAsync, tick } from '@angular/core/testing';
import { Operation } from '@app/operations/operation';
import { OperationFactory } from '@app/operations/operation.factory';
import { Observable, Subject } from 'rxjs';
import { OperationConfiguration } from '../models/configuration/operation-configuration';
import { OperationInstruction } from '../models/operation-instruction';
import { OperationInstructionStatus } from '../models/operation-instruction-status.enum';
import { ApplicationService } from './application.service';
import { ContextEntityService } from './context-entity.service';
import { OperationInstructionService } from './operation-instruction.service';
import { OperationStatusService } from './operation-status.service';
import { LoggerService } from './logger.service';

/* global spyOn */

/**
 * An operation that does nothing.
 */
class FakeOperation {
    public resultSubject: Subject<any> = new Subject<any>();
    public abortSubject: Subject<void> = new Subject<void>();
    public operationName: string;

    /**
     *
     */
    public constructor(name: string, resultSubject: Subject<any>) {
        this.resultSubject = resultSubject;
        this.operationName = name;
    }

    public execute(args: any = {}, operationId: number = Date.now()): Observable<any> {
        return this.resultSubject.asObservable();
    }
}

describe('OperationInstructionService', () => {
    let applicationService: ApplicationService = new ApplicationService();
    let operationStatusService: OperationStatusService = new OperationStatusService();

    it('critical operations should wait for blocking operations',
        fakeAsync(() => {
            // Arrange
            let operationConfigs: Array<string | OperationConfiguration> = new Array<string | OperationConfiguration>();
            operationConfigs.push({
                name: "formUpdate",
                backgroundExecution: true,
            });
            operationConfigs.push("bind");

            const formUpdateSubject: Subject<any> = new Subject<any>();
            const bindSubject: Subject<any> = new Subject<any>();
            let operationFactoryStub: any = {
                create: (operationName: string): Operation => {
                    switch (operationName) {
                        case "formUpdate":
                            return new FakeOperation(operationName, formUpdateSubject) as any as Operation;
                        case "bind":
                            return new FakeOperation(operationName, bindSubject) as any as Operation;
                    }
                },
            };
            let loggerService: LoggerService = new LoggerService(null, applicationService);
            let operationInstructionService: OperationInstructionService = new OperationInstructionService(
                operationFactoryStub as OperationFactory,
                null,
                applicationService ,
                new ContextEntityService(null, null, applicationService),
                null,
                null,
                null,
                operationStatusService,
                loggerService,
                null);
            spyOn<any>(operationInstructionService, 'reloadContextEntitiesIfOperationIsConfiguredToDoSo');

            let formUpdateActionOperation: OperationInstruction = new OperationInstruction(
                operationConfigs[0]);

            let bindActionOperation: OperationInstruction = new OperationInstruction(
                operationConfigs[1]);

            // Act + Assert
            expect(formUpdateActionOperation.status).toBe(OperationInstructionStatus.NotStarted);
            operationInstructionService.execute(formUpdateActionOperation);
            tick();
            expect(formUpdateActionOperation.status).toBe(OperationInstructionStatus.Started);
            expect(bindActionOperation.status).toBe(OperationInstructionStatus.NotStarted);
            operationInstructionService.execute(bindActionOperation);
            tick();
            expect(bindActionOperation.status).toBe(OperationInstructionStatus.NotStarted);
            let httpResponse: HttpResponse<any> = new HttpResponse<any>();
            (<any>httpResponse).status = 'success';
            formUpdateSubject.next(httpResponse);
            tick();
            expect(formUpdateActionOperation.status).toBe(OperationInstructionStatus.Completed);
            expect(bindActionOperation.status).toBe(OperationInstructionStatus.Started);
        }));
});
