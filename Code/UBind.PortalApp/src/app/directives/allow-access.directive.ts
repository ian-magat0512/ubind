import { Directive, TemplateRef, ViewContainerRef, Input, OnInit, OnDestroy } from "@angular/core";
import { Permission, PermissionDataModel } from "@app/helpers/permissions.helper";
import { EventService, UserId } from "@app/services/event.service";
import { Subject } from "rxjs";
import { takeUntil } from "rxjs/operators";
import { PermissionService } from "@app/services/permission.service";

/**
 * Allows the checking of permissions within a html template, such that if the permission check fails,
 * the html element is not rendered.
 */
@Directive({
    selector: "[ubAllowAccess]",
})
export class AllowAccessDirective implements OnInit, OnDestroy {

    private destroyed: Subject<void>;
    private permission: Permission | Array<Permission>;
    private checkPermissionRules: boolean = false;
    private permissionModel: PermissionDataModel;
    private operation: string;

    @Input()
    public set ubAllowAccess(permission: Permission | Array<Permission>) {
        this.permission = permission;
    }

    @Input()
    public set ubAllowAccessOperation(operation: string) {
        this.operation = operation;
    }

    @Input()
    public set ubAllowAccessPermissionModel(permissionModel: PermissionDataModel) {
        this.permissionModel = permissionModel;
        this.checkPermissionRules = true;

        // this is called multiple times even if the model isnt set yet,
        // by the time its set with value, retrigger permission check.
        if (this.permissionModel
            && Object.keys(this.permissionModel).length > 0) {
            this.checkPermission();
        }
    }

    public constructor(
        private templateRef: TemplateRef<any>,
        private viewContainer: ViewContainerRef,
        private permissionService: PermissionService,
        private eventService: EventService,
    ) {
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.eventService.userLoginSubject$.pipe(takeUntil(this.destroyed)).subscribe((userId: UserId) => {
            this.checkPermission();
        });
        this.checkPermission();
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    private checkPermission(): void {
        if ((this.permission instanceof Array)) {
            // default value OR if empty.
            if (!this.operation) {
                this.operation = "OR";
            }
            if (this.operation) {
                switch (this.operation.toUpperCase()) {
                    case "OR":
                        this.createOrRemoveContainer((this.permission as Array<Permission>)
                            .some((p: Permission) => this.isAllowed(p)));
                        break;
                    case "AND":
                        this.createOrRemoveContainer((this.permission as Array<Permission>)
                            .every((p: Permission) => this.isAllowed(p)));
                        break;
                    default:
                        break;
                }
            }
        } else {
            let isAllowed: boolean = this.isAllowed(this.permission);
            this.createOrRemoveContainer(isAllowed);
        }
    }

    private createOrRemoveContainer(hasPermission: boolean): void {
        this.viewContainer.clear();
        if (hasPermission) {
            this.viewContainer.createEmbeddedView(this.templateRef);
        }
    }

    private isAllowed(permission: Permission): boolean {
        let isAllowed: boolean = false;
        if (this.checkPermissionRules) {
            if (this.permissionModel && Object.keys(this.permissionModel).length > 0) {
                isAllowed = this.permissionService.hasElevatedPermissionsViaModel(
                    permission,
                    this.permissionModel);
            } else {
                // do nothing.
            }
        } else {
            isAllowed = this.permissionService && this.permissionService.hasPermission(permission);
        }
        return isAllowed;
    }
}
