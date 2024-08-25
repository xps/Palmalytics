import { describe, expect, test } from 'vitest';
import { format } from '../../src/helpers/durations';

describe('format', () => {
    test('should return the duration correctly formatted', () => {
        expect(format(0)).toBe('0s');
        expect(format(10)).toBe('10s');
        expect(format(60)).toBe('1m 0s');
        expect(format(70)).toBe('1m 10s');
        expect(format(120)).toBe('2m 0s');
        expect(format(130)).toBe('2m 10s');
        expect(format(3600)).toBe('1h 0m 0s');
        expect(format(3660)).toBe('1h 1m 0s');
        expect(format(3670)).toBe('1h 1m 10s');
        expect(format(7199)).toBe('1h 59m 59s');
    });
});
