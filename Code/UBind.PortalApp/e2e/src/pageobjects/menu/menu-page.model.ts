import { by, element } from "protractor";

export class MenuModel {
    public constructor() {
    }
    public GenericMenuButtonElAll = element.all(by.tagName("ion-buttons"));
    public MenuButtonElAll = element.all(by.css("ion-buttons[id='menu-btn']"));
}