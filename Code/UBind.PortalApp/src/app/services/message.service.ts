import { Injectable } from '@angular/core';
import { EmailResourceModel } from '@app/resource-models/email.resource-model';
import { SmsResourceModel } from '@app/resource-models/sms.resource-model';
import { RouteHelper } from '@app/helpers/route.helper';
import { EntityType } from '@app/models/entity-type.enum';
import { MessageResourceModel } from '@app/resource-models/message.resource-model';
import { Observable } from 'rxjs';
import { map, mergeMap } from 'rxjs/operators';
import { EmailApiService } from './api/email-api.service';
import { SmsApiService } from './api/sms-api.service';
import { EntityLoaderService } from './entity-loader.service';

/**
 * Service to handle email and sms.
 */
@Injectable({ providedIn: 'root' })
export class MessageService implements EntityLoaderService<MessageResourceModel> {
    public constructor(
        private emailApiService: EmailApiService,
        private smsApiService: SmsApiService,
        private routeHelper: RouteHelper,
    ) { }

    private emailMap: (value: EmailResourceModel, index: number) => MessageResourceModel =
        (e: EmailResourceModel) => {
            const message: MessageResourceModel = {
                id: e.id,
                recipient: e.recipient,
                subject: e.subject,
                createdDateTime: e.createdDateTime,
                message: e.htmlMessage,
                tags: e.tags,
                relationship: e.relationship,
                type: 'email',
            };
            return message;
        };

    private smsMap: (value: SmsResourceModel, index: number) => MessageResourceModel =
        (s: SmsResourceModel) => {
            const message: MessageResourceModel = {
                id: s.id,
                recipient: s.to,
                subject: s.message,
                createdDateTime: s.createdDateTime,
                message: s.message,
                tags: s.tags,
                relationship: s.relationship,
                type: 'sms',
            };
            return message;
        };

    private emailArrayMap: (value: Array<EmailResourceModel>, index: number) => Array<MessageResourceModel> =
        (e: Array<EmailResourceModel>) => e.map(this.emailMap);

    private smsArrayMap: (value: Array<SmsResourceModel>, index: number) => Array<MessageResourceModel> =
        (e: Array<SmsResourceModel>) => e.map(this.smsMap);

    public getList(params?: Map<string, string | Array<string>>): Observable<Array<MessageResourceModel>> {
        return this.emailApiService.getList(params)
            .pipe(
                map(this.emailArrayMap),
                mergeMap((e: Array<MessageResourceModel>) => this.smsApiService.getList(params)
                    .pipe(
                        map(this.smsArrayMap),
                        map((s: Array<MessageResourceModel>) => e.concat(s)),
                    )),
            );
    }

    public getById(id: string, params?: Map<string, string | Array<string>>): Observable<MessageResourceModel> {
        const messageType: string = this.routeHelper.getQueryParam("type");

        if (messageType == "sms") {
            return this.smsApiService.getById(id, params)
                .pipe(map(this.smsMap));
        }

        return this.emailApiService.getById(id, params)
            .pipe(map(this.emailMap));
    }

    public getEntityEmails(entityType: EntityType, entityId: string, params?: Map<string, string | Array<string>>,
    ): Observable<Array<MessageResourceModel>> {
        return this.emailApiService.getEntityEmails(entityType, entityId, null, params)
            .pipe(
                map(this.emailArrayMap),
                mergeMap((e: Array<MessageResourceModel>) =>
                    this.smsApiService.getEntitySms(entityType, entityId, null, params)
                        .pipe(
                            map(this.smsArrayMap),
                            map((s: Array<MessageResourceModel>) => e.concat(s)),
                        )),
            );
    }
}
