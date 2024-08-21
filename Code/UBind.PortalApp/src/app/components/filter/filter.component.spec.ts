import { ComponentFixture, TestBed } from "@angular/core/testing";
import { NavParams, ModalController } from "@ionic/angular";
import { FormBuilder, ReactiveFormsModule } from "@angular/forms";
import { CUSTOM_ELEMENTS_SCHEMA } from "@angular/core";
import { FilterComponent } from "./filter.component";
import { QueryRequestHelper } from "@app/helpers";
import { Checkbox } from "@app/viewmodels/checkbox.viewmodel";
import { SharedModule } from "@app/shared.module";
import { RouterModule } from "@angular/router";
import { RouterTestingModule } from "@angular/router/testing";

describe("FilterComponent", () => {
    let component: FilterComponent;
    let fixture: ComponentFixture<FilterComponent>;
    let statusList: Array<string> = [
        "Incomplete",
        "Review",
        "Endorsement",
        "Approved",
        "Declined",
        "Complete"];
    let navParamsStub: any = {
        get: (param: string): Array<any> => {
            return QueryRequestHelper.constructStringFilters(statusList, []);
        },
    };

    beforeEach(() => {
        TestBed.configureTestingModule({
            declarations: [
                FilterComponent,
            ],
            providers: [
                { provide: NavParams, useValue: navParamsStub },
                { provide: FormBuilder, useClass: FormBuilder },
                { provide: ModalController, useValue: {} },
            ],
            schemas: [
                CUSTOM_ELEMENTS_SCHEMA,
            ],
            imports: [
                ReactiveFormsModule,
                SharedModule,
                RouterModule.forRoot([]),
                RouterTestingModule,
            ],
        }).compileComponents();
    });

    beforeEach(async () => {
        fixture = TestBed.createComponent(FilterComponent);
        component = fixture.debugElement.componentInstance;
        fixture.detectChanges();
        await fixture.whenStable();
    });

    it("should create the component", async () => {
        fixture.detectChanges();
        const app: any = fixture.debugElement.componentInstance;
        expect(app).toBeTruthy();
    });

    it("status should contain the statusList", () => {
        fixture.detectChanges();
        expect(component.statuses.map((s: Checkbox) => s.name)).toEqual(jasmine.arrayContaining(statusList));
    });

});
