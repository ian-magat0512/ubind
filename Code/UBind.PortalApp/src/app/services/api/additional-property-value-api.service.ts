import { Injectable } from '@angular/core';
import { AdditionalPropertyValue } from '@app/models/additional-property-item-view.model';
import { HttpClient } from '@angular/common/http';
import { AppConfigService } from '@app/services/app-config.service';
import { AppConfig } from '@app/models/app-config';
import { Observable } from 'rxjs/internal/Observable';
import { AdditionalPropertyDefinitionTypeEnum } from '@app/models/additional-property-definition-types.enum';
import { ApiHelper } from '@app/helpers';
import {
    AdditionalPropertyValueUpsertResourceModel,
    UpdateAdditionalPropertyValuesResourceModel,
} from '@app/resource-models/additional-property.resource-model';
import { EntityType } from '@app/models/entity-type.enum';

/**
 * Service for additional properties.
 */
@Injectable({ providedIn: 'root' })
export class AdditionalPropertyValueApiService {
    private baseApiUrl: string;
    public constructor(
        private httpClient: HttpClient,
        private appConfigService: AppConfigService,
    ) {
        appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseApiUrl = appConfig.portal.api.baseUrl + 'additional-property-value';
        });
    }

    public getValuesByValueAndAdditionalPropertyDefinitionId(
        id: string,
        inputValue: string,
        propertyType: AdditionalPropertyDefinitionTypeEnum,
        tenantId: string,
        entityType: EntityType,
        entityId: string,
    ): Observable<Array<AdditionalPropertyValue>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set("additionalPropertyDefinitionId", id);
        params.set("propertyType", propertyType);
        params.set("value", inputValue);
        params.set("tenantId", tenantId);
        params.set("entityType", entityType);
        params.set("entityId", entityId);
        return this.httpClient.get<Array<AdditionalPropertyValue>>(this.baseApiUrl, ApiHelper.toHttpOptions(params));
    }

    public isUnique(
        id: string,
        inputValue: string,
        propertyType: AdditionalPropertyDefinitionTypeEnum,
        tenantId: string,
        entityType: EntityType,
        entityId: string,
    ): Observable<boolean> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set("additionalPropertyDefinitionId", id);
        params.set("propertyType", propertyType);
        params.set("value", inputValue);
        params.set("tenantId", tenantId);
        params.set("entityType", entityType);
        params.set("entityId", entityId);
        return this.httpClient.get<boolean>(`${this.baseApiUrl}/unique`, ApiHelper.toHttpOptions(params));
    }

    public getValuesByEntityId(entityId: string, tenantAlias: string): Observable<Array<AdditionalPropertyValue>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set("entityId", entityId);
        params.set("tenantId", tenantAlias);
        return this.httpClient.get<Array<AdditionalPropertyValue>>(this.baseApiUrl, ApiHelper.toHttpOptions(params));
    }

    public getValuesByEntityType(
        entityType: EntityType,
        entityId: string,
        tenantId: string,
    ): Observable<Array<AdditionalPropertyValue>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set("entityType", entityType);
        params.set("entityId", entityId);
        params.set("tenantId", tenantId);
        return this.httpClient.get<Array<AdditionalPropertyValue>>(this.baseApiUrl, ApiHelper.toHttpOptions(params));
    }

    public updatePropertValues(
        properties: Array<AdditionalPropertyValueUpsertResourceModel>,
        entityId: string,
        entityType: EntityType,
    ): Observable<string> {
        const url: string = `${this.baseApiUrl}/${entityId}`;
        const model: UpdateAdditionalPropertyValuesResourceModel = {
            entityType: entityType,
            properties: properties,
            environment: this.appConfigService.getEnvironment(),
        };
        return this.httpClient.put<string>(url, model);
    }
}
