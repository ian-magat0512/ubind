import { browser, by, element, ProtractorBy, Button, protractor, ElementFinder } from 'protractor';
import { Driver } from 'selenium-webdriver/edge';
var path = require('path');

export class AppPage {
  public actionCount: number = 0;
  public waitTimeSecondsDefault: number = 120;

  constructor() {
    if (!browser.radioTmp) {
      browser.radioTmp = [];
    }

    if (!browser.footerClickTmp) {
      browser.footerClickTmp = [];
    }

    if (!browser.buttonTmp) {
      browser.buttonTmp = [];
    }

  }

  // pass in a frameid or a frame elementfinder
  public async focusToFrameByElement(el: ElementFinder, logMessage) {
    return await browser.switchTo().frame(el.getWebElement()).then((x) => {
      console.log("focus to frame by element:" + logMessage);
      return browser.getPageSource().then((x) => {
        return x
      });
    });
  }

  public async focusToFrameById(frameId: string, waitInSeconds = 600) {
    var el = element(by.id(frameId));
    return this.waitForElementVisible(el, frameId, waitInSeconds).then((x) => {
      return browser.switchTo().frame(el.getWebElement()).then((x) => {
        console.log("focus to frame by id:" + frameId);
        return browser.getPageSource().then((x) => {
          return x
        });
      });
    });
  }

  private async backToMainFraim() {
    return await browser.driver.switchTo().defaultContent().then((x) => {
      browser.waitForAngular();
      return browser.getPageSource().then((x) => {
        return x
      });
    });
  }

  public backToFrame(frameId: string, waitInSeconds: number = 300) {
    return this.backToMainFraim().then((x) => {
      return this.focusToFrameById(frameId, waitInSeconds);
    });
  }

  public waitForElementVisible(elementLocator, command, waitInSeconds?) {
    var seconds = 0;
    let _this = this;
    return _this.sleepInSeconds(.2, false).then((x) => {
      waitInSeconds = waitInSeconds == null ? _this.waitTimeSecondsDefault : waitInSeconds;

      _this.actionCount++;
      return browser.wait(protractor.ExpectedConditions.presenceOf(elementLocator), waitInSeconds * 1000).then((x) => {
        return browser.wait(protractor.ExpectedConditions.visibilityOf(elementLocator), waitInSeconds * 1000);
      }).catch((e) => {
        console.log("waiting for visibility failed : " + command);
        throw e;
      });
    });
  }

  public waitForElementClickable(elementLocator, command, waitInSeconds?) {
    waitInSeconds = waitInSeconds == null ? this.waitTimeSecondsDefault : waitInSeconds;

    return browser.wait(protractor.ExpectedConditions.elementToBeClickable(elementLocator), waitInSeconds * 1000)
      .catch((x) => {
        console.log("waiting for clickability failed : " + command);
        throw x;
      });
  }

  public setBaseUrl(baseUrl) {
    return browser.baseUrl = baseUrl;
  }

  public navigateTo(destination) {
    return browser.get(destination);
  }

  public sleepInSeconds(seconds, log = true) {
    return browser.sleep(seconds * 1000).then((x) => {
      if (log) {
        console.log("slept in : " + seconds * 1000);
      }
    });
  }

