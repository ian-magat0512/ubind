import { browser, by, element, ElementFinder, protractor, WebElement, Button } from 'protractor';

export class AppPage {

    waitTimeSecondsDefault = 300;

    public constructor() {
    }

    setBaseUrl(baseUrl) {
        return browser.baseUrl = baseUrl;
    }

    navigateTo(destination) {
        return browser.get(destination);
    }

    waitForElementClickable(elementLocator, command, waitInSeconds?) {
        waitInSeconds = waitInSeconds == null ? this.waitTimeSecondsDefault : waitInSeconds;

        return browser.wait(protractor.ExpectedConditions.elementToBeClickable(elementLocator), waitInSeconds * 1000)
            .catch(x => {
                console.log("waiting for clickability failed : " + command);
                throw x;
            });
    }

    waitForPage(expectedUrl, timeout) {
        var loaded = false;

        browser.wait(function () {
            browser.executeScript(function () {
                return {
                    url: window.location.href,
                    haveAngular: !!window['angular']
                };
            }).then(function (obj: any) {
                loaded = (obj.url == expectedUrl && obj.haveAngular);
            });

            return loaded;
        }, timeout);
    }

    async waitForElement(elementLocator, command = null, waitInSeconds = 30) {
        var seconds = 0;

        this.sleepInSeconds(.2);

        seconds = waitInSeconds * 1000;

        return browser.wait(protractor.ExpectedConditions.visibilityOf(elementLocator), seconds).catch(e => {
            throw { exception: e.stack, command: command };
        });
    }

    sleepInSeconds(seconds) {
        return browser.sleep(seconds * 1000);
    }

    sendKeys(elementId, value) {
        let el = element(by.id(elementId));
        return this.waitForElement(el, elementId).then(x => {
            return el.sendKeys(value);
        });
    }

    setIonicInput(element: ElementFinder, value) {
        return this.waitForElement(element, "input").then(x => {
            return browser.executeScript("arguments[0].setAttribute('value','" + value + "');", element);
        });
    }

    //scrolls into view to the elementId, if there is an innerTagElementProvided it will focus on that via its index
    scrollIntoView(elementId, innerTagElement = null, index = 0) {
        var scrollToScript = 'if(document.getElementById("' + elementId + '"))document.getElementById("' + elementId + '").scrollIntoView();';

        if (innerTagElement != null) {
            scrollToScript = 'if(document.getElementById("' + elementId + '"))document.getElementById("' + elementId + '").getElementsByTagName("' + innerTagElement + '")[' + index + '].scrollIntoView();';
        }

        return browser.driver.executeScript(scrollToScript);
    }

    clickSegment(segmentId, value) {
        if (!browser.segmentTmp)
            browser.segmentTmp = [];
        browser.segmentTmp.push({ id: segmentId, value: value });
        browser.tmpFunction = this.sleepInSeconds;

        let segment = element(by.id(segmentId));

        return this.waitForElement(segment, segmentId).then(x => {
            return this.getInnerElementsTexts(segment, 'ion-segment-button').then(array => {
                let o = 0;
                for (o = 0; o < array.length; o++) {
                    let button = browser.segmentTmp[0];
                    if (typeof button === 'undefined') {
                        console.log(button!.value + "Button" + o + " button doesnt exist");
                        break;
                    }

                    if (button!.value.toUpperCase() == array[o].toUpperCase()) {
                        browser.segmentTmp.shift();
                        browser.tmpFunction(.3);
                        return element(by.id(button!.id)).all(by.tagName('ion-segment-button')).get(o).click();
                        break;
                    }
                }
            });
        });
    }

    ionListToggleAll(ionListElementId: string) {
        let listElement = element(by.id(ionListElementId));
        return this.waitForElement(listElement, ionListElementId).then(x => {
            return listElement!.all(by.tagName("ion-item")).then(async items => {
                for (let o = 0; o <= items.length; o++) {
                    if (items[o])
                        await items[o]!.getText().then(async text => {
                            if (text.toLowerCase().indexOf('disabled') >= 0) {
                                return await items[o]!.all(by.tagName('ion-toggle')).then(async toggles => {
                                    return await toggles[0].click();
                                })
                            }
                        }).catch(x => {
                            throw x;
                        });
                }
                browser.listTmp.shift();
            });
        });
    }

