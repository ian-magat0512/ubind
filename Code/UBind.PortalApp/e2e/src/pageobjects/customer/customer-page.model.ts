import { by, element } from "protractor";

export class CustomerPageModel {
    public constructor() {
    }
    
    public ListPersonNewIonList = element(by.id('list-person-new-ion-list'));
    public ListPersonIonSegment = element(by.id('list-person-ion-segment'));
    public ListPersonNewIonListId = 'list-person-new-ion-list';
    public ListPersonIonSegmentId = 'list-person-ion-segment';
}