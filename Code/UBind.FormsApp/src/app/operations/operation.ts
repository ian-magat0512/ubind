import { Injectable, EventEmitter, Output } from '@angular/core';
import { BehaviorSubject, Observable, Subject, Subscriber, Subscription, SubscriptionLike } from 'rxjs';
import { HttpResponse, HttpEventType, HttpErrorResponse, HttpEvent } from '@angular/common/http';
import { ApiService } from '../services/api.service';
import { ApplicationService } from '../services/application.service';
import { MessageService } from '../services/message.service';
import { ErrorHandlerService } from '@app/services/error-handler.service';
import { EventService } from '@app/services/event.service';
import { QuoteState } from '@app/models/quote-state.enum';
import { ClaimState } from '@app/models/claim-state.enum';
import { OperationRequestSettings } from './operation-request-settings';

/**
 * Represents the currently executing request for an operation.
 */
interface RequestOperation {
    request: Observable<HttpEvent<any>>;
    id: number;
}

/**
 * Subclasses can extend this interface to provide strongly type operation specific arguments.
 */
export interface OperationArguments {
    cancelEmitter?: EventEmitter<any>;
    reportProgress?: boolean;
}

/**
 * An operation sends a call to the back end to write or read data, or change state.
 */
@Injectable()
export abstract class Operation {

    public inProgressSubject: Subject<boolean> = new BehaviorSubject<boolean>(false);
    @Output() public nextResult: EventEmitter<any> = new EventEmitter<any>();

    public operationName: string;
    protected applicableFormType: string;

    protected _status: any;
    protected latestOperationId: any;
    protected _operationInProgress: boolean = false;
    protected _hasError: boolean = false;
    protected enableQueuedExecution: boolean = false;
    protected executionQueueLength: number = 0;

    protected defaultRequestSettings: OperationRequestSettings = {
        retryAttempts: 0,
        retryIntervalMillis: 3000,
        retryIntervalMultiplier: 1,
        timeoutRequests: false,
        defaultTimeoutMillis: 30000,
    };

    /**
     * If true, an app event will be published with the details of the response,
     * that that uBind injector gets a copy. It can then do things like push the
     * data to analytics.
     */
    protected publishAppEvents: boolean = true;

    public constructor(
        protected applicationService: ApplicationService,
        protected apiService: ApiService,
        protected messageService: MessageService,
        protected errorHandlerService: ErrorHandlerService,
        protected eventService: EventService) {
    }

    public getExecutionQueueLength(): number {
        return this.executionQueueLength;
    }

    /**
     * @param abortSubject a subject to notify when this execution of the operation should
     * be aborted or cancelled.
     */
    public execute(
        requestParams: any = {},
        args: OperationArguments = null,
        operationId: any = Date.now(),
        abortSubject: Subject<any> = null,
    ): Observable<any> {
        if (this.applicableFormType && this.applicableFormType != this.applicationService.formType) {
            throw new Error('Operation not allowed for this form type ' + this.applicationService.formType);
        }
        this.latestOperationId = operationId;
        if (this.enableQueuedExecution && this.executionQueueLength > 1) {
            let requestObservable: Observable<any>;
            if (this.executionQueueLength > 0) {
                this._operationInProgress = true;
                this.inProgressSubject.next(true);
                requestObservable = new Observable((observer: any) => {
                    let cancelled: boolean = false;
                    let cancelSubscription: SubscriptionLike;
                    let waitCount: number = this.executionQueueLength - 1;

                    const resultSubscription: Subscription = this.nextResult.subscribe((data: any) => {
                        waitCount--;
                        if (waitCount == 0) {
                            resultSubscription.unsubscribe();
                            if (!cancelled) {
                                this._operationInProgress = true;
                                this.inProgressSubject.next(true);
                                observer.next({ 'status': 'executing' });
                                this.generateRequest(data, args, operationId, abortSubject).subscribe(
                                    (data: any) => {
                                        observer.next(data);
                                        if (data.status != 'uploading') {
                                            observer.complete();
                                        }
                                    },
                                );
                            } else {
                                this.executionQueueLength--;
                                this.nextResult.emit({ 'status': 'cancelled' });
                                observer.complete();
                            }
                        }
                    });

                    cancelSubscription = args.cancelEmitter?.subscribe(
                        (data: any) => {
                            cancelled = true;
                            cancelSubscription.unsubscribe();
                        },
                    );
                });
                return requestObservable;
            } else {
                this._operationInProgress = true;
                this.inProgressSubject.next(true);
                requestObservable = new Observable((subscriber: Subscriber<any>) => {
                    subscriber.next({ 'status': 'executing' });
                    this.generateRequest(requestParams, args, operationId, abortSubject).subscribe(
                        (data: any) => {
                            subscriber.next(data);
                            if (data.status != 'uploading') {
                                subscriber.complete();
                            }
                        },
                    );
                });
                return requestObservable;
            }
        } else {
            this._operationInProgress = true;
            this.inProgressSubject.next(true);
            return this.generateRequest(requestParams, args, operationId, abortSubject);
        }
    }