  public processSendKeys(anchorElementId: string, value, waitTimeInSeconds?: number) {
    anchorElementId = this.appendPrefixIfDoesntExist("anchor-", anchorElementId);
    var anchorElement = element(by.css('div[id="' + anchorElementId + '"]'));
    let _this = this;

    return _this.waitForElementVisible(anchorElement, anchorElementId, waitTimeInSeconds).then(async (x) => {

      var field = await _this.getTagName(anchorElement).then((x) => {
        return x;
      });

      var elementFinder: ElementFinder = null;
      var datePickerFields: ElementFinder[] = [];
      switch (field) {
        case 'textarea-field':
          elementFinder = anchorElement.all(by.tagName('textarea')).get(0);
          break;
        case 'select-field':
          elementFinder = anchorElement.all(by.tagName('select')).get(0);
          break;
        case 'datepicker-field':
          // element somehow changed for mobile and desktop viewPortTypes
          if (browser.viewPortType == "mobile") {
            datePickerFields.push(anchorElement.all(by.tagName('input')).get(0));
            datePickerFields.push(anchorElement.all(by.tagName('input')).get(1));
          } else {
            elementFinder = anchorElement.all(by.tagName('input')).get(0);
          }
          break;
        case 'input-field':
          elementFinder = anchorElement.all(by.tagName('input')).get(0);
          break;
      }

      // datepicker-field is weird. it has two input fields to send keys but only one works,
      if (datePickerFields.length > 0) {
        return _this.datePickerSendKeys(datePickerFields[1], value, anchorElementId).then((x) => {
        }).catch((e) => {
          return _this.datePickerSendKeys(datePickerFields[0], value, anchorElementId).then((x) => {
          }).catch((e) => {
          });
        });
      } else {
        return _this.sendKeys(elementFinder, value, anchorElementId, waitTimeInSeconds);
      }
    });
  }

  private sendKeys(elementFinder, value, anchorElementId, waitTimeInSeconds) {
    return this.waitForElementVisible(elementFinder, anchorElementId, waitTimeInSeconds).then(async (x) => {
      elementFinder.clear().then((x) => { }).catch((x) => { });
      return elementFinder.sendKeys(value).then((x) => {
        console.log("sent key : " + anchorElementId);
      }).catch((x) => {
        console.log("error on send key: " + anchorElementId);
        throw x;
      });
    });
  }

  private datePickerSendKeys(elementFinder, value, anchorElementId) {
    return this.sleepInSeconds(.3, false).then((x) => {
      return browser.actions().mouseMove(elementFinder).click()
        .sendKeys(protractor.Key.ARROW_LEFT)
        .sendKeys(protractor.Key.ARROW_LEFT)
        .sendKeys(protractor.Key.ARROW_LEFT)
        .sendKeys(protractor.Key.ARROW_LEFT)
        .sendKeys(value).perform().then((x) => {
          console.log("sent key : " + anchorElementId);
        }).catch((e) => {
          console.log("error on send key: " + anchorElementId);
          throw e;
        });
    })
  }

  public attachmentUpload(anchorElementId: string, filePath: string, waitTimeSeconds?: number) {
    anchorElementId = this.appendPrefixIfDoesntExist("anchor-", anchorElementId);
    var absolutePath = path.resolve(__dirname, filePath);
    var el = element(by.css('div[id="' + anchorElementId + '"]'));

    return this.waitForElementVisible(el, anchorElementId, waitTimeSeconds).then((x) => {

      return el.all(by.css('input[type="file"]')).get(0).sendKeys(absolutePath).then((x) => {
        console.log('attachment upload :' + anchorElementId);
      }).catch((x) => {
        console.log('error on attachment upload :' + anchorElementId);
        throw x;
      });
    });
  }

  // clicks a child element via its id
  public childClick(parentId: string, selectionId: string) {
    var el = element(by.id(parentId));
    var innerEl = el.all(by.id(selectionId)).get(0);
    return this.waitForElementVisible(innerEl, selectionId).then((x) => {
      browser.executeScript("arguments[0].click();", innerEl);
      return innerEl.click().then(() => {
        console.log("clicked iframe selection: " + parentId);
      }).catch((x) => {
        console.log("error on iframe selection: " + parentId);
        throw x;
      });
    });
  }

  public checkboxClick(anchorElementId, waitTimeSeconds?: number) {
    anchorElementId = this.appendPrefixIfDoesntExist("anchor-", anchorElementId);
    var el = element(by.id(anchorElementId));
    var innerEl = el.all(by.tagName('label')).get(0);

    return this.waitForElementVisible(el, anchorElementId, waitTimeSeconds).then((x) => {
      var command = "arguments[0].classList.add('selected');";

      return browser.executeScript(command, innerEl).then((x) => {
        return innerEl.click().then((x) => {
          console.log("clicked checkbox: " + anchorElementId);
        }).catch((x) => {
          console.log("error on checkbox: " + anchorElementId);
          throw x;
        });
      });
    });
  }

