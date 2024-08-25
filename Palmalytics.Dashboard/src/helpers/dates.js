const formatMap = {
    'd': d => d.toLocaleString('default', { day: 'numeric' }),
    'dd': d => d.toLocaleString('default', { day: '2-digit' }),
    'M': d => d.toLocaleString('default', { month: 'numeric' }),
    'MM': d => d.toLocaleString('default', { month: '2-digit' }),
    'MMM': d => d.toLocaleString('default', { month: 'short' }),
    'MMMM': d => d.toLocaleString('default', { month: 'long' }),
    'yy': d => d.toLocaleString('default', { year: '2-digit' }),
    'yyyy': d => d.toLocaleString('default', { year: 'numeric' })
};

// Formats a date according to the given format string
export function format(value, format = 'dd-MMM-yy') {
    if (value == null)
        return null;
    if (typeof value === 'undefined')
        return 'undefined';
    
    const date = typeof value === 'string' ? new Date(value) : value;
    
    return format.replace(/d{1,4}|M{1,4}|yy(?:yy)?/g, match => formatMap[match](date));
}