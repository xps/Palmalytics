import '../styles/components/period-selector.css';

const periods = {
    'today': 'Today',
    'separator-1': '---',
    'last-7-days': 'Last 7 days',
    'last-30-days': 'Last 30 days',
    'separator-2': '---',
    'last-12-months': 'Last 12 months',
    'month-to-date': 'Month to date',
    'last-month': 'Last month',
    'separator-3': '---',
    'year-to-date': 'Year to date',
    'last-year': 'Last year',
    'separator-4': '---',
    'all-time': 'All time'
};

export default function PeriodSelector({ value, onChanged }) {
    return (
        <div id="period-selector" className="btn-group ms-auto">
            <button className="btn dropdown-toggle shadow" type="button" data-bs-toggle="dropdown" aria-expanded="false">
                {periods[value]}
            </button>
            <ul className="dropdown-menu dropdown-menu-end">
                {Object.keys(periods).map((key, index) => {
                    return periods[key] != '---' ?
                        <li key={key}>
                            <button className={'dropdown-item' + (key == value ? ' active' : '')} onClick={() => onChanged(key)}>
                                {periods[key]}
                            </button>
                        </li> :
                        <li key={index}>
                            <hr className="dropdown-divider" />
                        </li>;
                })}
            </ul>
        </div>
    );
}