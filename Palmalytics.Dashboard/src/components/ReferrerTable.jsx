import { useState, useEffect, useRef } from 'react';
import ReportFooter from './ReportFooter';
import Pager from './Pager';
import Spinner from './Spinner';
import tableBase from './TableBase';
import * as api from '../helpers/api';
import * as numbers from '../helpers/numbers';

export default function ReferrerTable({ period, filters, onAddFilter }) {
    var { data, page, loading, responseTime, changePage } = tableBase({ 
        period,
        filters,
        apiCall: () => api.getReferrers(period, filters, page)
    });

    return (
        <div id="referrer-table" className="table-container">
            { !loading ?
                <>
                    <table className="table table-striped">
                        <thead>
                            <tr>
                                <th width="52">#</th>
                                <th>
                                    { !filters.referrer ?
                                        "Referrer" :
                                        "Referrer URL" }
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
                                        { !filters.referrer ?
                                            <a href="#" onClick={e => { e.preventDefault(); onAddFilter({ referrer: row.label }) }}>
                                                {row.label}
                                            </a> :
                                            <a href="#" onClick={e => { e.preventDefault(); onAddFilter({ referrerUrl: row.label }) }}>
                                                {row.label}
                                            </a>
                                        }
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