import { Injectable } from "@angular/core";
import { UserTypePathHelper } from "@app/helpers/user-type-path.helper";
import { QuoteApiService } from "./api/quote-api.service";
import { AuthenticationService } from "./authentication.service";
import { LoginRedirectService } from "./login-redirect.service";
import { NavProxyService } from "./nav-proxy.service";
import { SharedAlertService } from "./shared-alert.service";
import { HttpErrorResponse } from "@angular/common/http";
import { ProblemDetails } from "@app/models/problem-details";

/**
 * This class manage customer services functions.
 */
@Injectable({ providedIn: 'root' })
export class CustomerService {
    public constructor(
        private sharedAlertService: SharedAlertService,
        private quoteApiService: QuoteApiService,
        private navProxy: NavProxyService,
        private userPath: UserTypePathHelper,
        private authService: AuthenticationService,
        private loginRedirectService: LoginRedirectService,
    ) {
    }

    public async associateQuoteWithCustomer(associationInvitationId: string, quoteId: string): Promise<void> {
        this.quoteApiService.verifyCustomerAssociationInvitation(associationInvitationId, quoteId)
            .subscribe(() => {
                this.quoteApiService.getQuoteNumber(quoteId).subscribe(
                    async (quoteNumberResponse: string) => {
                        if (await this.confirmQuoteAssociation(quoteNumberResponse)) {
                            this.executeQuoteAssociation(associationInvitationId, quoteId, quoteNumberResponse);
                        }
                    },
                );
            }, async (err: HttpErrorResponse) => {
                if (ProblemDetails.isProblemDetailsResponse(err)) {
                    let appError: ProblemDetails = ProblemDetails.fromJson(err.error);
                    if (appError.Code == 'quote.association.user.not.found') {
                        if (await this.confirmSwitchUserAccount()) {
                            this.authService.logout();

                            this.loginRedirectService.pathSegments = [
                                'my-quotes',
                                quoteId,
                                'associate',
                                associationInvitationId,
                            ];
                            this.loginRedirectService.queryParams = null;
                            this.loginRedirectService.redirect();
                        }
                    } else {
                        throw err;
                    }
                } else {
                    throw err;
                }
            });
    }

    private executeQuoteAssociation(invitationId: string, quoteId: string, quoteNumber: string): void {
        this.quoteApiService.associateWithCustomer(invitationId, quoteId)
            .subscribe(() => {
                this.sharedAlertService.showToast(`Quote ${quoteNumber} was associated with your account`);
                this.navProxy.navigate([this.userPath.quote, quoteId]);
            });
    }

    private async confirmQuoteAssociation(quoteNumber: string): Promise<boolean> {
        return new Promise((resolve: any): any => {
            this.sharedAlertService.showWithActionHandler({
                header: 'Confirm quote association',
                subHeader: `Please confirm that you would like to associate quote ${quoteNumber} with your account.`,
                buttons: [
                    {
                        text: 'No',
                        handler: (): any => {
                            resolve(false);
                        },
                    },
                    {
                        text: 'Yes',
                        handler: (): any => {
                            resolve(true);
                        },
                    },
                ],
            });
        });
    }

    private async confirmSwitchUserAccount(): Promise<boolean> {
        return new Promise((resolve: any): any => {
            this.sharedAlertService.showWithActionHandler({
                header: 'Switch user accounts',
                subHeader: 'The quote you have attempted to associate with your '
                    + 'account belongs to a different user account. To proceed with the association, '
                    + 'you must first switch to the right account.',
                buttons: [
                    {
                        text: 'No',
                        handler: (): any => {
                            resolve(false);
                        },
                    },
                    {
                        text: 'Yes',
                        handler: (): any => {
                            resolve(true);
                        },
                    },
                ],
            });
        });
    }
}
