import { useState } from 'react';
import { Tabs, Tab } from './components/Tabs';
import Header from './components/Header';
import Filters from './components/Filters';
import Chart from './components/Chart';
import ReferrerTable from './components/ReferrerTable';
import UtmTable from './components/UtmTable';
import CountryTable from './components/CountryTable';
import PagesTable from './components/PagesTable';
import BrowserTable from './components/BrowserTable';
import OSTable from './components/OSTable';
import Footer from './components/Footer';
import * as api from './helpers/api';

function App() {
    const [period, setPeriod] = useState('last-12-months');
    const [filters, setFilters] = useState({});

    const handleAddFilter = function (filter) {
        setFilters({ ...filters, ...filter });
    };

    return <>
        <div className="container">
            <Header />
            <Filters period={period} filters={filters} onPeriodChanged={setPeriod} onFiltersChanged={setFilters} />
            <Chart period={period} filters={filters} />
            <Tabs height={585}>
                <Tab name="Top Pages">
                    <PagesTable key="top-pages" period={period} filters={filters} onAddFilter={handleAddFilter} filterName="path" apiFunction={api.getTopPages} />
                </Tab>
                <Tab name="Entry Pages">
                    <PagesTable key="entry-pages" period={period} filters={filters} onAddFilter={handleAddFilter} filterName="entryPath" apiFunction={api.getEntryPages} />
                </Tab>
                <Tab name="Exit Pages">
                    <PagesTable key="exit-pages" period={period} filters={filters} onAddFilter={handleAddFilter} filterName="exitPath" apiFunction={api.getExitPages} />
                </Tab>
            </Tabs>
            <Tabs height={585}>
                <Tab name="Referrers">
                    <ReferrerTable period={period} filters={filters} onAddFilter={handleAddFilter} />
                </Tab>
                <Tab name="UTM Sources">
                    <UtmTable parameter="source" period={period} filters={filters} onAddFilter={handleAddFilter} />
                </Tab>
                <Tab name="UTM Media">
                    <UtmTable parameter="medium" period={period} filters={filters} onAddFilter={handleAddFilter} />
                </Tab>
                <Tab name="UTM Campaigns">
                    <UtmTable parameter="campaign" period={period} filters={filters} onAddFilter={handleAddFilter} />
                </Tab>
                <Tab name="UTM Terms">
                    <UtmTable parameter="term" period={period} filters={filters} onAddFilter={handleAddFilter} />
                </Tab>
                <Tab name="UTM Contents">
                    <UtmTable parameter="content" period={period} filters={filters} onAddFilter={handleAddFilter} />
                </Tab>
            </Tabs>
            <Tabs height={585}>
                <Tab name="Locations">
                    <CountryTable period={period} filters={filters} onAddFilter={handleAddFilter} />
                </Tab>
            </Tabs>
            <Tabs height={585}>
                <Tab name="Browsers">
                    <BrowserTable period={period} filters={filters} onAddFilter={handleAddFilter} />
                </Tab>
                <Tab name="OS">
                    <OSTable period={period} filters={filters} onAddFilter={handleAddFilter} />
                </Tab>
            </Tabs>
            <Footer />
        </div>
    </>;
}

export default App;
