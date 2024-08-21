import { NgModule } from "@angular/core";
import { SharedComponentsModule } from "@app/components/shared-components.module";
import { ListModule } from "@app/list.module";
import { SharedModule } from "@app/shared.module";
import { CreateDataTablePage } from "./create-data-table/create-data-table.page";
import {
    DataTableDefinitionListDetailPage,
} from "./data-table-definition-list-detail/data-table-definition-list-detail.page";
import { DetailDataTableDefinitionPage } from "./detail-data-table-definition/detail-data-table-definition.page";
import { EditDataTablePage } from "./edit-data-table/edit-data-table.page";
import { ListDataTableDefinitionPage } from "./list-data-table-definition/list-data-table-definition.page";

/**
 * Module to group data table components that will 
 * be shared to other module.
 */
@NgModule({
    declarations: [
        DataTableDefinitionListDetailPage,
        CreateDataTablePage,
        DetailDataTableDefinitionPage,
        EditDataTablePage,
        ListDataTableDefinitionPage,
    ],
    imports: [
        SharedModule,
        SharedComponentsModule,
        ListModule,
    ],
    exports: [
        DataTableDefinitionListDetailPage,
        CreateDataTablePage,
        DetailDataTableDefinitionPage,
        EditDataTablePage,
        ListDataTableDefinitionPage,
    ],
})
export class DataTableSharedComponentModule { }
