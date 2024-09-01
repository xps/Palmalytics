import { describe, expect, test } from 'vitest';
import { format } from '../../src/helpers/dates';

describe('format', () => {
    test('returns the formatted date in the default format if no format is provided', () => {
        const date = new Date('2022-03-01');
        const formattedDate = format(date);
        expect(formattedDate).toBe('01-Mar-22');
    });

    test('returns the formatted date in the "d-MMM-yy" format', () => {
        const date = new Date('2022-03-01');
        const formattedDate = format(date, 'd-MMM-yy');
        expect(formattedDate).toBe('1-Mar-22');
    });

    test('returns the formatted date in the "yyyy-MM-dd" format', () => {
        const date = new Date('2022-03-01');
        const formattedDate = format(date, 'yyyy-MM-dd');
        expect(formattedDate).toBe('2022-03-01');
    });

    test('returns the formatted date in the "M/d/yyyy" format', () => {
        const date = new Date('2022-03-01');
        const formattedDate = format(date, 'M/d/yyyy');
        expect(formattedDate).toBe('3/1/2022');
    });

    test('returns the formatted date in the "ddd MMM yy" format', () => {
        const date = new Date('2022-03-01');
        const formattedDate = format(date, 'MMM yy');
        expect(formattedDate).toBe('Mar 22');
    });
    
    test('returns the formatted date in the "MMMM yyyy" format', () => {
        const date = new Date('2022-03-10');
        const formattedDate = format(date, 'MMMM yyyy');
        expect(formattedDate).toBe('March 2022');
    });

    test('returns the formatted date in the "ddd" format', () => {
        const date = new Date('2022-03-10');
        const formattedDate = format(date, 'ddd');
        expect(formattedDate).toBe('Thu');
    });

    test('returns the formatted date in the "dddd" format', () => {
        const date = new Date('2022-03-10');
        const formattedDate = format(date, 'dddd');
        expect(formattedDate).toBe('Thursday');
    });
});