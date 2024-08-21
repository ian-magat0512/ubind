import { Injectable } from '@angular/core';
import { AdditionalPropertyDefinitionContextType } from '@app/models/additional-property-context-type.enum';
import { EntityType } from '@app/models/entity-type.enum';
import { AdditionalPropertyDefinition } from '@app/models/additional-property-item-view.model';
import { HttpClient } from '@angular/common/http';
import { AppConfigService } from '@app/services/app-config.service';
import { AppConfig } from '@app/models/app-config';
import { AdditionalPropertyDefinitionResourceModel }
    from '@app/resource-models/additional-property-definition.resource-model';
import { Observable } from 'rxjs/internal/Observable';
import { ApiHelper } from '@app/helpers';
import * as ChangeCase from 'change-case';
import { AdditionalPropertyDefinitionSchemaTypeEnum } from '@app/models/additional-property-schema-type.enum';

/**
 * Service for additional properties.
 */
@Injectable({ providedIn: 'root' })
export class AdditionalPropertyDefinitionApiService {
    private baseApiUrl: string;

    public constructor(
        private httpClient: HttpClient,
        appConfigService: AppConfigService,
    ) {
        appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseApiUrl = appConfig.portal.api.baseUrl + 'additional-property-definition';
        });
    }

    public create(
        model: AdditionalPropertyDefinitionResourceModel,
    ): Observable<AdditionalPropertyDefinitionResourceModel> {
        return this.httpClient.post<AdditionalPropertyDefinitionResourceModel>(this.baseApiUrl, model);
    }

    public update(
        model: AdditionalPropertyDefinitionResourceModel,
    ): Observable<AdditionalPropertyDefinitionResourceModel> {
        let url: string = this.baseApiUrl + '/' + model.id;
        return this.httpClient.put<AdditionalPropertyDefinitionResourceModel>(url, model);
    }

    public delete(tenant: string, additionalPropertyId: string): Observable<void> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set("tenant", tenant);
        let url: string = this.baseApiUrl + '/' + additionalPropertyId;
        return this.httpClient.delete<void>(url, ApiHelper.toHttpOptions(params));
    }

    public getAdditionalPropertyDefinitionsByContextTypeAndContextId(
        tenant: string,
        contextType: AdditionalPropertyDefinitionContextType,
        contextId: string,
    ): Observable<Array<AdditionalPropertyDefinition>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set("tenant", tenant);
        params.set("contextType", contextType);
        params.set("contextId", contextId);
        return this.getList(params);
    }

    private getList(params?: Map<string, string | Array<string>>): Observable<Array<AdditionalPropertyDefinition>> {
        return this.httpClient.get<Array<AdditionalPropertyDefinition>>(
            this.baseApiUrl,
            ApiHelper.toHttpOptions(params),
        );
    }

    public getAdditionalPropertyDefinitionsByContextTypeAndContextIdAndParentContextId(
        tenant: string,
        contextType: AdditionalPropertyDefinitionContextType,
        contextId: string,
        parentContextId: string,
    ): Observable<Array<AdditionalPropertyDefinition>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set("tenant", tenant);
        params.set("contextType", contextType);
        params.set("contextId", contextId);
        params.set("parentContextId", parentContextId);
        return this.getList(params);
    }

    public getAdditionalPropertyDefinitionsByContextTypeAndEntityTypeAndContextId(
        tenant: string,
        contextType: AdditionalPropertyDefinitionContextType,
        entityType: EntityType,
        contextId: string,
    ): Observable<Array<AdditionalPropertyDefinition>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set("tenant", tenant);
        params.set("contextType", contextType);
        params.set("contextId", contextId);
        params.set("entity", entityType);
        return this.getList(params);
    }

    public getAdditionalPropertyDefinitionsByContextTypeAndEntityTypeAndContextIdAndParentContextId(
        tenant: string,
        contextType: AdditionalPropertyDefinitionContextType,
        entityType: EntityType,
        contextId: string,
        parentContextId: string,
        mergeResult: boolean = false,
    ): Observable<Array<AdditionalPropertyDefinition>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set("tenant", tenant);
        params.set("contextType", contextType);
        params.set("contextId", contextId);
        params.set("entity", entityType);
        params.set("parentContextId", parentContextId);
        if (mergeResult) {
            params.set("mergeResult", "true");
        }
        return this.getList(params);
    }

    public getAdditionalPropertyDefinitionsByContextAndEntityAndParentContextId(
        tenant: string,
        contextType: AdditionalPropertyDefinitionContextType,
        contextId: string,
        entityType: EntityType,
        entityId: string,
        parentContextId: string,
        mergeResult: boolean = false,
    ): Observable<Array<AdditionalPropertyDefinition>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set("tenant", tenant);
        params.set("contextType", contextType);
        params.set("contextId", contextId);
        params.set("entity", entityType);
        params.set("entityId", entityId);
        params.set("parentContextId", parentContextId);
        if (mergeResult) {
            params.set("mergeResult", "true");
        }
        return this.getList(params);
    }

    public isNameAvailable(
        tenant: string,
        contextType: AdditionalPropertyDefinitionContextType,
        entityType: EntityType,
        contextId: string,
        parentContextId: string,
        name: string,
        id: string,
    ): Observable<boolean> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set("name", name);
        return this.hasAdditionalPropertyDefinitionWithAliasOrName(
            tenant,
            contextType,
            entityType,
            contextId,
            parentContextId,
            id,
            params,
        );
    }

    public isAliasAvailable(
        tenant: string,
        contextType: AdditionalPropertyDefinitionContextType,
        entityType: EntityType,
        contextId: string,
        parentContextId: string,
        alias: string,
        id: string,
    ): Observable<boolean> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set("alias", alias);
        return this.hasAdditionalPropertyDefinitionWithAliasOrName(
            tenant,
            contextType,
            entityType,
            contextId,
            parentContextId,
            id,
            params,
        );
    }

    public getDefaultSchema(schemaType: AdditionalPropertyDefinitionSchemaTypeEnum): Observable<string> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set("schemaType", schemaType);
        let url: string = this.baseApiUrl + '/default-schema';
        return this.httpClient.get<string>(url, ApiHelper.toHttpOptions(params));
    }

    private hasAdditionalPropertyDefinitionWithAliasOrName(
        tenant: string,
        contextType: AdditionalPropertyDefinitionContextType,
        entityType: EntityType,
        contextId: string,
        parentContextId: string,
        id: string,
        params: Map<string, string | Array<string>>,
    ): Observable<boolean> {
        params.set("tenant", tenant);
        params.set("contextType", contextType);
        params.set("entity", entityType);
        params.set("contextId", contextId);
        if (parentContextId) {
            params.set("parentContextId", parentContextId);
        }
        let url: string = !id ? this.baseApiUrl + '/verify-name-or-alias'
            : this.baseApiUrl + '/verify-name-or-alias/' + id;
        return this.httpClient.get<boolean>(url, ApiHelper.toHttpOptions(params));
    }

    public getAdditionalPropertyDefinitionById(
        tenant: string,
        id: string,
    ): Observable<AdditionalPropertyDefinition> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set("tenant", tenant);
        let url: string = this.baseApiUrl + '/' + id;
        return this.httpClient.get<AdditionalPropertyDefinition>(url, ApiHelper.toHttpOptions(params));
    }

    public getEntityDescriptionInPluralForm(entityType: EntityType): string {
        if (entityType == EntityType.Policy) {
            return "policies";
        } else {
            return ChangeCase.sentenceCase(entityType).toLowerCase() + 's';
        }
    }

    public getIconByContextType(contextType: AdditionalPropertyDefinitionContextType): string {
        let icon: string = 'none';
        switch (contextType) {
            case AdditionalPropertyDefinitionContextType.Product:
                icon = 'cube';
                break;
            case AdditionalPropertyDefinitionContextType.Tenant:
                icon = 'cloud-circle';
                break;
            case AdditionalPropertyDefinitionContextType.Organisation:
                icon = 'business';
                break;
        }
        return icon;
    }
}
