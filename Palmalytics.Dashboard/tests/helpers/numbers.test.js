import { describe, expect, test } from 'vitest';
import { commas, compact, percent, percentDecimal } from '../../src/helpers/numbers';

describe('commas', () => {
    test('should return the number correctly formatted', () => {
        expect(commas(1)).toBe('1');
        expect(commas(12)).toBe('12');
        expect(commas(123)).toBe('123');
        expect(commas(1_234)).toBe('1,234');
        expect(commas(12_345)).toBe('12,345');
        expect(commas(123_456)).toBe('123,456');
        expect(commas(1_234_567)).toBe('1,234,567');
        expect(commas(12_345_678)).toBe('12,345,678');
        expect(commas(123_456_789)).toBe('123,456,789');
        expect(commas(1_234_567_890)).toBe('1,234,567,890');
    });

    test('should return the correct sign', () => {
        expect(commas(-1)).toBe('-1');
        expect(commas(-12)).toBe('-12');
        expect(commas(-123)).toBe('-123');
        expect(commas(-1_234)).toBe('-1,234');
        expect(commas(-12_345)).toBe('-12,345');
        expect(commas(-123_456)).toBe('-123,456');
        expect(commas(-1_234_567)).toBe('-1,234,567');
        expect(commas(-12_345_678)).toBe('-12,345,678');
        expect(commas(-123_456_789)).toBe('-123,456,789');
        expect(commas(-1_234_567_890)).toBe('-1,234,567,890');
    });
});

describe('compact', () => {
    test('should return the full number if it is less than 1000', () => {
        expect(compact(999)).toBe('999');
    });

    test('should return the number in a compact format with 2 significant digits', () => {
        expect(compact(1_000)).toBe('1.0k');
        expect(compact(1_234)).toBe('1.2k');
        expect(compact(1_267)).toBe('1.3k');
        expect(compact(3_967)).toBe('4.0k');
        expect(compact(12_345)).toBe('12k');
        expect(compact(12_545)).toBe('13k');
        expect(compact(123_456)).toBe('123k');
        expect(compact(123_678)).toBe('124k');

        expect(compact(1_000_000)).toBe('1.0M');
        expect(compact(1_234_567)).toBe('1.2M');
        expect(compact(1_256_789)).toBe('1.3M');
        expect(compact(3_967_789)).toBe('4.0M');
        expect(compact(12_345_678)).toBe('12M');
        expect(compact(123_456_789)).toBe('123M');
        expect(compact(123_567_890)).toBe('124M');

        expect(compact(1_000_000_000)).toBe('1.0B');
        expect(compact(1_234_567_890)).toBe('1.2B');
        expect(compact(1_256_789_012)).toBe('1.3B');
        expect(compact(3_967_789_012)).toBe('4.0B');
        expect(compact(12_345_678_901)).toBe('12B');
        expect(compact(123_456_789_012)).toBe('123B');
        expect(compact(123_567_890_123)).toBe('124B');

        expect(compact(1_000_000_000_000)).toBe('1.0T');
        expect(compact(1_234_567_890_123_456)).toBe('1,235T');
    });

    test('should return the correct sign', () => {
        expect(compact(-1_000)).toBe('-1.0k');
        expect(compact(-1_256_789)).toBe('-1.3M');
        expect(compact(-1_234_567_890_123_456)).toBe('-1,235T');
    });
});

describe('percent', () => {
    test('should return the formatted percentage', () => {
        expect(percent(0)).toBe('0%');
        expect(percent(0.09)).toBe('<0.1%');
        expect(percent(0.123)).toBe('0.1%');
        expect(percent(0.156)).toBe('0.2%');
        expect(percent(5.000)).toBe('5.0%');
        expect(percent(5.123)).toBe('5.1%');
        expect(percent(5.156)).toBe('5.2%');
        expect(percent(10)).toBe('10%');
        expect(percent(23.25)).toBe('23%');
        expect(percent(23.56)).toBe('24%');
        expect(percent(99.99)).toBe('100%');
        expect(percent(100)).toBe('100%');
    });
});

describe('percentDecimal', () => {
    test('should return the formatted percentage', () => {
        expect(percentDecimal(100, 2)).toBe('100.00%');
        expect(percentDecimal(90.5121, 2)).toBe('90.51%');
        expect(percentDecimal(5.6744, 2)).toBe('5.67%');
        expect(percentDecimal(5.6789, 2)).toBe('5.68%');
        expect(percentDecimal(1.00001, 2)).toBe('1.00%');
        expect(percentDecimal(0.09934, 2)).toBe('0.099%');
        expect(percentDecimal(0.09984, 2)).toBe('0.10%');
        expect(percentDecimal(0.00000123, 2)).toBe('0.0000012%');
    });
});