// Function that takes a number as a parameter and returns the formatted number with groups of 3 digits separated by commas.
// Examples: 1234 -> 1,234, 1234567 -> 1,234,567, 1234567890 -> 1,234,567,890.
export function commas(number) {
    return number.toLocaleString('en-US');
};

// Function that takes a number as a parameter and returns the formatted number in a compact format with 2 significant digits.
// Examples: 1,240 -> 1.2k, 1,200,000 -> 1.2M, 1,200,000,000 -> 1.2B.
// If the number is less than 1000, it is returned as is.
export function compact(number) {
    const abs = Math.abs(number);
    const sign = number < 0 ? '-' : '';

    if (abs < 1_000) {
        return number.toString();
    }

    const units = {
        'k': 1_000,
        'M': 1_000_000,
        'B': 1_000_000_000,
        'T': 1_000_000_000_000
    };

    for (const unit in units) {
        if (abs < units[unit] * 1_000) {
            const result = Math.floor(abs / units[unit]);
            const rest = abs % units[unit];
            if (result < 10) {
                var roundedRest = Math.round(rest / (units[unit] / 10));
                if (roundedRest < 10)
                    return `${sign}${result}.${roundedRest}${unit}`;
                else
                    return `${sign}${result + 1}.0${unit}`;
            }
            else
                return `${sign}${Math.round(abs / units[unit])}${unit}`;
        }
    }

    return commas(Math.round(number / 1_000_000_000_000)) + 'T';
};

// Formats a percentage
export function percent(number) {
    if (number == 0)
        return '0%';
    if (number >= 10)
        return `${Math.round(number)}%`;
    if (number >= 0.1)
        return number.toFixed(1) + '%';
    return '<0.1%';
}

// Formats a percentage with n significant digits after the decimal point
export function percentDecimal(number, digits) {
    if (number > 1)
        return number.toFixed(digits) + '%';
   
    const decimals = number.toString().split('.')[1];
    for (let i = 0; i < decimals.length; i++) {
        if (decimals[i] != '0') {
            const formatted = number.toFixed(digits + i);
            if (formatted.endsWith('00'))
                return number.toFixed(digits + i - 1) + '%';
            else
                return formatted + '%';
        }
    }

    return number.toString() + '%';
}