  public footerClickNext(waitTimeSeconds: number = 300) {
    let _this = this;
    return this.sleepInSeconds(15)
      .then((x) => {
        return this.FooterClick('next', waitTimeSeconds);
      }).then((x) => {

        // reclick this is usually for australian reliance having errors
        return _this.closeModalPopup(5).then((x) => {
          return _this.FooterClick('next', 5).catch((e) => {
          });
        }).catch((e) => {
        });
      });
  }

  public closeModalPopup(waitInSeconds) {
    let _this = this;
    var closeSelector = "button[aria-label='Close']";
    var el = element(by.css(closeSelector));
    return this.waitForElementVisible(el, "close popup", waitInSeconds).then((x) => {
      return el.click().then((x) => {
        return _this.sleepInSeconds(waitInSeconds);
      });
    });
  }

  public FooterClickBack(waitTimeSeconds: number = 300) {
    return this.FooterClick('back', waitTimeSeconds);
  }

  public FooterClick(buttonId: string, waitTimeSeconds?: number) {
    browser.footerClickTmp.push({ id: buttonId, retryCount: 20 });

    let _this = this;
    var footerEl = element(by.id("formFooter-actions"));
    return this.waitForElementVisible(footerEl, buttonId, waitTimeSeconds / 20).then((x) => {
      return _this.ProcessFooterClick(footerEl, buttonId, waitTimeSeconds);
    });
  }

  private ProcessFooterClick(footerEl, buttonId, waitTimeSeconds) {
    var buttonDetails = browser.footerClickTmp[0];
    return footerEl.all(by.id(buttonDetails.id)).then((x) => {
      if (browser.footerClickTmp.length > 0) {
        let btn: ElementFinder = x[0];

        return this.waitForElementClickable(btn, "next", waitTimeSeconds / 20)
          .then((x) => {
            return btn.isEnabled().then((x) => {
              return btn.click().then((x) => {
                browser.footerClickTmp.shift();
                this.actionCount = 0;
                console.log('clicked button: ' + buttonId);
              }).catch((x) => {
                console.log('error on button: ' + buttonId);
                throw x;
              });
            });
          });
      }
    }).catch((x) => {
      if (buttonDetails!.retryCount > 0) {
        buttonDetails.retryCount = buttonDetails.retryCount - 1;
        return this.ProcessFooterClick(footerEl, buttonId, waitTimeSeconds);
      } else {
        browser.footerClickTmp.shift();
        this.actionCount = 0;
        console.log("error on get inner buttons for footer buttons : " + buttonId);
        throw x;
      }
    });
  }

  public sidebarClickNext() {
    browser.nextButtonClicked = false;
    return element(by.id("calculationWidgetFooter-actions")).isEnabled().then((x) => {
      return element(by.id("calculationWidgetFooter-actions")).all(by.id("next")).then((x) => {
        if (x[0] === 'undefined' || x[0] == null) {
          browser.nextButtonClicked = false;
        } else {
          if (browser.nextButtonClicked == false) {
            browser.nextButtonClicked = true;
            return x[0].click();
          }
        }
      });
    });
  }

