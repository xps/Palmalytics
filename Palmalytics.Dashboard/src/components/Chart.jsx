import '../styles/components/chart.css';
import { useState, useEffect } from 'react';
import Spinner from './Spinner';
import ApexChart from 'react-apexcharts';
import ReportFooter from './ReportFooter';
import IntervalSelector from './IntervalSelector';
import * as api from '../helpers/api';
import * as dates from '../helpers/dates';
import * as numbers from '../helpers/numbers';
import * as durations from '../helpers/durations';

const availableIntervalsPerPeriod = {
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

function formatAxisValue(property, value) {
    if (property == 'Bounce Rate')
        return numbers.percent(value);
    if (property == 'Session Duration')
        return durations.format(value);
    if (property == 'Pages / Session')
        return value.toFixed(1);
    return numbers.commas(value);
}

function formatAxisDate(interval, value) {
    if (interval == 'days')
        return dates.format(value, "d MMM yy");
    if (interval == 'weeks')
        return dates.format(value, "d MMM yy");
    if (interval == 'months')
        return dates.format(value, "MMM yyyy");
    if (interval == 'years')
        return dates.format(value, "yyyy");
    return value;
}

function formatTooltipDate(interval, value) {
    if (interval == 'days')
        return dates.format(value, "ddd d MMM yyyy");
    if (interval == 'weeks')
        return 'Week of ' + dates.format(value, "d MMM yyyy");
    if (interval == 'months')
        return dates.format(value, 'MMMM yyyy');
    if (interval == 'years')
        return dates.format(value, "yyyy");
    return value;
}

function formatTooltipValue(property, value) {
    if (property == 'Bounce Rate')
        return numbers.percent(value);
    if (property == 'Session Duration')
        return durations.format(value);
    if (property == 'Pages / Session')
        return value.toFixed(1);
    return numbers.commas(value);
}

export default function Chart({ period, filters }) {
    const [interval, setInterval] = useState('months');
    const [property, setProperty] = useState('Sessions');

    const [topData, setTopData] = useState(null);
    const [chartData, setChartData] = useState(null);
    const [responseTime, setResponseTime] = useState(0);

    useEffect(() => {
        if (!availableIntervalsPerPeriod[period].includes(interval)) {
            setInterval(availableIntervalsPerPeriod[period][0]);
        } else {
            const time = new Date().getTime();
            api.getChart(period, interval, property, filters).then(chartData => {
                setChartData(chartData);
                setResponseTime(new Date().getTime() - time);
            });
        }
    }, [period, interval, property, filters]);

    useEffect(() => {
        const time = new Date().getTime();
        api.getTopData(period, filters).then(topData => {
            setTopData(topData);
            setResponseTime(new Date().getTime() - time);
        });
    }, [period, filters]);

    if (!topData || !chartData) {
        return (
            <div id="chart" className="block shadow loading">
                <Spinner />
            </div>
        );
    }

    const chartOptions = {
        chart: {
            type: 'line',
            toolbar: {
                show: false
            },
            offsetX: 0
        },
        series: [{
            name: property,
            data: chartData.data.map(x => x.value)
        }],
        yaxis: {
            min: 0,
            labels: {
                formatter: function (value) {
                    return formatAxisValue(property, value);
                }
            }
        },
        xaxis: {
            categories: chartData.data.map(x => x.date),
            //tickAmount: Math.min(52, chartData.data.length),
            labels: {
                formatter: function (value) {
                    if (interval == 'days') {
                        if (chartData.data.length > 180) {
                            const d = new Date(value);
                            if (d.getDate() != 1)
                                return '';
                        }
                    }
                    return formatAxisDate(interval, value);
                }
            }
        },
        dataLabels: {
            enabled: false,
            formatter: function(value) {
                return numbers.compact(value);
            },
            style: {
                fontSize: '12px',
                colors: ["#304758"]
            }
        },
        tooltip: {
            enabled: true,
            custom: ({ dataPointIndex }) => {
                const dataPoint = chartData.data[dataPointIndex];
                return `<div class="chart-tooltip shadow">
                            <div class="tooltip-title">${formatTooltipDate(interval, dataPoint.date)}</div>
                            <div>${property}: ${formatTooltipValue(property, dataPoint.value)}</div>
                        </div>`;
            }
        }
    };

    return (
        <div id="chart" className="block shadow">
            <div id="dates">
                <div>{dates.format(chartData.dateFrom)} to {dates.format(chartData.dateTo)}</div>
                <div><small>{numbers.commas(chartData.totalDays)} Days  • {numbers.commas(chartOptions.series[0].data.length)} Data Points</small></div>
                <IntervalSelector value={interval} intervals={availableIntervalsPerPeriod[period]} onChanged={setInterval} />
            </div>
            <div id="numbers">
                <button className={'number ' + (property == 'Sessions' ? 'active' : '')} onClick={() => setProperty('Sessions')} disabled={property == "Sessions"}>
                    <div className="value" title={numbers.commas(topData.totalSessions)}>
                        {numbers.compact(topData.totalSessions)}
                    </div>
                    <div className="label">Sessions</div>
                </button>
                <div className="separator"></div>
                <button className={'number ' + (property == 'Page Views' ? 'active' : '')} onClick={() => setProperty('Page Views')} disabled={property == "Page Views"}>
                    <div className="value" title={numbers.commas(topData.totalPageViews)}>
                        {numbers.compact(topData.totalPageViews)}
                    </div>
                    <div className="label">Page Views</div>
                </button>
                <div className="separator"></div>
                <button className={'number ' + (property == 'Bounce Rate' ? 'active' : '')} onClick={() => setProperty('Bounce Rate')} disabled={property == "Bounce Rate"}>
                    <div className="value" title={numbers.commas(topData.averageBounceRate)}>
                        {numbers.compact(topData.averageBounceRate)}%
                    </div>
                    <div className="label">Bounce Rate</div>
                </button>
                <div className="separator"></div>
                <button className={'number ' + (property == 'Session Duration' ? 'active' : '')} onClick={() => setProperty('Session Duration')} disabled={property == "Session Duration"}>
                    <div className="value" title={numbers.commas(topData.averageSessionDuration)}>
                        {durations.format(topData.averageSessionDuration)}
                    </div>
                    <div className="label">Session Duration</div>
                </button>
                <div className="separator"></div>
                <button className={'number ' + (property == 'Pages / Session' ? 'active' : '')} onClick={() => setProperty('Pages / Session')} disabled={property == "Pages / Session"}>
                    <div className="value" title={numbers.commas(topData.averageSessionDuration)}>
                        {topData.averagePagesPerSession.toFixed(1)}
                    </div>
                    <div className="label">Pages / Session</div>
                </button>
            </div>
            <ApexChart options={chartOptions} series={chartOptions.series} type="area" height={350} />
            <ReportFooter samplingFactor={chartData.samplingFactor} responseTime={responseTime} rowCount={chartData.totalPageViews} />
        </div>
    );
}