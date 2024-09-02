import '../styles/components/interval-selector.css';

const labels = {
    'days': 'Days',
    'weeks': 'Weeks',
    'months': 'Months',
    'years': 'Years'
};

export default function IntervalSelector({ value, intervals, onChanged }) {
    return (
        <div id="interval-selector">
            <div className="btn-group ms-auto">
                <button className="btn dropdown-toggle" type="button" data-bs-toggle="dropdown" aria-expanded="false">
                    {labels[value]}
                </button>
                <ul className="dropdown-menu dropdown-menu-end">
                    {intervals.map(key => {
                        return (
                            <li key={key}>
                                <button className={'dropdown-item' + (key == value ? ' active' : '')} onClick={() => onChanged(key)}>
                                    {labels[key]}
                                </button>
                            </li>
                        );
                    })}
                </ul>
            </div>
        </div>
    );
}