    //requirements to use this are Identifiers of the ionic-items and ionic-lists
    clickIonList(ionListElementId: string, value = "", reinitialize = true, takeAction = true, scroll = true) {
        let innerElementType = 'ion-item';
        let listElement = element(by.id(ionListElementId));
        let _this = this;
        return this.waitForElement(listElement, ionListElementId).then(async x => {
            let getInnerElements = undefined;

            if (reinitialize) {
                if (!browser.listTmp)
                    browser.listTmp = [];
                browser.listTmp.push({ id: ionListElementId, value: value });
                getInnerElements = _this.getInnerElementsTexts(listElement, innerElementType);
            }
            else {
                getInnerElements = _this.getInnerElementsTexts(listElement, innerElementType);
            }

            return getInnerElements.then(async array => {
                let o = 0;
                let item = browser.listTmp[0];
                var lowerCaseArray = array.map(function (value) {
                    return value.toUpperCase();
                });
                var indexOfValue = lowerCaseArray.indexOf(item.value.toUpperCase());
                var val = array.find(a => a.includes(item.value));
                indexOfValue = array.indexOf(val);
                //found the value
                if (indexOfValue >= 0 || value == "") {
                    if (value == "")
                        indexOfValue = 0;
                    browser.listTmp.shift();
                    _this.sleepInSeconds(.3);
                    if (takeAction) {
                        return element(by.id(item.id)).all(by.tagName(innerElementType)).get(indexOfValue).click().then(x => {
                            return true;
                        }).catch(e => {
                            return false;
                        });
                    }
                    else {
                        return true;
                    }
                }
                else {
                    //search for the value
                    for (o = 0; o <= array.length; o++) {
                        if (array[o] != "") {
                            //not able to find the value
                            if (array[o + 1] == undefined || !scroll) {
                                browser.listTmp.shift();
                                _this.sleepInSeconds(.3);
                                return false;
                            }

                            //possibility that there is another set of records to scroll to.
                            if (array[o + 1] == "") {
                                let lastIndex = o;

                                if (scroll) {
                                    //scroll into view of the these ids
                                    return _this.scrollIntoView(item.id, innerElementType, lastIndex).then(async x => {
                                        //do inner elements again after focusing on the last index
                                        return _this.clickIonList(item.id, item.value, false, takeAction);
                                    });
                                }
                                else {
                                    browser.listTmp.shift();
                                    _this.sleepInSeconds(.3);
                                    return false;
                                }
                            }
                        }
                    }
                }
            });
        });
    }

    clickIonicMenu(value) {
        let sideMenuId = 'side-menu-ion-list';
        let listElement = element(by.id(sideMenuId));
        let _this = this;
        return this.waitForElement(listElement, sideMenuId).then(x => {
            if (!browser.menuTmp)
                browser.menuTmp = [];
            browser.menuTmp.push({ value: value });
            return element.all(by.id(sideMenuId)).then(async listElements => {
                (<ElementFinder[]>listElements)!.forEach(async listElement => {
                    await _this.getInnerElementsTexts(listElement, 'ion-item').then(array => {
                        let o = 0;
                        for (o = 0; o <= array.length; o++) {
                            let button = browser.menuTmp[0];
                            if (button!.value == array[o]) {
                                browser.menuTmp.shift();
                                _this.sleepInSeconds(.3);
                                return listElement.all(by.tagName('ion-item')).get(o).click();
                            }
                        }
                    }).catch(e => {
                        throw e;
                    });
                });
            });
        });
    }

    //clicks a button inside any element
    clickButton(elementId, value) {
        var el = element(by.id(elementId));
        return this.getInnerElementsTexts(el, "button").then(texts => {
            for (let i = 0; i < texts.length; i++) {
                var text = texts[i];
                if (value.toLowerCase() == text.toLowerCase()) {
                    return el.all(by.tagName("button")).get(i).click();
                }
            }
        });
    }

    getInnerElementsTexts(element, tagName): Promise<Array<string>> {
        return element.all(by.tagName(tagName)).getText();
    }

    //allows to retry a promise
    retryPromise(retries, executor, waitTimeInSeconds = 5) {
        if (typeof retries !== 'number') {
            throw new TypeError('retries is not a number')
        }

        return new Promise(executor).then(x => {

        }).catch(error => {
            if (retries > 0) {
                return this.sleepInSeconds(waitTimeInSeconds).then(x => {
                    return this.retryPromise(retries - 1, executor, waitTimeInSeconds);
                });
            }
            else {
                Promise.reject(error);
                throw error;
            }
        });
    }
}
