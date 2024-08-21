
import { takeUntil } from 'rxjs/operators';
import { Component, Output, EventEmitter, ElementRef, OnInit, OnDestroy } from '@angular/core';
import { Field } from '../field';
import { ApplicationService } from '@app/services/application.service';
import { FormService } from '@app/services/form.service';
import { AttachmentService } from '@app/services/attachment.service';
import { AttachmentOperation } from '@app/operations/attachment.operation';
import { WorkflowService } from '@app/services/workflow.service';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { AttachmentFileProperties } from '@app/models/attachment-file-properties';
import { CalculationService } from '@app/services/calculation.service';
import { FormUpdateOperation } from '@app/operations/form-update.operation';
import { HttpErrorResponse } from '@angular/common/http';
import { BroadcastService } from '@app/services/broadcast.service';
import { Subscription } from 'rxjs';
import { FieldMetadataService } from '@app/services/field-metadata.service';
import { FieldType } from '@app/models/field-type.enum';
import { FieldSelector } from '@app/models/field-selectors.enum';
import { Errors } from '@app/models/errors';
import { EventService } from '@app/services/event.service';
import { UuidHelper } from '@app/helpers/uuid.helper';
import { FieldEventLogRegistry } from '@app/services/debug/field-event-log-registry';
import { OperationArguments } from '@app/operations/operation';

/**
 * Export attachment field component class.
 * This class manage attachment field functions.
 */
@Component({
    selector: '' + FieldSelector.Attachment,
    templateUrl: './attachment.field.html',
    styleUrls: [
        './attachment.field.scss',
    ],
})
export class AttachmentField extends Field implements OnInit, OnDestroy {

    @Output() public cancel: EventEmitter<any> = new EventEmitter<any>();

    private inputField: any;
    private file: File;
    private fileReader: FileReader;
    public imageElement: any;
    public uploadStatus: string = 'none';
    public uploadPercentComplete: number;

    public fileName: string;
    public mimeType: any;
    private fileData: any;
    public fileSizeBytes: any;
    public imageWidth: any;
    public imageHeight: any;
    public attachmentId: any;

    private broadcast$: Subscription;

    /**
     * A version of the valueProperties which can be output for debugging purposes.
     * We need a version without the fileData, since if we try to print the fileData
     * the browser will likely crash for a large file, plus it won't be very useful
     * to fill up the screen.
     */
    public debugValueProperties: object;

    public constructor(
        public applicationService: ApplicationService,
        protected formService: FormService,
        protected attachmentOperation: AttachmentOperation,
        protected attachmentService: AttachmentService,
        protected workflowService: WorkflowService,
        protected elementRef: ElementRef,
        protected expressionDependencies: ExpressionDependencies,
        calculationService: CalculationService,
        private formUpdateOperation: FormUpdateOperation,
        protected broadcast: BroadcastService,
        fieldMetadataService: FieldMetadataService,
        eventService: EventService,
        fieldEventLogRegistry: FieldEventLogRegistry,
    ) {
        super(
            formService,
            workflowService,
            expressionDependencies,
            calculationService,
            applicationService,
            fieldMetadataService,
            eventService,
            fieldEventLogRegistry);

        this.fieldType = FieldType.Attachment;
    }

    public ngOnInit(): void {
        super.ngOnInit();
        this.fileReader = new FileReader();
        this.fileReader.onloadend = this.onFileRead.bind(this);
        this.inputField = this.elementRef.nativeElement.getElementsByClassName('file-selector')[0];
        if (this.initialValue && this.initialValue.indexOf(':') != -1) {
            this.processLoadedAttachment(this.initialValue);
        }
        this.onUpdateDisabledState();

        if (this.broadcast$) {
            this.broadcast$.unsubscribe();
        }

        this.broadcast$ = this.broadcast.on('Error500Event').subscribe(() => {
            if (this.uploadStatus == 'sending') {
                this.updateFieldValue();
                this.uploadStatus = 'error';
                this.cancel.emit({});
                this.attachmentOperation.decrementExecutionQueueLength();
            }
        });
    }

    protected onUpdateDisabledState(): void {
        if (this.disabledExpression) {
            this.disabledExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((disabled: boolean) => this.setDisabledAttribute(disabled));
        }
    }

