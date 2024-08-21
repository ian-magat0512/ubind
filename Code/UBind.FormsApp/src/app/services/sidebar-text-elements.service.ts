import { Injectable } from "@angular/core";
import { QuoteType } from "@app/models/quote-type.enum";
import { ConfigService } from "./config.service";

/**
 * Export sidebar text elements service class.
 * TODO: Write a better class header: sidebar text elements functions.
 */
@Injectable({
    providedIn: 'root',
})
export class SidebarTextElementsService {

    public constructor(
        private configService: ConfigService,
    ) { }

    /**
     * attempts to get the element text with the given property name from the given sidebar text elements, 
     * and if not found tries to get it from the default sidebar text elements.
     * If still not found, returns null.
     */
    public getSidebarTextElementForQuoteType(quoteType: QuoteType, propertyName: string): any {
        let textElements: any = this.getSidebarTextElementSetForQuoteType(quoteType);
        let defaultSidebarTextElements: any = this.configService.textElements?.sidebar;
        if (textElements && textElements[propertyName] && textElements[propertyName].text) {
            return textElements[propertyName].text;
        } else if (defaultSidebarTextElements &&
            defaultSidebarTextElements[propertyName] &&
            defaultSidebarTextElements[propertyName].text) {
            return defaultSidebarTextElements[propertyName].text;
        }
        return null;
    }

    /**
     * attempts to get the element text with the given property name from the given sidebar text elements, 
     * and if not found tries to get it from the default sidebar text elements.
     * If still not found, returns null.
     */
    public getSidebarTextElementForClaim(propertyName: string): any {
        // TODO: Not implemented.
        return null;
    }

    private getSidebarTextElementSetForQuoteType(quoteType: QuoteType): any {
        let textElements: any = this.configService.textElements;
        switch (quoteType) {
            case QuoteType.NewBusiness:
                return textElements.sidebar?.sidebarPurchase ?? textElements.sidebarPurchase;
            case QuoteType.Adjustment:
                return textElements.sidebar?.sidebarAdjustment ?? textElements.sidebarAdjustment;
            case QuoteType.Cancellation:
                return textElements.sidebar?.sidebarCancellation ?? textElements.sidebarCancellation;
            case QuoteType.Renewal:
                return textElements.sidebar?.sidebarRenewal ?? textElements.sidebarRenewal;
            default:
                break;
        }

        throw new Error(`When attempting to get the sidebar text elements for a given quote type, ` +
            `we came across the quote type ${quoteType} which was unknown or not expected here.`);
    }
}
