import { InjectionToken } from "@angular/core";
import { AccountApiService } from "@app/services/api/account-api.service";
import { CustomerApiService } from "@app/services/api/customer-api.service";
import { PersonApiService } from "@app/services/api/person-api.service";
import { ReportApiService } from "@app/services/api/report-api.service";
import { RoleApiService } from "@app/services/api/role-api.service";
import { UserApiService } from "@app/services/api/user-api.service";
import { EntityLoaderSaverService } from "@app/services/entity-loader-saver.service";

// This use to DI services that implements on different classes. 
// This will serve as a token for the service so when passing it as parameter
// it will be identified as instance of the service.
// please refer on this link for more info: https://v9.angular.io/guide/migration-injectable
export const entityLoaderSaverServiceToken: InjectionToken<EntityLoaderSaverService<any>> =
    new InjectionToken<EntityLoaderSaverService<any>>('EntityLoaderSaverService');

export const serviceProvidersToken: Array<any> = [
    AccountApiService,
    { provide: entityLoaderSaverServiceToken, useExisting: AccountApiService },
    CustomerApiService,
    { provide: entityLoaderSaverServiceToken, useExisting: CustomerApiService },
    PersonApiService,
    { provide: entityLoaderSaverServiceToken, useExisting: PersonApiService },
    ReportApiService,
    { provide: entityLoaderSaverServiceToken, useExisting: ReportApiService },
    RoleApiService,
    { provide: entityLoaderSaverServiceToken, useExisting: RoleApiService },
    UserApiService,
    { provide: entityLoaderSaverServiceToken, useExisting: UserApiService },
];
