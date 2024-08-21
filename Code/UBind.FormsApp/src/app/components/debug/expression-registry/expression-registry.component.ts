import { Component } from "@angular/core";
import { Expression } from "@app/expressions/expression";
import { ExpressionDebugRegistry } from "@app/expressions/expression-debug-registry";

/**
 * Export expression registry component class.
 * This class manage expression register.
 */
@Component({
    selector: 'expression-registry',
    templateUrl: './expression-registry.component.html',
})
export class ExpressionRegistryComponent {

    public expressions: Set<Expression>;

    public constructor(expressionRegistryService: ExpressionDebugRegistry) {
        this.expressions = expressionRegistryService.expressions;
    }

}
