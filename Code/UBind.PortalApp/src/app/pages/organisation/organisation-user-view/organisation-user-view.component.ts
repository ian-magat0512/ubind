import { Component, Input, Output, EventEmitter, ViewChild, OnChanges, SimpleChanges } from '@angular/core';
import { DefaultImgHelper } from '@app/helpers/default-img-helper';
import { IconLibrary } from '@app/models/icon-library.enum';
import { UserViewModel } from '@app/viewmodels/user.viewmodel';
import { contentAnimation } from '@assets/animations';
import { UserApiService } from "@app/services/api/user-api.service";
import { Observable } from "rxjs";
import { UserResourceModel } from "@app/resource-models/user/user.resource-model";
import { MapHelper } from '@app/helpers/map.helper';
import {
    EntityDetailSegmentListComponent,
} from '@app/components/entity-detail-segment-list/entity-detail-segment-list.component';

/**
 * Export Organisation User view component class
 * TODO: Write a better class header: user view in organisation.
 */
@Component({
    selector: 'app-organisation-user-view',
    templateUrl: 'organisation-user-view.component.html',
    animations: [contentAnimation],
})

export class OrganisationUserViewComponent implements OnChanges {

    @ViewChild('usersList') private usersList: EntityDetailSegmentListComponent;
    @Input() public apiService: UserApiService;
    @Input() public userParams: Map<string, string | Array<string>>;
    @Output() public clickedUserActionButton: EventEmitter<UserViewModel> = new EventEmitter<UserViewModel>();
    @Output() public raiseUserLoadedEvent: EventEmitter<boolean | null> = new EventEmitter<boolean | null>();
    public defaultUserImgPath: string = 'assets/imgs/default-user.svg';
    public defaultUserImgFilter: string
        = 'invert(52%) sepia(0%) saturate(0%) hue-rotate(153deg) brightness(88%) contrast(90%)';
    public iconLibrary: typeof IconLibrary = IconLibrary;
    public userTypeViewModel: typeof UserViewModel = UserViewModel;

    public constructor() { }

    public ngOnChanges(changes: SimpleChanges): void {
        if (changes.userParams && this.usersList) {
            this.usersList.reload();
        }
    }

    public userDidSelectItem(userViewModel: UserViewModel): void {
        this.clickedUserActionButton.emit(userViewModel);
    }

    public usersLoaded(hasUsers: boolean | undefined): void {
        this.raiseUserLoadedEvent.emit(hasUsers);
    }

    public getSegmentUserList(
        params?: Map<string, string | Array<string>>,
    ): Observable<Array<UserResourceModel>> {
        params = MapHelper.merge(params, this.userParams);
        return this.apiService.getList(params);
    }

    public setDefaultImg(event: any): void {
        DefaultImgHelper.setImageSrcAndFilter(event.target, this.defaultUserImgPath, this.defaultUserImgFilter);
    }
}
