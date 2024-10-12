import { useState, useEffect, useRef } from 'react';
import ReportFooter from './ReportFooter';
import Pager from './Pager';
import Spinner from './Spinner';
import tableBase from './TableBase';
import * as api from '../helpers/api';
import * as numbers from '../helpers/numbers';

function capitalize(str) {
    return str.length ? str.charAt(0).toUpperCase() + str.slice(1) : str;
}

export default function UtmTable({ parameter, period, filters, onAddFilter }) {
    var { data, page, loading, responseTime, changePage } = tableBase({ 
        period,
        filters,
        apiCall: () => api.getUtmParameter(parameter, period, filters, page)
    });

    return (
        <div id={ `utm-${parameter}-table` } className="table-container">
            { !loading ?
                <>
                    <table className="table table-striped">
                        <thead>
                            <tr>
                                <th width="52">#</th>
                                <th>
                                    UTM {capitalize(parameter)}
                                </th>
                                <th width="100" className="text-end">Sessions</th>
                                <th width="100" className="text-end">%</th>
                            </tr>
                        </thead>
                        <tbody>
                            {data.rows.map((row, index) => {
                                return <tr key={row.label}>
                                    <td className="rank">
                                        {(page - 1) * 10 + index + 1}
                                    </td>
                                    <td>
                                        <a href="#" onClick={e => { e.preventDefault(); onAddFilter({ ['utm' + capitalize(parameter)]: row.label }) }}>
                                            {row.label}
                                        </a>
                                    </td>
                                    <td className="text-end" title={numbers.commas(row.value)}>
                                        {numbers.compact(row.value)}
                                    </td>
                                    <td className="text-end">
                                        <span title={row.percentage.toFixed(3) + '%'}>{numbers.percent(row.percentage)}</span>
                                    </td>
                                </tr>
                            })}
                        </tbody>
                    </table>
                    <Pager page={page} pageCount={data.pageCount} onPageChanged={changePage} />
                    <ReportFooter samplingFactor={data.samplingFactor} responseTime={responseTime} />
                </> :
                <Spinner />
            }
        </div>
    );
}