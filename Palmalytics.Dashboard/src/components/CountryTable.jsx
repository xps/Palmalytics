import { useState, useEffect, useRef } from 'react';
import ReportFooter from './ReportFooter';
import Pager from './Pager';
import Spinner from './Spinner';
import tableBase from './TableBase';
import * as api from '../helpers/api';
import * as numbers from '../helpers/numbers';
import * as countries from '../helpers/countries';

export default function CountryTable({ period, filters, onAddFilter }) {
    var { data, page, loading, responseTime, changePage } = tableBase({ 
        period,
        filters,
        apiCall: () => api.getCountries(period, filters, page)
    });

    return (
        <div id="country-table" className="table-container">
            { !loading ?
                <>
                    <div className="attribution">
                        <span>IP Geolocation from </span>
                        <a href="https://db-ip.com" rel="noreferrer" target="_blank">DB-IP</a>
                    </div>
                    <table className="table table-striped">
                        <thead>
                            <tr>
                                <th width="52">#</th>
                                <th>Country</th>
                                <th width="50"></th>
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
                                        <a href="#" onClick={e => { e.preventDefault(); onAddFilter({ country: row.label }) }}>
                                            {countries.getCountryName(row.label)}
                                        </a>
                                    </td>
                                    <td className="text-center">
                                        {countries.getCountryFlag(row.label)}
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