  public radioSelect(anchorElementId: string, value: string, waitTimeSeconds?: number) {
    anchorElementId = this.appendPrefixIfDoesntExist("anchor-", anchorElementId);
    let _this = this;
    let el = element(by.id(anchorElementId));

    browser.radioTmp.push({ id: anchorElementId, value: value });

    var secondSelectionEl = element(by.id(anchorElementId))
      .all(by.tagName('label')).get(1);

    return _this.waitForElementVisible(secondSelectionEl, anchorElementId + "2nd element", waitTimeSeconds).then((x) => {

      return _this.getTextFromInnerLabels(el).then((array) => {
        let o = 0;
        let match = false;
        let button = browser.radioTmp[0];
        for (o = 0; o <= array.length; o++) {

          var secondSelectionEl = element(by.id(button.id)).all(by.tagName('label')).get(1);

          _this.waitForElementVisible(secondSelectionEl, "", waitTimeSeconds);
          if (typeof button === 'undefined') {
            console.log(button!.id + "Button" + o + " button doesnt exist");
            break;
          }

          if (button.value == array[o]) {
            element(by.id(button.id)).all(by.tagName('label')).get(o).click().then((x) => {
              console.log("clicked radio : " + button.id);
            }).catch((x) => {
              console.log("error on radio : " + button.id);
              throw x;
            });

            match = true;
            browser.radioTmp.shift();
            _this.sleepInSeconds(.3);
            break;
          }
        }
        if (!match) {
          browser.radioTmp.shift();
        }
      }).catch((x) => {
        console.log("error on get inner labels for radio : " + anchorElementId);
        throw x;
      });
    });
  }

  // value could be any value on the button
  public buttonClick(anchorElementId: string, value: string, waitTimeSeconds?: number) {
    anchorElementId = this.appendPrefixIfDoesntExist("anchor-", anchorElementId);
    let _this = this;

    var elementId = anchorElementId.replace('anchor-', '');
    let btnGroupEl = element(by.id(elementId));

    browser.buttonTmp.push({ id: elementId, value: value });
    return _this.waitForElementVisible(btnGroupEl, anchorElementId + 1, waitTimeSeconds).then(async (x) => {

      return _this.getTextFromInnerLabels(btnGroupEl).then((array) => {
        let o = 0;
        let button = browser.buttonTmp[0];

        for (o = 0; o <= array.length; o++) {
          if (typeof button === 'undefined') {
            console.log(button!.id + "Button" + o + " button doesnt exist");
            break;
          }

          if (button.value == array[o]) {
            var buttonElement = element(by.id(button.id + "Button" + o));
            return _this.waitForElementVisible(buttonElement, anchorElementId + 2, waitTimeSeconds).then((x) => {
              return buttonElement.click().then((x) => {
                console.log("clicked button: " + button.id + "Button" + o);
                browser.buttonTmp.shift();
                _this.sleepInSeconds(.3);
              }).catch((x) => {
                console.log("error on button: " + button.id + "Button" + o);
                return buttonElement.click().then((x) => {
                  console.log("clicked button: " + button.id + "Button" + o);
                  browser.buttonTmp.shift();
                  _this.sleepInSeconds(.3);
                }).catch((x) => {
                  console.log("error on button: " + button.id + "Button" + o);
                  throw x;
                });
              });
            });
          }
        }
      }).catch((x) => {
        console.log("error on get inner labels for buttons : " + anchorElementId + " " + x);
        throw x;
      });
    });
  }

  // get text inside labels in array form
  private getTextFromInnerLabels(element): Promise<Array<string>> {
    return element.all(by.tagName("label")).getText();
  }

  public getTagName(element: ElementFinder) {
    return element.all(by.css('input-field, radio-field, buttons-field, select-field, datepicker-field, iframe-field, textarea-field, toggle-field, checkbox-field, attachment-field, search-select-field')).get(0).getTagName();
  }

  public appendPrefixIfDoesntExist(string, value) {
    if (value.indexOf(string) < 0) {
      value = string + value;
    }
    return value;
  }

  // allows to retry a promise
  public retryPromise(retries, executor, waitTimeInSeconds = 5) {
    if (typeof retries !== 'number') {
      throw new TypeError('retries is not a number')
    }

    return new Promise(executor).then((x) => {

    }).catch((error) => {
      if (retries > 0) {
        return this.sleepInSeconds(waitTimeInSeconds).then((x) => {
          return this.retryPromise(retries - 1, executor, waitTimeInSeconds);
        });
      } else {
        Promise.reject(error);
        throw error;
      }
    });
  }
}