    public generateRequest(
        data: any,
        args: OperationArguments,
        operationId: number,
        abortSubject: Subject<any>,
        requestSettings: OperationRequestSettings = null,
    ): Observable<any> {
        return this.get(this.operationName, data, args, operationId, requestSettings, abortSubject);
    }

    public get status(): any {
        return this._status;
    }

    public get operationInProgress(): any {
        return this._operationInProgress;
    }

    // creates a get request
    protected get(
        path: string,
        data: any,
        args: OperationArguments,
        operationId: any,
        requestSettings: OperationRequestSettings = null,
        abortSubject: Subject<any> = null,
    ): Observable<any> {
        return this.request('get', path, data, args, operationId, requestSettings, abortSubject);
    }

    // creates a post request
    protected post(
        path: string,
        data: any,
        args: OperationArguments,
        operationId: any,
        requestSettings: OperationRequestSettings = null,
        abortSubject: Subject<any> = null,
        reportProgress: boolean = false,
    ): Observable<any> {
        return this.request('post', path, data, args, operationId, requestSettings, abortSubject, reportProgress);
    }

    // creates a patch request
    protected patch(
        path: string,
        data: any,
        args: OperationArguments,
        operationId: any,
        requestSettings: OperationRequestSettings = null,
        abortSubject: Subject<any> = null,
    ): Observable<any> {
        return this.request('patch', path, data, args, operationId, requestSettings, abortSubject);
    }

    // creates a put request
    protected put(
        path: string,
        data: any = null,
        args: OperationArguments,
        operationId: any,
        requestSettings: any = null,
        abortSubject: Subject<any> = null,
    ): Observable<any> {
        return this.request('put', path, data, args, operationId, requestSettings, abortSubject);
    }

