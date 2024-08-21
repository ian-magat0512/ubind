import { NgModule } from "@angular/core";
import { SharedModule } from "./shared.module";
import { SharedComponentsModule } from "./components/shared-components.module";
import { SegmentedEntityListComponent } from "./components/entity-list/segmented-entity-list.component";
import { EntityListComponent } from "./components/entity-list/entity-list.component";
import { ListNoSelectionPage } from "./pages/list-no-selection/list-no-selection.page";

/**
 * A module which provides all of the entity list and related components.
 * This can be imported by other modules which list entities. 
 */
@NgModule({
    declarations: [
        SegmentedEntityListComponent,
        EntityListComponent,
        ListNoSelectionPage,
    ],
    imports: [
        SharedModule,
        SharedComponentsModule,
    ],
    exports: [
        SegmentedEntityListComponent,
        EntityListComponent,
        ListNoSelectionPage,
    ],
})

export class ListModule { }
