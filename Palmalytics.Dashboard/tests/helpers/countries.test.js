import { describe, expect, test } from 'vitest';
import * as countries from '../../src/helpers/countries';

describe('getCountryName', () => {
    test('should return the country name from the code', () => {
        expect(countries.getCountryName('US')).toBe('United States');
        expect(countries.getCountryName('DE')).toBe('Germany');
        expect(countries.getCountryName('CN')).toBe('China');
    });
});

describe('getCountryFlag', () => {
    test('should return the country flag from the code', () => {
        expect(countries.getCountryFlag('US')).toBe('🇺🇸');
        expect(countries.getCountryFlag('DE')).toBe('🇩🇪');
        expect(countries.getCountryFlag('CN')).toBe('🇨🇳');
    });
});