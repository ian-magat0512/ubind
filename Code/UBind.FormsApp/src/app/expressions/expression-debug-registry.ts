import { Injectable } from "@angular/core";
import { EventService } from "@app/services/event.service";
import { Expression } from "./expression";

/**
 * This class acts a central registry for all expressions. It is used by the ExpressionsDebugComponent
 * to render the result of all expressions for debugging purposes.
 */
@Injectable({
    providedIn: 'root',
})
export class ExpressionDebugRegistry {

    public expressions: Set<Expression> = new Set<Expression>();

    public constructor(eventService: EventService) {
        eventService.expressionCreatedSubject.subscribe((expression: Expression) => this.register(expression));
        eventService.expressionDisposedSubject.subscribe((expression: Expression) => this.deregister(expression));
    }

    public register(expression: Expression): void {
        if (!expression.isConstant() || expression.arguments.size) {
            this.expressions.add(expression);
        }
    }

    public deregister(expression: Expression): void {
        if (!expression.isConstant() || expression.arguments.size) {
            this.expressions.delete(expression);
        }
    }
}
