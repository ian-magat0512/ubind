import { HttpParams } from '@angular/common/http';
import { AppConfigService } from '../services/app-config.service';

/**
 * Export API helper class
 * This is the API routes.
 */
export class ApiHelper {
    public constructor(private appConfigService: AppConfigService) {
    }

    public static account: any = {
        route: `/account/`,
        params: {
            userId: 'userId',
        },
    };

    public static customer: any = {
        route: `/customer`,
        params: {
            customerId: 'customerId',
            status: 'status',
        },
    };

    public static customerByOwner: any = {
        route: `/customersbyowner`,
        params: {
            status: 'status',
            ownerAuthId: 'ownerAuthId',
        },
    };

    public static customerAssociateToUserAccount: any = {
        route: '/associate-user',
        params: {
            customerId: 'customerId',
        },
    };

    public static customerDeactivateUserAccount: any = {
        route: '/customer/block',
        params: {
            customerId: 'customerId',
        },
    };

    public static customerReactivateUserAccount: any = {
        route: '/customer/unblock',
        params: {
            customerId: 'customerId',
        },
    };

    public static customerDetails: any = {
        route: `/customer/details`,
    };

    public static deployment: any = {
        route: `/deployments/`,
    };

    public static deploymentForRelease: any = {
        route: `/deployments/`,
        params: {
            productId: 'productId',
        },
    };

    public static quoteDocument: any = {
        route: `/quote/document/`,
        params: {
            quoteId: 'quoteId',
        },
    };

    public static quoteVersionDocument: any = {
        route: `/quote-version/document/`,
        params: {
            quoteVersionId: 'quoteVersionId',
        },
    };

    public static document: any = {
        route: `/document/`,
    };

    public static exportPolicy: any = {
        route: `/export/policy`,
        params: {
            format: 'format',
        },
    };

    public static invitationValidateActivation: any = {
        route: `/invitation/validate-activation`,
        params: {
            userId: 'userId',
            invitationId: 'invitationId',
        },
    };

    public static invitationSetPassword: any = {
        route: `/invitation/set-password`,
        params: {
            userId: 'userId',
            invitationId: 'invitationId',
            password: 'password',
        },
    };

    public static invitationValidatePasswordReset: any = {
        route: `/invitation/validate-reset-password`,
        params: {
            invitationId: 'invitationId',
            userId: 'userId',
        },
    };

    public static invitationRequestPassword: any = {
        route: `/invitation/request-reset-password`,
        params: {
            email: 'email',
        },
    };

    public static invitationResetPassword: any = {
        route: `/invitation/reset-password`,
        params: {
            userId: 'userId',
            invitationId: 'invitationId',
            password: 'password',
        },
    };

    public static policy: any = {
        route: `/policy`,
    };

    public static policyTransaction: any = {
        route: `/policy/transaction`,
        params: {
            quoteId: 'quoteId',
        },
    };

    public static policiesForRenewal: any = {
        route: `/policy/policiesForRenewal`,
    };

    public static policyAdjustment: any = {
        route: `/policy/policyAdjustment`,
        params: {
            policyId: 'policyId',
        },
    };

    public static policyRenewal: any = {
        route: `/policy/policyRenewal`,
        params: {
            policyId: 'policyId',
        },
    };

    public static policyCancellation: any = {
        route: `/policy/cancel`,
        params: {
            quoteId: 'quoteId',
        },
    };

    public static policyDetails: any = {
        route: `/details/base`,
        params: {
            policyId: 'id',
            quoteId: 'quoteId',
        },
    };

    public static policyClaims: any = {
        route: `/details/claims`,
    };

    public static policyQuestions: any = {
        route: `/details/questions`,
    };

    public static policyPremiums: any = {
        route: `/details/premium`,
    };

    public static policyDocuments: any = {
        route: `/details/documents`,
    };

    public static policyHistory: any = {
        route: `/history`,
    };

    public static portal: any = {
        route: `/portal`,
    };

    public static portalByTenant: any = {
        route: `/portal/byTenant`,
        params: {
            tenantId: 'tenantId',
        },
    };

    public static portalActiveFeatures: any = {
        route: `/portal/active-features`,
    };

    public static portalDetails: any = {
        route: `/portal/details`,
        params: {
            portalId: 'id',
        },
    };

    public static portalSetting: any = {
        route: `/portal/portalsetting`,
        params: {
            portalId: 'portalId',
        },
    };

    public static portalUpdateUrl: any = {
        route: `/portal/deployment`,
        params: {
            portalId: 'portalId',
        },
    };

    public static product: any = {
        route: `/product`,
    };

    public static productByTenant: any = {
        route: `/product/byTenant`,
        params: {
            tenantId: 'tenantId',
        },
    };

    public static productDetails: any = {
        route: `/product`,
        params: {
            tenantId: 'tenantId',
            productId: 'productId',
        },
    };

    public static productDevRelease: any = {
        route: `/product/devRelease`,
        params: {
            tenantId: 'tenantId',
            productId: 'productId',
        },
    };

    public static productSourceFiles: any = {
        route: `/product/source`,
        params: {
            productId: 'productId',
            tenantId: 'tenantId',
        },
    };

    public static quote: any = {
        route: `/quote`,
    };

    public static quoteCreate: any = {
        route: `/quote`,
        params: {
            productId: 'productId',
            type: 'type',
            customerId: 'customerId',
            isTestData: 'isTestData',
        },
    };

    public static quoteDetails: any = {
        route: `/quote/details`,
        params: {
            quoteId: 'quoteId',
        },
    };

