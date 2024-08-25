import '../styles/components/interval-selector.css';

const units = {
    'days': 'Days',
    'weeks': 'Weeks',
    'months': 'Months',
    'years': 'Years'
};

const availableUnitsPerPeriod = {
    'today': ['days'],
    'last-7-days': ['days'],
    'last-30-days': ['days'],
    'last-12-months': ['days', 'weeks', 'months'],
    'month-to-date': ['days'],
    'last-month': ['days'],
    'year-to-date': ['days', 'weeks', 'months'],
    'last-year': ['days', 'weeks', 'months'],
    'all-time': ['weeks', 'months', 'years'],
};

export default function IntervalSelector({ value, period, onChanged }) {
    if (!availableUnitsPerPeriod[period].includes(value)) {
        value = availableUnitsPerPeriod[period][0];
        onChanged(value);
    }

    return (
        <div id="interval-selector">
            <div className="btn-group ms-auto">
                <button className="btn dropdown-toggle" type="button" data-bs-toggle="dropdown" aria-expanded="false">
                    {units[value]}
                </button>
                <ul className="dropdown-menu dropdown-menu-end">
                    {availableUnitsPerPeriod[period].map(key => {
                        return (
                            <li key={key}>
                                <button className={'dropdown-item' + (key == value ? ' active' : '')} onClick={() => onChanged(key)}>
                                    {units[key]}
                                </button>
                            </li>
                        );
                    })}
                </ul>
            </div>
        </div>
    );
}