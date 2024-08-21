import { Injectable } from "@angular/core";
import { Observable, Subject, SubscriptionLike, BehaviorSubject, combineLatest } from "rxjs";
import { EnvironmentChange } from "@app/models/environment-change";
import { PageWithMaster } from "@pages/master-detail/page-with-master";
import { QuoteStateChangedModel } from "@app/models/quote-state-changed.model";
import { ClaimStateChangedModel } from "@app/models/claim-state-changed.model";
import { QuoteStepChangedModel } from "@app/models/quote-step-changed.model";
import { ProductFilter } from "@app/models/product-filter";
import { WindowSize } from "@app/models/window-size.enum";
import { map } from "rxjs/operators";
import { OrganisationModel } from "@app/models/organisation.model";
import { MenuState } from "@app/models/menu-state.enum";

export type PortalId = string;
export type UserId = string;
type SubjectTypeName = string;
type EntityTypeName = string;

/**
 * Export Event service class.
 * TODO: Write a better class header: event functions.
 */
@Injectable({ providedIn: 'root' })
export class EventService {

    private subjects: Map<SubjectTypeName, Map<EntityTypeName, SubscriptionLike>>
        = new Map<SubjectTypeName, Map<EntityTypeName, SubscriptionLike>>();

    // Route change
    private routeChangedSubject: Subject<string> = new Subject();
    public routeChangedSubject$: Observable<string> = this.routeChangedSubject.asObservable();

    // Organisation state changed
    private organisationStateChangedSubject: Subject<any> = new Subject();
    public organisationStateChangedSubject$: Observable<any> = this.organisationStateChangedSubject.asObservable();

    // Detail component created
    private detailComponentCreatedSubject: Subject<PageWithMaster> = new Subject<PageWithMaster>();
    public detailComponentCreatedSubject$: Observable<PageWithMaster> =
        this.detailComponentCreatedSubject.asObservable();

    // Detail view data change.
    private detailViewDataChangedSubject: Subject<void> = new Subject<void>();
    public detailViewDataChangedSubject$: Observable<void> =
        this.detailViewDataChangedSubject.asObservable();

    // User login/logout
    private userLoginSubject: Subject<UserId> = new Subject<UserId>();
    public userLoginSubject$: Observable<UserId> = this.userLoginSubject.asObservable();
    private userLogoutSubject: Subject<void> = new Subject<void>();
    public userLogoutSubject$: Observable<void> = this.userLogoutSubject.asObservable();
    private userAuthenticatedSubject: Subject<boolean> = new BehaviorSubject<boolean>(false);
    public userAuthenticatedSubject$: Observable<boolean> = this.userAuthenticatedSubject.asObservable();
    private performingUserOrganisationSubject: Subject<OrganisationModel>
        = new BehaviorSubject<OrganisationModel>(null);
    public performingUserOrganisationSubject$: Observable<OrganisationModel>
        = this.performingUserOrganisationSubject.asObservable();

    // User Password Expired
    private userPasswordExpiredSubject: Subject<void> = new Subject<void>();
    public userPasswordExpiredSubject$: Observable<void> = this.userPasswordExpiredSubject.asObservable();

    // User picture changed
    private userPictureChangedSubject: Subject<UserId> = new Subject<UserId>();
    public userPictureChangedSubject$: Observable<UserId> = this.userPictureChangedSubject.asObservable();

    // Quote state changed
    private quoteStateChangedSubject: Subject<QuoteStateChangedModel> = new Subject<QuoteStateChangedModel>();
    public quoteStateChangedSubject$: Observable<QuoteStateChangedModel> = this.quoteStateChangedSubject.asObservable();

    // Quote state changed
    private quoteStepChangedSubject: Subject<QuoteStepChangedModel> = new Subject<QuoteStepChangedModel>();
    public quoteStepChangedSubject$: Observable<QuoteStepChangedModel> = this.quoteStepChangedSubject.asObservable();

    // Claim state changed
    private claimStateChangedSubject: Subject<ClaimStateChangedModel> = new Subject<ClaimStateChangedModel>();
    public claimStateChangedSubject$: Observable<ClaimStateChangedModel> = this.claimStateChangedSubject.asObservable();

    // Environment changed
    private environmentChangedSubject: Subject<EnvironmentChange> = new Subject<EnvironmentChange>();
    public environmentChangedSubject$: Observable<EnvironmentChange> = this.environmentChangedSubject.asObservable();

    // Filter Status List changed
    private filterStatusListChangedSubject: Subject<boolean> = new Subject<boolean>();
    public filterStatusListChangedSubject$: Observable<boolean> = this.filterStatusListChangedSubject.asObservable();

    // Customer updated
    private customerUpdatedSubject: Subject<void> = new Subject();
    public customerUpdatedSubject$: Observable<void> = this.customerUpdatedSubject.asObservable();

    // Product Filter Update
    private productFilterUpdateSubject: Subject<Array<ProductFilter>> = new Subject<Array<ProductFilter>>();
    public productFilterUpdateSubject$: Observable<Array<ProductFilter>> =
        this.productFilterUpdateSubject.asObservable();

    // Feature setting changed
    private featureSettingChangedSubject: Subject<void> = new Subject<void>();
    public featureSettingChangedSubject$: Observable<void> = this.featureSettingChangedSubject.asObservable();