    public static quoteDiscard: any = {
        route: `/quote/discard`,
        params: {
            quoteId: 'quoteId',
        },
    };

    public static quoteDetailsByQuoteIds: any = {
        route: `/quote/detailsByQuoteIds`,
        params: {
            quoteIds: 'quoteIds',
        },
    };

    public static quoteFormData: any = {
        route: `/quote/formdata`,
        params: {
            quoteId: 'quoteId',
        },
    };

    public static quoteVersion: any = {
        route: `/quote/version`,
        params: {
            quoteId: 'quoteId',
        },
    };

    public static quoteVersionDetail: any = {
        route: `/quote/version/detail`,
        params: {
            quoteVersionId: 'quoteVersionId',
        },
    };

    public static release: any = {
        route: `/releases/`,
        params: {
            releaseId: 'releaseId',
        },
    };

    public static restoreRelease: any = {
        route: `/releases/restore/`,
        params: {
            releaseId: 'releaseId',
        },
    };

    public static releaseForProduct: any = {
        route: `/releases/product`,
        params: {
            productId: 'productId',
            tenantId: 'tenantId',
        },
    };

    public static releaseSourceFiles: any = {
        route: `/releases/source`,
        params: {
            releaseId: 'id',
        },
    };

    public static settingsForPortal: any = {
        route: `/settings/bytenantandportal/`,
        params: {
            tenantId: 'tenant',
            portal: 'portal',
        },
    };

    public static settingsForTenant: any = {
        route: `/settings/byTenant/`,
        params: {
            tenantId: 'id',
        },
    };

    public static systemAlert: any = {
        route: `/system-alerts`,
    };

    public static systemAlertByTenant: any = {
        route: `/system-alerts/tenants`,
    };

    public static systemAlertByProduct: any = {
        route: `/system-alerts/products`,
    };

    public static settingUpdate: any = {
        route: `/settings/`,
        params: {
            settingId: 'id',
        },
    };

    public static tenants: any = {
        route: `/tenant`,
    };

    public static tenant: any = {
        route: `/tenant`,
        params: {
            tenantId: 'id',
        },
    };

    public static tenantUpdate: any = {
        route: `/tenant/`,
        params: {
            tenantId: 'id',
        },
    };

    public static tenantSession: any = {
        route: `/tenant/session`,
        params: {
            tenantId: 'id',
        },
    };

    public static tenantForLoggedInUser: any = {
        route: `/tenant/forLoggedInUser`,
    };

    public static tenantName: any = {
        route: `/tenant/tenantName`,
        params: {
            tenantId: 'tenantId',
        },
    };

    public static user: any = {
        route: `/user`,
        params: {
            userId: 'userId',
            status: 'status',
        },
    };

    public static userDetails: any = {
        route: `/user/details`,
        params: {
            userId: 'userId',
        },
    };

    public static emailTemplate: any = {
        route: '/email-templates',
        params: {
            customerId: 'customerId',
        },
    };

    public static email: any = {
        route: "/email",
        params: {
            status: "status",
        },
    };

    public static emailByQuoteId: any = {
        route: "/email/quotes",
        params: {
            status: "status",
        },
    };

    public static emailByUserId: any = {
        route: "/email/users",
        params: {
            status: "status",
        },
    };

    public static emailByCustomerId: any = {
        route: "/email/customers",
        params: {
            status: "status",
        },
    };

    public static emailDetails: any = {
        route: `/email/details`,
        params: {
            emailId: 'id',
        },
    };

    public static displayableFields: any = {
        route: `/quote-configurations/displayable-fields`,
    };

    public static report: any = {
        route: `/report`,
    };

    public static reportGenerate: any = {
        route: `/report/generate`,
    };

    public static reportFilename: any = {
        route: `/report/filename`,
    };

    public static reportFileDownload: any = {
        route: `/report/downloadReportFile`,
        params: {
            reportId: 'reportId',
            reportFileId: 'reportFileId',
            tenantId: 'tenantId',
        },
    };

    public static reportByTenantId: any = {
        route: `/report/byTenantId`,
        params: {
            tenantId: 'tenantId',
        },
    };

    public static reportById: any = {
        route: `/report/byReportId`,
        params: {
            reportId: 'reportId',
            tenantId: 'tenantId',
        },
    };

    public static role: any = {
        route: `/role/roles`,
    };

    public static roleById: any = {
        route: `/role/roles/`,
        params: {
            roleId: 'roleId',
        },
    };

    public static rolePermissionsByRoleType: any = {
        route: `/role/permissions/`,
        params: {
            roleType: 'roleType',
        },
    };

    public static toHttpParams(params: Map<string,
        string | Array<string>>): { [param: string]: string | Array<string> } {
        const obj: any = {};
        params.forEach((value: any, key: string) => (obj[key] = value));
        return obj;
    }

    public static toHttpOptions(params: Map<string,
        string | Array<string>>): { params: { [param: string]: string | Array<string> } } {
        if (!params) {
            return { params: {} };
        } else {
            return { params: ApiHelper.toHttpParams(params) };
        }
    }

    public static generateRequestOptions(searchParams?: any, additionalFilters?: any): HttpParams {
        let params: HttpParams = new HttpParams();
        if (searchParams) {
            if (!Array.isArray(searchParams)) {
                params = params.append(searchParams.label, searchParams.value);
            } else {
                for (const searchParam of searchParams) {
                    params = params.append(searchParam.label, searchParam.value);
                }
            }
        }
        if (additionalFilters) {
            for (const filter of additionalFilters) {
                params = params.append(filter.label, filter.value);
            }
        }
        return params;
    }
}
