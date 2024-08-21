import { FormlyFieldConfig } from "@ngx-formly/core";
import { QuestionSetConfigSyncHelper } from "./question-set-config-sync.helper";

describe('QuestionSetConfigSyncHelper', () => {

    it('should remove fields which no longer exist', () => {
        // arrange
        const source: Array<FormlyFieldConfig> = [
            {
                fieldGroup: [
                    getFieldConfig('milk'),
                    getFieldConfig('cheese'),
                ],
            },
            {
                fieldGroup: [
                    getFieldConfig('eggs'),
                ],
            },
        ];

        const target: Array<FormlyFieldConfig> = [
            {
                fieldGroup: [
                    getFieldConfig('bread'),
                ],
            },
            {
                fieldGroup: [
                    getFieldConfig('milk'),
                    getFieldConfig('cheese'),
                ],
            },
            {
                fieldGroup: [
                    getFieldConfig('eggs'),
                    getFieldConfig('tomatoes'),
                ],
            },
        ];

        const milkRef: any = target[1].fieldGroup[0];
        const cheeseRef: any = target[1].fieldGroup[1];
        const eggsRef: any = target[2].fieldGroup[0];

        // act
        QuestionSetConfigSyncHelper.synchronise(source, target);

        // assert
        expect(target[0].fieldGroup[0].key).toBe('milk');
        expect(target[0].fieldGroup[1].key).toBe('cheese');
        expect(target[1].fieldGroup[0].key).toBe('eggs');
        expect(target[0].fieldGroup[0]).toEqual(milkRef);
        expect(target[0].fieldGroup[1]).toEqual(cheeseRef);
        expect(target[1].fieldGroup[0]).toEqual(eggsRef);
        expect(target.length).toBe(2);
    });

    it('should add new items in the right order', () => {
        // arrange
        const source: Array<FormlyFieldConfig> = [
            {
                fieldGroup: [
                    getFieldConfig('bread'),
                ],
            },
            {
                fieldGroup: [
                    getFieldConfig('milk'),
                    getFieldConfig('cheese'),
                ],
            },
            {
                fieldGroup: [
                    getFieldConfig('eggs'),
                    getFieldConfig('tomatoes'),
                ],
            },
        ];

        const target: Array<FormlyFieldConfig> = [
            {
                fieldGroup: [
                    getFieldConfig('milk'),
                    getFieldConfig('cheese'),
                ],
            },
            {
                fieldGroup: [
                    getFieldConfig('eggs'),
                ],
            },
        ];

        // act
        QuestionSetConfigSyncHelper.synchronise(source, target);

        // assert
        expect(target[0].fieldGroup[0].key).toBe('bread');
        expect(target[1].fieldGroup[0].key).toBe('milk');
        expect(target[2].fieldGroup[1].key).toBe('tomatoes');
        expect(target.length).toBe(3);
        expect(target[0].fieldGroup.length).toBe(1);
        expect(target[1].fieldGroup.length).toBe(2);
        expect(target[2].fieldGroup.length).toBe(2);
    });

    it('should reorder items keeping the original objects', () => {
        // arrange
        const source: Array<FormlyFieldConfig> = [
            {
                fieldGroup: [
                    getFieldConfig('tomatoes'),
                ],
            },
            {
                fieldGroup: [
                    getFieldConfig('milk'),
                    getFieldConfig('bread'),
                ],
            },
            {
                fieldGroup: [
                    getFieldConfig('cheese'),
                    getFieldConfig('eggs'),
                ],
            },
        ];

        const target: Array<FormlyFieldConfig> = [
            {
                fieldGroup: [
                    getFieldConfig('bread'),
                ],
            },
            {
                fieldGroup: [
                    getFieldConfig('milk'),
                    getFieldConfig('cheese'),
                ],
            },
            {
                fieldGroup: [
                    getFieldConfig('eggs'),
                    getFieldConfig('tomatoes'),
                ],
            },
        ];

        const breadRef: any = target[0].fieldGroup[0];
        const milkRef: any = target[1].fieldGroup[0];
        const cheeseRef: any = target[1].fieldGroup[1];
        const eggsRef: any = target[2].fieldGroup[0];
        const tomatoesRef: any = target[2].fieldGroup[1];

        // act
        QuestionSetConfigSyncHelper.synchronise(source, target);

        // assert
        expect(target.length).toBe(3);
        expect(target[0].fieldGroup.length).toBe(1);
        expect(target[1].fieldGroup.length).toBe(2);
        expect(target[2].fieldGroup.length).toBe(2);
        expect(target[0].fieldGroup[0].key).toBe('tomatoes');
        expect(target[0].fieldGroup[0]).toEqual(tomatoesRef);
        expect(target[1].fieldGroup[0]).toEqual(milkRef);
        expect(target[1].fieldGroup[1]).toEqual(breadRef);
        expect(target[2].fieldGroup[0]).toEqual(cheeseRef);
        expect(target[2].fieldGroup[1]).toEqual(eggsRef);
    });

    // eslint-disable-next-line prefer-arrow/prefer-arrow-functions
    function getFieldConfig(key: string): any {
        return {
            key: key,
            templateOptions: {
                fieldConfiguration: {},
            },
        };
    }
});