    protected setDisabledAttribute(condition: boolean): void {
        let buttons: HTMLCollection = this.elementRef.nativeElement.getElementsByTagName('button') as HTMLCollection;
        let inputs: HTMLCollection = this.elementRef.nativeElement.getElementsByTagName('input') as HTMLCollection;
        // eslint-disable-next-line @typescript-eslint/prefer-for-of
        for (let i: number = 0; i < buttons.length; i++) {
            let button: Element = buttons[i];
            if (condition) {
                button.setAttribute('disabled', 'true');
            } else {
                button.removeAttribute('disabled');
            }
        }
        // eslint-disable-next-line @typescript-eslint/prefer-for-of
        for (let i: number = 0; i < inputs.length; i++) {
            let input: Element = inputs[i];
            if (condition) {
                input.setAttribute('disabled', 'true');
            } else {
                input.removeAttribute('disabled');
            }
        }
    }

    protected processLoadedAttachment(value: any): void {
        const propertiesArray: any = value.split(':');
        this.fileName = propertiesArray[0];
        this.mimeType = propertiesArray[1];
        this.attachmentId = propertiesArray[2];
        this.imageWidth = propertiesArray[3];
        this.imageHeight = propertiesArray[4];
        let fileProperties: AttachmentFileProperties = this.attachmentService.getAttachment(this.attachmentId);
        if (fileProperties != null) {
            this.fileData = fileProperties['fileData'];
            this.fileSizeBytes = fileProperties['fileSizeBytes'];
        } else {
            this.fileSizeBytes = propertiesArray[5];
        }
        this.uploadStatus = 'complete';
        this.updateFieldValue();
    }

    public onChange(event: any): void {
        if (this.inputField) {
            this.file = this.inputField.files[0];
            if (this.file != null && event) {
                this.uploadStatus = 'queued';
                this.formControl.markAsUntouched();
                this.formControl['uploading'] = true;
                this.fileReader.readAsDataURL(this.file);
            }
        }
    }

    public onFileRead(event: any): void {
        // Set the attachment Id to a new Guid so we don't need to wait
        // for the upload completion before we can proceed to the next page
        this.attachmentId = UuidHelper.newGuid();
        this.fileName = this.file.name;
        this.mimeType = this.file.type || 'text/plain';
        this.fileData = (this.fileReader.result as string).replace(/^data:(.*;base64,)?/, '');
        this.fileSizeBytes = Math.round((this.fileData.length * 6) / 8);
        this.imageWidth = null;
        this.imageHeight = null;
        if (this.mimeType.startsWith('image/')) {
            const imageSrc: string = `data:${this.mimeType};base64, ${this.fileData}`;
            this.imageElement = document.createElement("img");
            this.imageElement.onload = this.onImageLoad.bind(this);
            this.imageElement.src = imageSrc;
        } else {
            this.storeAndUpload(event);
        }
    }

    public onImageLoad(event: any): void {
        this.imageWidth = this.imageElement.naturalWidth;
        this.imageHeight = this.imageElement.naturalHeight;
        this.storeAndUpload(event);
    }

    private storeAndUpload(event: any): void {
        this.storeAttachment();
        this.updateFieldValue();
        this.formControl.updateValueAndValidity({ onlySelf: true, emitEvent: true });
        if (this.formControl.invalid) {
            this.uploadStatus = 'error';
        } else {
            this.uploadFileToServer(event);
        }
    }