    /**
     * Creates an observable for request/s made by the operation, to the server-side.
     * @param requestType The type of request to be fired.
     * @param path The path to be used to send off the request to.
     * @param requestPayload The payload or request parameters to be used as part of the request.
     * @param operationId The ID of the operation to be fired, which is a timestamp of the 
     * exact time the request is executed.
     * @param customRequestSettings An object containing custom requests settings, if any.
     * @param abortSubject The cancellation emitter to be used, if any.
     */
    protected request(
        requestType: string,
        path: string,
        requestPayload: any,
        args: OperationArguments,
        operationId: any,
        customRequestSettings: any = null,
        abortSubject: Subject<any> = null,
        reportProgress: boolean = false,
    ): Observable<any> {
        const requestsArray: Array<RequestOperation> = [];
        let hasError: boolean = false;
        let response: any;
        const requestSettings: OperationRequestSettings = {};
        Object.assign(requestSettings, this.defaultRequestSettings, customRequestSettings);
        const timeout: any = requestSettings.timeoutRequests ?
            requestSettings.retryIntervalMillis : requestSettings.defaultTimeoutMillis;
        this._operationInProgress = true;
        this.inProgressSubject.next(true);
        return new Observable((subscriber: Subscriber<any>) => {
            let cancelSubscription: SubscriptionLike;
            let request: RequestOperation;
            let retryCount: number = 0;
            let retryIntervalMillis: number = requestSettings.retryIntervalMillis;
            const retryOperation: () => void = (): void => {
                this._operationInProgress = true;
                this.inProgressSubject.next(true);
                if (!response &&
                    this._operationInProgress && this.latestOperationId == operationId
                ) {
                    request = {
                        request: this.apiService[requestType](path, requestPayload, timeout, args?.reportProgress)
                            .subscribe(
                                (data: any) => {
                                    if (this.operationName == 'attachment'
                                        && data && data.type == HttpEventType.UploadProgress
                                    ) {
                                        subscriber.next({
                                            'status': 'uploading',
                                            'loaded': data.loaded,
                                            'total': data.total,
                                        });
                                    } else if (data instanceof HttpResponse) {
                                        if (cancelSubscription) {
                                            cancelSubscription.unsubscribe();
                                        }

                                        this._operationInProgress = false;
                                        this.inProgressSubject.next(false);
                                        response = this.processResponse(data, operationId, requestPayload, args);
                                        this.nextResult.emit(response);
                                        subscriber.next(response);
                                        subscriber.complete();
                                    }
                                },
                                (err: HttpErrorResponse) => {
                                    hasError = !(err.status == 401);
                                    if (requestSettings.retryAttempts > 0 && this._operationInProgress) {
                                        if (retryCount < requestSettings.retryAttempts) {
                                            retryCount++;
                                            retryIntervalMillis *= requestSettings.retryIntervalMultiplier;
                                            setTimeout(() => retryOperation(), retryIntervalMillis);
                                        }
                                    } else {
                                        if (cancelSubscription) {
                                            cancelSubscription.unsubscribe();
                                        }
                                        this.registerError(err, operationId, requestType);
                                        this._operationInProgress = false;
                                        this.inProgressSubject.next(false);
                                        this.nextResult.emit(err);
                                        if (!this.processError(err, operationId, requestType)) {
                                            subscriber.error(err);
                                        }
                                        subscriber.complete();
                                    }
                                },
                                () => {
                                    this.decrementExecutionQueueLength();
                                },
                            ),
                        'id': operationId,
                    };
                    requestsArray.push(request);
                } else {
                    this._operationInProgress = false;
                    this.inProgressSubject.next(false);
                    this.decrementExecutionQueueLength();
                }
            };

            this.handleAbort(abortSubject, requestsArray, subscriber);
            retryOperation(); // if this is the first time, it's not really retrying, just executing.

            if (requestSettings.timeoutRequests && requestSettings.retryAttempts > 0
                && this._operationInProgress && this.latestOperationId == operationId && !hasError) {
                setTimeout(
                    () => {
                        if (this._operationInProgress && this.latestOperationId == operationId && !hasError) {
                            console.log('The operation ' + this.operationName
                                + ` has timed out after ${requestSettings.defaultTimeoutMillis}. Retrying.`);
                            retryOperation();
                        }
                    }, requestSettings.defaultTimeoutMillis);
            }
        });
    }

    protected processResponse(data: any, operationId: any, requestPayload: any, args: OperationArguments): any {
        if (data instanceof HttpResponse && this.publishAppEvents) {
            this.eventService.appEventSubject.next(this.operationName + 'Operation');
        }
        let payload: any = {};
        if (data.body != null) {
            try {
                payload = data.body;
                if (payload['policyId'] != null) {
                    this.applicationService.policyId = payload['policyId'];
                }
                if (payload['quoteId'] != null) {
                    this.applicationService.quoteId = payload['quoteId'];
                }
                if (payload['claimId'] != null) {
                    this.applicationService.claimId = payload['claimId'];
                }
                if (payload['quoteState']) {
                    this.applicationService.applicationState = payload['quoteState'];
                    this.processQuoteStateChange(payload['quoteState']);
                }
                if (payload['claimState']) {
                    this.applicationService.applicationState = payload['claimState'];
                    this.processClaimStateChange(payload['claimState']);
                }
                if (payload['calculationResultId'] != null) {
                    this.applicationService.calculationResultId = payload['calculationResultId'];
                }
                if (payload['premiumFundingProposal'] != null) {
                    this.applicationService.premiumFundingProposalId = payload['premiumFundingProposal'].proposalId;
                    this.applicationService.principalFinanceAcceptanceUrl =
                        payload['premiumFundingProposal'].acceptanceUrl;
                }
                if (payload['policyNumber'] != null) {
                    this.applicationService.policyNumber = payload['policyNumber'];
                }

                if (payload['quoteReference'] != null && !this.applicationService.quoteReference) {
                    this.applicationService.quoteReference = payload['quoteReference'];
                }

                if (data['customerId']) {
                    this.applicationService.customerId = data['customerId'];
                }

            } catch (e) {
                // caters for blank responses
                console.error('Error processing operation response:');
                console.error(data);
            }
        }
        this._status = data;
        Object.assign(this._status, payload);
        this._status['operationId'] = operationId;
        if (payload['succeeded'] == null || payload['succeeded'] == true) {
            this._status['status'] = 'success';
        }
        this.applicationService.operationStatuses.set(this.operationName, this._status);
        return this._status;
    }

