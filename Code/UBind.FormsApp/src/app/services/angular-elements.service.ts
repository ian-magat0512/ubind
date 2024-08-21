import { Injectable, Injector } from "@angular/core";
import { createCustomElement } from "@angular/elements";
import { ActionWidget } from "@app/components/widgets/action/action.widget";
import { PriceWidget } from "@app/components/widgets/price/price.widget";
import { TooltipWidget } from "@app/components/widgets/tooltip/tooltip.widget";

/**
 * Service for managing angular custom elements.
 * Angular custom elements allows us to use angular components as
 * custom HTML elements (e.g. similar to WebComponents)
 */
@Injectable({
    providedIn: 'root',
})
export class AngularElementsService {

    public constructor(
        private injector: Injector,
    ) {}

    /**
     * If there are certain angular components we want to be usable when we write in html into the DOM
     * dynamically, these need to be registered as AngularElements so they become Web Components.
     * We use this for action buttons which we might want to use in a header or footer, and generate using
     * an expression method.
     */
    public registerAngularComponentsAsAngularElements(): void {
        const actionWidgetElement: any = createCustomElement(ActionWidget, { injector: this.injector });
        if (!customElements.get('ubind-action-widget')) {
            customElements.define('ubind-action-widget', actionWidgetElement);
        }
        const priceWidgetElement: any = createCustomElement(PriceWidget, { injector: this.injector });
        if (!customElements.get('ubind-price-widget')) {
            customElements.define('ubind-price-widget', priceWidgetElement);
        }
        const tooltipWidgetElement: any = createCustomElement(TooltipWidget, { injector: this.injector });
        if (!customElements.get('ubind-tooltip-widget')) {
            customElements.define('ubind-tooltip-widget', tooltipWidgetElement);
        }
    }
}
