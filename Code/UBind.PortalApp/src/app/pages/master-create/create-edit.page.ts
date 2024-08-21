import { Directive, ElementRef, Injector, OnInit } from "@angular/core";
import { IonicLifecycleEventReplayBus } from "@app/services/ionic-lifecycle-event-replay-bus";
import { PageWithMaster } from "../master-detail/page-with-master";
import { EventService } from "@app/services/event.service";
import { DetailsListFormItem } from "@app/models/details-list/details-list-form-item";
import { FormGroup } from "@angular/forms";
import { FormHelper } from "@app/helpers/form.helper";
import { EntityEditFieldOption } from "@app/models/entity-edit-field-option";
import { Subject } from "rxjs";
import { titleCase } from 'title-case';

/**
 * A base class/component for create/edit functionality.
 */
@Directive({ selector: '[appCreateEdit]' })
export abstract class CreateEditPage<TModel> implements OnInit, PageWithMaster {

    // defines what the name of the entity is.
    public abstract subjectName: string;
    public abstract isEdit: boolean;

    // title of the create/edit page.
    public title: string = '';
    public form: FormGroup;
    public model: TModel;
    public isLoading: boolean;
    public errorMessage: string = '';
    public detailList: Array<DetailsListFormItem>;
    public fieldOptions: Array<EntityEditFieldOption> = [];
    public ionicLifecycleEventReplayBus: IonicLifecycleEventReplayBus;
    protected destroyed: Subject<void>;

    public constructor(
        eventService: EventService,
        elementRef: ElementRef,
        public injector: Injector,
        public formHelper: FormHelper,
    ) {
        this.ionicLifecycleEventReplayBus = new IonicLifecycleEventReplayBus(elementRef);
        eventService.detailComponentCreated(this);
    }

    public ngOnInit(): void {
        this.setTitle();
    }

    // closes the create-edit page.
    public async close(): Promise<void> {
        if (this.form.dirty) {
            if (!await this.formHelper.confirmExitWithUnsavedChanges()) {
                return;
            }
        }
        this.returnToPrevious();
    }

    public async save(value: any): Promise<void> {
        if (this.form.invalid) {
            return;
        }

        if (this.isEdit) {
            this.update(value);
        } else {
            this.create(value);
        }
    }

    public setTitle(): void {
        let prefix: string = this.isEdit ? "Edit" : "Create";
        this.title = titleCase(prefix + " " + this.subjectName);
    }

    public abstract update(value: any): void;
    public abstract create(value: any): void;
    public abstract returnToPrevious(): void;
    public abstract load(): void;
    protected abstract buildForm(): FormGroup;
}
