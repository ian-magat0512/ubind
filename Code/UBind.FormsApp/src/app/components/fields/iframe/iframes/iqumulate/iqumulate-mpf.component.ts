import { Component, OnInit, OnDestroy, Input, ViewChild, AfterViewInit, EventEmitter, Output } from '@angular/core';
import { StringHelper } from '@app/helpers/string.helper';
import { Errors } from '@app/models/errors';
import { IQumulateRequestData } from '@app/resource-models/iqumulate-request-data';
import { IQumulateResponseCode, IQumulateResponseData } from '@app/resource-models/iqumulate-response-data';

/**
 * Export IQumulate Mpf component class
 * this component houses the converted code from the JS file provided by Iqumulate
 */
@Component({
    selector: 'app-iQumulate-mpf',
    templateUrl: './iqumulate-mpf.component.html',
    styles: [],
})

export class IQumulateMpfComponent implements OnInit, OnDestroy, AfterViewInit {
    @ViewChild('iQumulateForm') public iQumulateForm: any;
    @Input() public action: any;
    @Input() public baseUrl: any;
    @Input() public messageOriginUrl: any;
    @Input() public parameters: IQumulateRequestData;
    @Input() public targetIframe: string;
    @Input() public isShown: boolean = true;
    @Input() public isFundingAccepted: boolean = false;

    @Output() public fundingAccepted: EventEmitter<any> = new EventEmitter<any>();
    @Output() public cancelFunding: EventEmitter<any> = new EventEmitter<any>();
    @Output() public finalizeFunding: EventEmitter<any> = new EventEmitter<any>();
    @Output() public loadCompleted: EventEmitter<any> = new EventEmitter<any>();
    public mpfFields: Array<any> = [];
    public mpfResponse: IQumulateResponseData;
    public isLoading: boolean = true;
    public errorMessage: string;
    private messageEventHandler: (event: MessageEvent) => void;

    public constructor() {
    }

    public ngOnInit(): void {
        this.parameters.host = {
            host: window.location.host,
            origin: window.location.origin,
            protocol: window.location.protocol,
        };
        this.mpfFields = this.buildParameters(this.parameters);
        this.messageEventHandler = this.handleIframeTask.bind(this);
        window.addEventListener('message', this.messageEventHandler);
    }

    public ngOnDestroy(): void {
        window.removeEventListener('message', this.messageEventHandler);
    }

    public handleIframeTask(event: MessageEvent): void {
        if (event.origin != this.messageOriginUrl) {
            // Ignoring message from ${event.origin} because it does not match 
            // the expected ${this.messageOriginUrl}
            // some postmessage calls are meant for the ubind main frame or it's parent frame
            // so we just ignore these.
            return;
        }
        const response: any = JSON.parse(event.data);
        switch (response.callback) {
            case 'brokerCallback': {
                this.mpfResponse = JSON.parse(response.data);
                switch (this.mpfResponse.General.responseCode) {
                    case IQumulateResponseCode.Success:
                        break;
                    case IQumulateResponseCode.ErrorOccured:
                        throw Errors.IQumulate.RequiredDataAbsentFromQuoteRequest(
                            this.mpfResponse.General.responseDescription);
                    case IQumulateResponseCode.ErrorOccuredDuringPaymentProcessing:
                        throw Errors.IQumulate.CreditCardProcessingError(
                            this.mpfResponse.General.responseDescription);
                    case IQumulateResponseCode.ForcedClose:
                        console.log('User closure of modal window.');
                        break;
                    default:
                        throw Errors.IQumulate.UnknownResponseCode(
                            this.mpfResponse.General.responseDescription);
                }
                console.log(this.mpfResponse.General.responseDescription);
                if (this.mpfResponse.Documents && this.mpfResponse.Documents.length > 0) {
                    this.isFundingAccepted = true;
                    this.fundingAccepted.emit(this.mpfResponse);
                } else {
                    throw Errors.IQumulate.NoDocumentsGenerated(this.mpfResponse.General.responseDescription);
                }
                break;
            }
            case 'closeMpfModalContnetCloseButton': {
                if (this.isFundingAccepted) {
                    this.finalizeFunding.emit();
                } else {
                    this.cancelFunding.emit();
                }
                break;
            }
            case 'iFrameLoadComplete': {
                if (!this.isFundingAccepted) {
                    this.loadCompleted.emit();
                }
                this.isLoading = false;
                break;
            }
        }
    }

    public ngAfterViewInit(): void {
        this.iQumulateForm.nativeElement.submit();
    }

    public buildParameters(params: any): any {
        const paramList: Array<string> = Object.getOwnPropertyNames(params);
        const paramArray: Array<any> = [];
        if (paramList && paramList.length > 0) {
            // Create a hidden field for each parameter
            for (let name of paramList) {
                let value: any = params[name];
                try {
                    value = JSON.stringify(value);
                } catch (e) {
                }
                name += 'JSON';
                value = value.replace(/\\/g, '');
                name = name.replace(/\\/g, '');
                name = StringHelper.capitalizeFirstLetter(name);
                paramArray.push({ name: name, value: value });
            }
            return paramArray;
        }
    }
}
