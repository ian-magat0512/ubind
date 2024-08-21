/**
 * This class is needed because the static functions contained here are reusable on all pages that use the forms-app
 */
export class WebFormHelper {
    public static viewType: any = {
        Quote: 'Quote',
        Policy: 'Policy',
        Claims: 'Claims',
        ClaimsUpdate: 'ClaimsUpdate',
        ClaimsReview: 'ClaimsReview',
        ClaimsAssess: 'ClaimsAssess',
        ClaimsSettle: 'ClaimsSettle',
        ClaimsNotification: 'ClaimsNotification',
        ClaimsVersionUpdate: 'ClaimsVersionUpdate',
    };

    public static modes: any = {
        Create: 'create',
        Edit: 'edit',
        Review: 'review',
        Assess: 'assess',
        Acknowledge: 'acknowledge',
        Settle: 'settle',
    };

    public getViewTypeLoadMessage(viewType: string): string {
        let viewTypeLoadMessage: string = 'Please wait...';
        switch (viewType) {
            case WebFormHelper.viewType.Quote:
                viewTypeLoadMessage = "Loading quote form...";
                break;
            case WebFormHelper.viewType.Policy:
                viewTypeLoadMessage = "Loading policy form...";
                break;
            case WebFormHelper.viewType.Claims:
                viewTypeLoadMessage = "Create claim";
                break;
            case WebFormHelper.viewType.ClaimsUpdate:
                viewTypeLoadMessage = "Update claim";
                break;
            case WebFormHelper.viewType.ClaimsReview:
                viewTypeLoadMessage = "Review claim";
                break;
            case WebFormHelper.viewType.ClaimsAssess:
                viewTypeLoadMessage = "Assess claim";
                break;
            case WebFormHelper.viewType.ClaimsSettle:
                viewTypeLoadMessage = "Settle claim";
                break;
            case WebFormHelper.viewType.ClaimsNotification:
                viewTypeLoadMessage = "Acknowledge claim";
                break;
            case WebFormHelper.viewType.ClaimsVersionUpdate:
                viewTypeLoadMessage = "Update claims version";
                break;
            default:
                break;
        }

        return viewTypeLoadMessage;
    }
}
