import '../styles/components/filters.css';
import PeriodSelector from "./PeriodSelector";

function formatFilterName(key) {
    return key.replace(/([A-Z])/g, ' $1')
        .replace(/^./, str => str.toUpperCase())
        .replace(/\bOs\b/g, 'OS')
        .replace(/\bPath\b/g, 'Page');
}

export default function Filters({ period, filters, onPeriodChanged, onFiltersChanged }) {
    function removeFilter(key) {
        const { [key]: value, ...rest } = filters;
        onFiltersChanged(rest);
    }

    const keys = Object.keys(filters);

    return (
        <div id="filters" className="d-flex align-items-end mb-3">
            { keys.length > 0 &&
                <div>
                    {/* <div className="mb-1 fw-bold">
                        Filters:
                    </div> */}
                    <div className="d-flex flex-wrap">
                        {keys.map(key => {
                            return (
                                <div key={key} className="filter shadow">
                                    <span className="name">{formatFilterName(key)}:</span>
                                    <span className="value">{filters[key]}</span>
                                    <button onClick={() => removeFilter(key)}>
                                        &times;
                                    </button>
                                </div>
                            );
                        })}
                    </div>
                </div>
            }

            <PeriodSelector value={period} onChanged={onPeriodChanged} />
        </div>
    );
}