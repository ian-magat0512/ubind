import { HttpClient, HttpParams } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { AppConfig } from "@app/models/app-config";
import { EntityType } from "@app/models/entity-type.enum";
import { DataTableCreateFromCsvModel } from "@app/resource-models/data-table-create-from-csv.resource-model";
import { DataTableDefinitionResourceModel } from "@app/resource-models/data-table-definition.resource-model";
import { Observable } from "rxjs";
import { AppConfigService } from "../app-config.service";
import { ApiHelper } from "@app/helpers";

/**
 * Service for handling request for data table definition.
 */
@Injectable({
    providedIn: 'root',
})
export class DataTableDefinitionApiService {
    private baseUrl: string;

    public constructor(
        private httpClient: HttpClient,
        appConfigService: AppConfigService,
    ) {
        appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseUrl = appConfig.portal.api.baseUrl + 'data-table-definition';
        });
    }

    public getDataTableDefinitions(
        params: Map<string, string | Array<string>>,
    ): Observable<Array<DataTableDefinitionResourceModel>> {
        return this.httpClient
            .get<Array<DataTableDefinitionResourceModel>>(this.baseUrl, ApiHelper.toHttpOptions(params));
    }

    public getDataTableDefinitionById(tenant: string, entityId: string): Observable<DataTableDefinitionResourceModel> {
        let params: HttpParams = new HttpParams();
        params = params.append("tenant", tenant);
        return this.httpClient.get<DataTableDefinitionResourceModel>(this.baseUrl + `/${entityId}`, { params });
    }

    public createDataTableFromCsv(
        tenant: string,
        entityType: EntityType,
        entityIdOrAlias: string,
        model: DataTableCreateFromCsvModel,
    ): Observable<DataTableDefinitionResourceModel> {
        let params: HttpParams = new HttpParams();
        params = params.append("tenant", tenant);
        params = params.append("entityType", entityType);
        params = params.append("entityIdOrAlias", entityIdOrAlias);
        return this.httpClient.post<DataTableDefinitionResourceModel>(this.baseUrl, model, { params });
    }

    public updateDataTableFromCsvData(
        tenant: string,
        model: DataTableCreateFromCsvModel,
    ): Observable<DataTableDefinitionResourceModel> {
        let params: HttpParams = new HttpParams();
        params = params.append("tenant", tenant);
        return this.httpClient.put<DataTableDefinitionResourceModel>(this.baseUrl, model, { params });
    }

    public deleteDataTableDefinition(tenant: string, definitionId: string): Observable<void> {
        let params: HttpParams = new HttpParams();
        params = params.append("tenant", tenant);

        return this.httpClient.delete<void>(this.baseUrl + `/${definitionId}`, { params });
    }

    public getTableSchemaValidationSchema(): Observable<string> {
        return this.httpClient.get<string>(this.baseUrl + "/validation-schema");
    }

}
