import { Injectable } from "@angular/core";
import { DataTableDefinitionResourceModel } from "@app/resource-models/data-table-definition.resource-model";
import { Observable } from "rxjs";
import { DataTableDefinitionApiService } from "./api/data-table-definition-api.service";
import { EntityLoaderService } from "./entity-loader.service";
import { DataTableDefinitionViewModel } from "@app/viewmodels/data-table-definition.viewmodel";
import { JsonValidator } from "@app/helpers/json-validator";
import { ErrorCodeTranslationHelper } from "@app/helpers/error-code-translation.helper";
import {
    AbstractControl, FormGroup, ValidationErrors, ValidatorFn,
} from '@angular/forms';
/**
 * Data table definition entity service.
 */
@Injectable({ providedIn: 'root' })
export class DataTableDefinitionService implements EntityLoaderService<DataTableDefinitionResourceModel> {

    private tableSchemaValidationSchema: string = null;

    public constructor(
        private dataTableDefinitionApiService: DataTableDefinitionApiService,
    ) {
    }

    public getList(params?: Map<string, string | Array<string>>): Observable<Array<DataTableDefinitionResourceModel>> {
        return this.dataTableDefinitionApiService.getDataTableDefinitions(params);
    }

    public getById(
        id: string,
        params?: Map<string, string | Array<string>>,
    ): Observable<DataTableDefinitionResourceModel> {
        const tenant: string = params.get("tenant") as string;
        return this.dataTableDefinitionApiService.getDataTableDefinitionById(tenant, id);
    }

    public async jsonTableSchemaValidator(): Promise<ValidatorFn> {
        const validationSchema: string =  await this.getTableSchemaValidationSchema();
        return (control: AbstractControl): ValidationErrors | null => {
            const validationResult: string | null = JsonValidator.assertSchema(
                validationSchema,
                control.value,
                { removeAdditional: false });
            if (validationResult != null) {
                return { jsonAssertionFailed: true };
            }
            control.markAsUntouched();
            return null;
        };
    }

    public highlightCsvError(
        formGroup: FormGroup, errorResponse: any): void {
        let errorCode: string = errorResponse?.error?.code ?? "";
        let errorMessage: string =  errorResponse?.error?.title ?? "";
        const csvDataAlias: string = DataTableDefinitionViewModel.dataTableFormItemAlias.csvData;
        const csvErrors: Array<boolean> = [
            ErrorCodeTranslationHelper.isDatatableCsvColumnHeaderNameNotFound(errorCode),
            ErrorCodeTranslationHelper.isDatatableCsvDataRequiredColumnNotFound(errorCode),
            ErrorCodeTranslationHelper.isDatatableCsvDataColumnValueNotUnique(errorCode),
        ];
        if (csvErrors.includes(true)) {
            formGroup.get(csvDataAlias).setErrors({ invalidCsv: { message: errorMessage } });
            formGroup.get(csvDataAlias).markAsTouched({ onlySelf: true });
        }
    }

    private async getTableSchemaValidationSchema(): Promise<string> {
        if (this.tableSchemaValidationSchema == null) {
            this.tableSchemaValidationSchema =
                await this.dataTableDefinitionApiService.getTableSchemaValidationSchema().toPromise();
        }
        return this.tableSchemaValidationSchema;
    }

}
