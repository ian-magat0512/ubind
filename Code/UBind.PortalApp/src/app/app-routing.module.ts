import { NgModule } from "@angular/core";
import { RouterModule } from "@angular/router";
import { routes } from "@app/routing/route-definitions";

/**
 * Provides the routes which angular uses to know which modules to load when certain urls match.
 */
@NgModule({
    imports: [RouterModule.forRoot(routes, { onSameUrlNavigation: 'reload', enableTracing: false })],
    exports: [RouterModule],
})
export class AppRoutingModule { }
