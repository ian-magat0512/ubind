import { ArrayHelper, ArraySyncStats } from "./array.helper";
import * as _ from 'lodash-es';
import { OptionConfiguration } from "@app/resource-models/configuration/option.configuration";

describe('ArrayHelper', () => {

    it('should remove items which no longer exist', () => {
        // arrange
        const sourceArray: Array<any> = [
            { name: 'milk' },
            { name: 'eggs' },
        ];

        let targetArray: Array<any> = [
            { name: 'bread' },
            { name: 'milk' },
            { name: 'cheese' },
            { name: 'eggs' },
            { name: 'tomatoes' },
        ];

        const milkRef: any = targetArray[1];
        const eggsRef: any = targetArray[3];

        // act
        ArrayHelper.synchronise<any>(sourceArray, targetArray);

        // assert
        expect(targetArray[0].name).toBe('milk');
        expect(targetArray[1].name).toBe('eggs');
        expect(targetArray[0]).toEqual(milkRef);
        expect(targetArray[1]).toEqual(eggsRef);
        expect(targetArray.length).toBe(2);
    });

    it('should add new items', () => {
        // arrange
        const sourceArray: Array<any> = [
            { name: 'apples' },
            { name: 'milk' },
            { name: 'lemons' },
            { name: 'cheese' },
            { name: 'tomatoes' },
            { name: 'ham' },
        ];

        let targetArray: Array<any> = [
            { name: 'milk' },
            { name: 'cheese' },
            { name: 'tomatoes' },
        ];

        // act
        ArrayHelper.synchronise<any>(sourceArray, targetArray);

        // assert
        expect(targetArray[0].name).toBe('apples');
        expect(targetArray[1].name).toBe('milk');
        expect(targetArray[2].name).toBe('lemons');
        expect(targetArray[3].name).toBe('cheese');
        expect(targetArray[4].name).toBe('tomatoes');
        expect(targetArray[5].name).toBe('ham');
        expect(targetArray.length).toBe(6);
    });

    it('should reorder items keeping the original objects', () => {
        // arrange
        const sourceArray: Array<any> = [
            { name: 'tomatoes' },
            { name: 'milk' },
            { name: 'bread' },
            { name: 'cheese' },
            { name: 'eggs' },
        ];

        let targetArray: Array<any> = [
            { name: 'bread' },
            { name: 'milk' },
            { name: 'cheese' },
            { name: 'eggs' },
            { name: 'tomatoes' },
        ];

        const breadRef: any = targetArray[0];
        const milkRef: any = targetArray[1];
        const cheeseRef: any = targetArray[2];
        const eggsRef: any = targetArray[3];
        const tomatoesRef: any = targetArray[4];

        // act
        ArrayHelper.synchronise<any>(sourceArray, targetArray);

        // assert
        expect(targetArray[0].name).toBe('tomatoes');
        expect(targetArray[1].name).toBe('milk');
        expect(targetArray[2].name).toBe('bread');
        expect(targetArray[3].name).toBe('cheese');
        expect(targetArray[4].name).toBe('eggs');
        expect(targetArray[0]).toEqual(tomatoesRef);
        expect(targetArray[1]).toEqual(milkRef);
        expect(targetArray[2]).toEqual(breadRef);
        expect(targetArray[3]).toEqual(cheeseRef);
        expect(targetArray[4]).toEqual(eggsRef);
        expect(targetArray.length).toBe(5);
    });

    it('should call the dispose function on items that were not added because they already exist', () => {
        // arrange
        const sourceArray: Array<any> = [
            { name: 'milk' },
            { name: 'eggs' },
        ];

        let targetArray: Array<any> = [
            { name: 'milk' },
            { name: 'eggs' },
        ];

        // act
        let disposedItems: Array<any> = new Array<any>();
        let disposeFunction: (item: any) => void = (item: any): void => {
            disposedItems.push(item);
        };
        ArrayHelper.synchronise<any>(sourceArray, targetArray, _.isEqual, disposeFunction);

        // assert
        expect(disposedItems.includes(sourceArray[0])).toBeTruthy();
        expect(disposedItems.includes(sourceArray[1])).toBeTruthy();
        expect(disposedItems.includes(targetArray[0])).toBeFalsy();
        expect(disposedItems.includes(targetArray[1])).toBeFalsy();
    });

    it('should call the dispose function on items that were removed', () => {
        // arrange
        const sourceArray: Array<any> = [
            { name: 'milk' },
            { name: 'eggs' },
        ];

        let targetArray: Array<any> = [
            { name: 'milk' },
            { name: 'eggs' },
            { name: 'bread' },
            { name: 'butter' },
        ];

        const breadRef: any = targetArray[2];
        const butterRef: any = targetArray[3];

        // act
        let disposedItems: Array<any> = new Array<any>();
        let disposeFunction: (item: any) => void = (item: any): void => {
            disposedItems.push(item);
        };
        ArrayHelper.synchronise<any>(sourceArray, targetArray, _.isEqual, disposeFunction);

        // assert
        expect(disposedItems.includes(breadRef)).toBeTruthy();
        expect(disposedItems.includes(butterRef)).toBeTruthy();
    });

    it('should produce stats on items added and removed', () => {
        // arrange
        const sourceArray: Array<OptionConfiguration> = [
            {
                "label": "No Cover",
                "value": "0",
                "icon": "glyphicon glyphicon-remove md custom",
            },
            {
                "label": "$2 million",
                "value": "2000000",
                "icon": "icon-2million",
            },
            {
                "label": "$5 million",
                "value": "5000000",
                "icon": "icon-5million",
            },
            {
                "label": "$10 million",
                "value": "10000000",
                "icon": "icon-10million",
            },
        ];

        let targetArray: Array<OptionConfiguration> = [
            {
                "label": "No Cover",
                "value": "0",
                "icon": "glyphicon glyphicon-remove md custom",
            },
            {
                "label": "$1,000",
                "value": "1000",
                "icon": "icon-1million",
            },
            {
                "label": "$5,000",
                "value": "5000",
                "icon": "icon-2million",
            },
            {
                "label": "$8,000",
                "value": "8000",
                "icon": "icon-5million",
            },
            {
                "label": "$12,000",
                "value": "12000",
                "icon": "icon-10million",
            },
        ];

        // act
        let stats: ArraySyncStats = ArrayHelper.synchronise<any>(sourceArray, targetArray);

        // assert
        expect(targetArray.length).toBe(4, "The target array should be the same length as the sourceArray");
        expect(stats.added).toBe(3, "Three items should have been added");
        expect(stats.removed).toBe(4, "Four items should have been removed");
        expect(targetArray[0].label).toBe('No Cover');
        expect(targetArray[1].label).toBe('$2 million');
        expect(targetArray[2].label).toBe('$5 million');
        expect(targetArray[3].label).toBe('$10 million');
    });

});
