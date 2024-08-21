import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApplicationService } from '@app/services/application.service';
import { ApiService } from '../api.service';
import { LogLevel } from '@app/models/log-level.enum';
import { LoggerService } from '@app/services/logger.service';
import { FormType } from '@app/models/form-type.enum';
import { HttpClient } from '@angular/common/http';
import { IQumulateRequestResourceModel } from '@app/resource-models/iqumulate-request-resource-model';
import { IQumlateFundingResponseResourceModel } from '@app/resource-models/iqumulate-funding-response-resource-model';

/**
 * Provides API services in relation to the IQumulate Premium Funding service, including
 * retreiving from the uBind API the request data needed by IQumulate, and also for
 * posting response data received from the IQumulate IFrame.
 */
@Injectable()
export class IqumulatePremiumFundingApiService {
    public operationName: string = 'iQumulateMPF';
    protected applicableFormType: FormType = FormType.Quote;
    protected response: any;
    protected includeFormModel: boolean = true;
    protected includeApplicationProperties: Array<string> = [
        'quoteId',
        'calculationResultId',
    ];

    public fieldMappingsObject: any;
    public formModel: any;
    public calculationModel: any;
    public mappings: any;

    public constructor(
        private httpClient: HttpClient,
        protected applicationService: ApplicationService,
        protected logger: LoggerService,
        protected apiService: ApiService,
    ) {
    }

    public getRequestData(): Observable<IQumulateRequestResourceModel> {
        let tenant: string = this.applicationService.tenantAlias;
        let product: string = this.applicationService.productAlias;
        let environment: string = this.applicationService.environment;
        let apiUrl: string = `/api/v1/${tenant}/${product}/${environment}/iqumulate/funding-request-data`;
        const params: any = {
            quoteId: this.applicationService.quoteId,
            calculationResultId: this.applicationService.calculationResultId,
        };
        return this.httpClient.get<IQumulateRequestResourceModel>(apiUrl, { params });
    }

    public postResponse(premiumFundingResponseData: any): Observable<object> {
        let fundingResponseModel: IQumlateFundingResponseResourceModel = {
            quoteId: this.applicationService.quoteId,
            pageResponse: premiumFundingResponseData,
            calculationResultId: this.applicationService.calculationResultId,
        };
        this.logger.log(
            LogLevel.Information,
            'Posting Iqumulate payment response',
            JSON.stringify(fundingResponseModel));
        let tenant: string = this.applicationService.tenantAlias;
        let product: string = this.applicationService.productAlias;
        let environment: string = this.applicationService.environment;
        let apiUrl: string = `/api/v1/${tenant}/${product}/${environment}/iqumulate/funding-response`;
        return this.httpClient.post(
            apiUrl,
            fundingResponseModel);
    }
}