    private registerError(err: HttpErrorResponse, operationId: string, requestType?: string): void {
        let payload: any = {};
        try {
            payload = err.error.json();
        } catch (e) {
            // NOP;
        }
        this._status = err;
        Object.assign(this._status, payload);
        this.applicationService.operationStatuses.set(this.operationName, this._status);
    }

    /**
     * 
     * @returns true if the error has been fully handled, or false if it should be propagated for further handling
     */
    protected processError(err: HttpErrorResponse, operationId: string, requestType?: string): boolean {
        if (err.status === 401 && this.applicationService.isLoadedWithinPortal) {
            this.messageService.sendMessage('authenticationError', JSON.stringify(err.error || ''));
        }

        /*
        else {
            if (ProblemDetails.isProblemDetailsResponse(err) || err instanceof ProblemDetails) {
                const problemDetails: ProblemDetails =
                    err instanceof ProblemDetails ? err : ProblemDetails.fromJson(err.error);
                if (problemDetails) {
                    this.errorHandlerService.handleError(problemDetails);
                } else {
                    this.apiService.forceShowErrorModal(ErrorMessages.ServerInternalError);
                }
            }
        }
        */
        return false;
    }

    protected decrementExecutionQueueLength(): void {
        if (this.executionQueueLength) {
            if (this.executionQueueLength > 0) {
                this.executionQueueLength--;
            }
        }
    }

    /**
     * detects whether the quote state has changed and sends a message/event to the parent frame (ie portal) 
     * so that it can respond, e.g by refreshing the item in the list.
     * @param newQuoteState
     */
    protected processQuoteStateChange(newQuoteState: QuoteState): void {
        if (!newQuoteState || this.applicationService.quoteState?.toLowerCase() === newQuoteState.toLowerCase()) {
            return;
        }
        this.applicationService.previousQuoteState = this.applicationService.quoteState;
        this.applicationService.quoteState = newQuoteState;
        // send a quote state changed message so the portal can respond to it by updating the list
        this.messageService.sendMessage('quoteStateChanged', {
            quoteId: this.applicationService.quoteId,
            previousQuoteState: this.applicationService.previousQuoteState,
            newQuoteState: newQuoteState,
        });
    }

    /**
     * detects whether the claim state has changed and sends a message/event to the parent frame (ie portal) 
     * so that it can respond, e.g by refreshing the item in the list.
     * @param newClaimState
     */
    private processClaimStateChange(newClaimState: ClaimState): void {
        const previousClaimState: ClaimState = this.applicationService.claimState;
        if (!newClaimState || previousClaimState?.toLowerCase() === newClaimState.toLowerCase()) {
            return;
        }
        this.applicationService.claimState = newClaimState;
        // send a claim state changed message so the portal can respond to it by updating the list
        this.messageService.sendMessage('claimStateChanged', {
            claimId: this.applicationService.claimId,
            previousClaimState: previousClaimState,
            newClaimState: newClaimState,
        });
    }

    private handleAbort(
        abortSubject: Subject<any>,
        requestsArray: Array<RequestOperation>,
        executionSubscriber: Subscriber<any>,
    ): SubscriptionLike {
        let cancelSubscription: SubscriptionLike;
        if (abortSubject) {
            cancelSubscription = abortSubject.subscribe(
                () => { // called when next
                    this.processAbort();
                    requestsArray.forEach((item: any) => {
                        item.request.unsubscribe();
                    });
                    executionSubscriber.complete();
                },
                () => { // called on complete
                    this._operationInProgress = false;
                    this.inProgressSubject.next(false);
                    this.nextResult.emit({ 'status': 'cancelled' });
                    this.decrementExecutionQueueLength();
                    cancelSubscription.unsubscribe();
                },
            );
        }
        return cancelSubscription;
    }

    /**
     * To be overridden for operations which need to do something when they are aborted/cancelled
     */
    protected processAbort(): void {
    }
}
