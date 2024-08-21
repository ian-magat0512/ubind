import parse from 'color-parse';
import convert from 'color-convert';
import { ColorHsl } from "@app/models/color-hsl";
import { ElementRef } from '@angular/core';

/**
 * Provides helper functions for color.
 */
export class ColorHelper {

    /**
     * This is to convert those css color to HSL.
     * @param color the selectionBar color or accent color
     * @returns the color HSL model that being convert.
     */
    public static convertCssColorToHsl(color: string): ColorHsl {
        if (!color) {
            return null;
        }

        const colorValue: any = parse(color);
        if (colorValue.space == 'rgb') {
            const hsl: any = convert.rgb.hsl(colorValue.values[0], colorValue.values[1], colorValue.values[2]);
            const colorHsl: ColorHsl = {
                hue: hsl[0],
                saturation: hsl[1],
                lightness: hsl[2],
            };

            return colorHsl;
        } else if (colorValue.space == 'hsl') {
            const colorHsl: ColorHsl = {
                hue: colorValue.values[0],
                saturation: colorValue.values[1],
                lightness: colorValue.values[2],
            };

            return colorHsl;
        } else {
            return null;
        }
    }

    /**
     * This function is used to get the computed color of an element.
     * It get's the background color of the element after applying color from css variable.
     */
    public static getComputedColor(cssVariable: string, colorReference: ElementRef): string {
        if (colorReference && colorReference.nativeElement) {
            colorReference.nativeElement.style.backgroundColor = cssVariable;
            let backGroundColor: string = window.getComputedStyle(colorReference.nativeElement).backgroundColor;
            return backGroundColor !== 'rgba(0, 0, 0, 0)' ? backGroundColor : null;
        }

        return null;
    }

    public static getUnselectedBarColor(baseColorHsl: ColorHsl): ColorHsl {
        const baseColorHue: number = parseInt(baseColorHsl.hue, 10);
        const baseColorSaturation: number = parseInt(baseColorHsl.saturation, 10);
        const baseColorLightness: number = parseInt(baseColorHsl.lightness, 10);
        const maxSaturation: number = 35;
        const saturationMultiplier: number = 0.35;
        const lightnessMultiplier: number = 3;
        let saturation: number = 0;
        let lightness: number = 0;

        if (baseColorSaturation < 75 && baseColorHue > 120) {
            let totalSaturation: number = baseColorSaturation * saturationMultiplier;
            saturation = totalSaturation < maxSaturation ? maxSaturation : totalSaturation;
        } else if (baseColorSaturation >= 75 && baseColorHue < 120) {
            saturation = baseColorSaturation * 0.7;
        } else {
            saturation = baseColorSaturation * saturationMultiplier;
        }

        if (baseColorLightness >= 35 && baseColorHue < 120) {
            lightness = baseColorLightness * 1.5;
        } else {
            lightness = baseColorLightness * lightnessMultiplier;
        }

        saturation = Math.max(0, Math.min(90, saturation));
        lightness = Math.max(0, Math.min(90, lightness));

        const unselectedBarColorHsl: ColorHsl = {
            hue: `${baseColorHue}`,
            saturation: `${saturation}%`,
            lightness: `${lightness}%`,
        };

        return unselectedBarColorHsl;
    }
}
