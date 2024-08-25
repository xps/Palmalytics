import { useState, useEffect } from 'react';
import ReportFooter from './ReportFooter';
import Pager from './Pager';
import Spinner from './Spinner';
import tableBase from './TableBase';
import * as numbers from '../helpers/numbers';

export default function PagesTable({ period, filters, apiFunction, filterName, onAddFilter }) {
    var { data, page, loading, responseTime, changePage } = tableBase({ 
        period,
        filters,
        apiCall: () => apiFunction(period, filters, page)
    });

    return (
        <div id="pages-table" className="table-container">
            { !loading ?
                <>
                    <table className="table table-striped">
                        <thead>
                            <tr>
                                <th width="52">#</th>
                                <th>Page</th>
                                <th width="100" className="text-end">Views</th>
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
                                        <a href="#" onClick={e => { e.preventDefault(); onAddFilter({ [filterName]: row.label }) }}>
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