    public uploadFileToServer(event: any): void {
        const requestParams: any = {
            fileProperties: this.getAttachmentFileProperties(),
        };
        const args: OperationArguments = {
            cancelEmitter: this.cancel,
            reportProgress: true,
        };
        this.attachmentOperation.execute(requestParams, args)
            .pipe(takeUntil(this.destroyed))
            .subscribe((data: any) => {
                let fileUploadStatus: string = data['status'];
                if (fileUploadStatus) {
                    if (fileUploadStatus == 'executing') {
                        this.uploadPercentComplete = 0;
                        this.uploadStatus = 'sending';
                    } else if (fileUploadStatus == 'success') {
                        let responseHasValidUUid: boolean = UuidHelper.isUuid(data['attachmentId'] || '');
                        if (responseHasValidUUid) {
                            this.attachmentId = data['attachmentId'];
                            this.storeAttachment();
                            this.updateFieldValue();
                            this.uploadStatus = 'complete';
                            this.uploadPercentComplete = Math.round(99 * (data['loaded'] / data['total']));
                            this.formUpdateOperation.execute(
                                this.formService.getValues(true, false, false, false, false))
                                .subscribe();
                        }  else {
                            this.updateFieldValue();
                            this.uploadStatus = 'error';
                            throw Errors.Attachments.InvalidResponseObtained(requestParams.fileProperties.fileName);
                        }
                    } else if (fileUploadStatus == 'uploading') {
                        this.uploadPercentComplete = Math.round(99 * (data['loaded'] / data['total']));
                        this.uploadStatus = 'sending';
                    }
                }
                if (data instanceof HttpErrorResponse) {
                    this.updateFieldValue();
                    this.uploadStatus = 'error';
                }
            },
            (error: HttpErrorResponse) => {
                this.updateFieldValue();
                this.uploadStatus = 'error';
                throw error;
            });
    }

    public storeAttachment(): void {
        let attachmentFileProperties: AttachmentFileProperties = this.getAttachmentFileProperties();
        this.attachmentService.setAttachment(attachmentFileProperties);
        this.data = attachmentFileProperties;
        let debugValueProperties: any = {
            fileName: attachmentFileProperties.fileName,
            mimeType: attachmentFileProperties.mimeType,
            fileSizeBytes: attachmentFileProperties.fileSizeBytes,
            fileData: '<Not included due to length>',
            imageWidth: attachmentFileProperties.imageWidth,
            imageHeight: attachmentFileProperties.imageHeight,
            attachmentId: attachmentFileProperties.attachmentId,
            quoteId: attachmentFileProperties.quoteId,
            claimId: attachmentFileProperties.claimId,
        };
        if (!attachmentFileProperties.quoteId) {
            delete attachmentFileProperties.quoteId;
            delete debugValueProperties.quoteId;
        }
        if (!attachmentFileProperties.claimId) {
            delete attachmentFileProperties.claimId;
            delete debugValueProperties.claimId;
        }
        this.debugValueProperties = debugValueProperties;
        if (attachmentFileProperties.attachmentId != this.fieldPath) {
            this.attachmentService.clearAttachment(this.fieldPath);
        }
    }

    public cancelUpload(event: any): void {
        this.cancel.emit({});
        this.formControl['uploading'] = false;
        this.deleteValue();
        this.attachmentOperation.decrementExecutionQueueLength();
    }

    protected updateFieldValue(): void {
        this.formControl.markAsTouched();
        this.formControl.setValue(this.getFilePropertiesAsString());
        this.formControl['uploading'] = false;
        super.onChange();
    }

    protected setFormControlValue(value: any): void {
        if (value == null) {
            this.deleteValue();
        }
    }

    protected getAttachmentFileProperties(): AttachmentFileProperties {
        return {
            fileName: this.fileName,
            mimeType: this.mimeType,
            fileData: this.fileData,
            fileSizeBytes: this.fileSizeBytes,
            imageWidth: this.imageWidth,
            imageHeight: this.imageHeight,
            attachmentId: this.attachmentId,
            quoteId: this.applicationService.quoteId,
            claimId: this.applicationService.claimId,
        };
    }

    protected getFilePropertiesAsString(): string {
        return this.fileName + ':' +
            this.mimeType + ':' +
            this.attachmentId + ':' +
            (this.imageWidth ? this.imageWidth : '') + ':' +
            (this.imageHeight ? this.imageHeight : '') + ':' +
            this.fileSizeBytes;
    }

    protected deleteValue(): void {
        if (this.inputField) {
            this.inputField.value = null;
            this.fileName = null;
            this.mimeType = null;
            this.attachmentId = null;
            this.fileData = null;
            this.fileSizeBytes = null;
            this.imageWidth = null;
            this.imageHeight = null;
            this.uploadStatus = 'none';
            this.formControl.setValue('');
            super.onChange(new CustomEvent("custom", {}));
        }
    }

    protected replaceValue(): void {
        this.inputField.click();
    }
}