    // Layout
    private windowSizeSubject: Subject<WindowSize> = new BehaviorSubject<WindowSize>(WindowSize.Small);
    public windowSizeSubject$: Observable<WindowSize> = this.windowSizeSubject.asObservable();
    public canShowProfilePictureIcon$: Observable<boolean>
        = combineLatest(this.windowSizeSubject$, this.userAuthenticatedSubject$).pipe(
            map(([windowSize, isAuthenticated]: [WindowSize, boolean]) => isAuthenticated
                && (windowSize == WindowSize.MediumLarge || windowSize == WindowSize.Large)));
    private menuStateSubject: Subject<MenuState> = new BehaviorSubject<MenuState>(MenuState.Zero);
    public menuStateSubject$: Observable<MenuState> = this.menuStateSubject.asObservable();

    // Detail view data change.
    private dashboardProductFilterChangedSubject: Subject<void> = new Subject<void>();
    public dashboardProductFilterChangedSubject$: Observable<void> =
        this.dashboardProductFilterChangedSubject.asObservable();

    public getEntityCreatedSubject<EntityType>(entityTypeName: EntityTypeName): Subject<EntityType> {
        return this.getSubject('entityCreated', entityTypeName);
    }

    public getEntityListHeadersUpdatedBehaviorSubject<EntityType>(
        entityTypeName: EntityTypeName,
    ): BehaviorSubject<EntityType> {
        return this.getBehaviorSubject('entityListHeaders', entityTypeName);
    }

    public getEntityUpdatedSubject<EntityType>(entityTypeName: EntityTypeName): Subject<EntityType> {
        return this.getSubject('entityUpdated', entityTypeName);
    }

    public getEntityDeletedSubject<EntityType>(entityTypeName: EntityTypeName): Subject<EntityType> {
        return this.getSubject('entityDeleted', entityTypeName);
    }

    private getSubject<EntityType>(
        subjectTypeName: SubjectTypeName,
        entityTypeName: EntityTypeName,
    ): Subject<EntityType> {
        let subjectTypeMap: Map<string, SubscriptionLike> = this.getSubjectTypeMap(subjectTypeName);
        let subject: Subject<EntityType> = <Subject<EntityType>>subjectTypeMap.get(entityTypeName);
        if (!subject) {
            subject = new Subject<EntityType>();
            subjectTypeMap.set(entityTypeName, subject);
        }
        return subject;
    }

    private getBehaviorSubject<EntityType>(
        subjectTypeName: SubjectTypeName,
        entityTypeName: EntityTypeName,
    ): BehaviorSubject<EntityType> {
        let subjectTypeMap: Map<string, SubscriptionLike> = this.getSubjectTypeMap(subjectTypeName);
        let subject: BehaviorSubject<EntityType> = <BehaviorSubject<EntityType>>subjectTypeMap.get(entityTypeName);
        if (!subject) {
            subject = new BehaviorSubject<EntityType>(null);
            subjectTypeMap.set(entityTypeName, subject);
        }
        return subject;
    }

    private getSubjectTypeMap(subjectTypeName: SubjectTypeName): Map<EntityTypeName, SubscriptionLike> {
        let subjectTypeMap: Map<string, SubscriptionLike> = this.subjects.get(subjectTypeName);
        if (!subjectTypeMap) {
            subjectTypeMap = new Map<EntityTypeName, SubscriptionLike>();
            this.subjects.set(subjectTypeName, subjectTypeMap);
        }
        return subjectTypeMap;
    }

    public routeChanged(url: string): void {
        this.routeChangedSubject.next(url);
    }

    public userLoggedIn(value?: UserId): void {
        this.userAuthenticatedSubject.next(true);
        this.userLoginSubject.next(value);
    }

    public userLoggedOut(): void {
        this.userAuthenticatedSubject.next(false);
        this.userLogoutSubject.next();
    }

    public performingUserOrganisationChanged(organisation?: OrganisationModel): void {
        this.performingUserOrganisationSubject.next(organisation);
    }

    public userPasswordExpired(): void {
        this.userPasswordExpiredSubject.next();
    }

    public userPictureChanged(value?: UserId): void {
        this.userPictureChangedSubject.next(value);
    }

    public quoteStateChanged(value?: QuoteStateChangedModel): void {
        this.quoteStateChangedSubject.next(value);
    }

    public quoteStepChanged(value?: QuoteStepChangedModel): void {
        this.quoteStepChangedSubject.next(value);
    }

    public customerUpdated(): void {
        this.customerUpdatedSubject.next();
    }

    public claimStateChanged(value?: ClaimStateChangedModel): void {
        this.claimStateChangedSubject.next(value);
    }

    public environmentChanged(value?: EnvironmentChange): void {
        this.environmentChangedSubject.next(value);
    }

    public detailComponentCreated(value?: PageWithMaster): void {
        this.detailComponentCreatedSubject.next(value);
    }

    public organisationStateChanged(): void {
        this.organisationStateChangedSubject.next();
    }

    // Filter Selection changed event for entity types
    public getEntityFilterChangedSubject<EntityType>(entityTypeName: EntityTypeName): Subject<EntityType> {
        return this.getSubject('entityFilterChanged', entityTypeName.toLowerCase());
    }

    public filterStatusListChanged(value?: boolean): void {
        this.filterStatusListChangedSubject.next(value);
    }

    public detailViewDataChanged(): void {
        this.detailViewDataChangedSubject.next();
    }

    public productFilterUpdate(filter: Array<ProductFilter>): void {
        this.productFilterUpdateSubject.next(filter);
    }

    public featureSettingChanged(): void {
        this.featureSettingChangedSubject.next();
    }

    public windowSizeChanged(value?: WindowSize): void {
        this.windowSizeSubject.next(value);
    }

    public menuStateChanged(value: MenuState): void {
        this.menuStateSubject.next(value);
    }

    public dashboardProductFilterChanged(): void {
        this.dashboardProductFilterChangedSubject.next();
    }
}
