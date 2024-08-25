import { useState, useEffect } from 'react';

export default function ({ period, filters, apiCall }) {
    const [loading, setLoading] = useState(true);
    const [data, setData] = useState(null);
    const [page, setPage] = useState(1);
    const [responseTime, setResponseTime] = useState(0);

    // Handle page change
    const changePage = (page) => {
        setLoading(true);
        setPage(page);
    };

    // Reset page to 1 when period or filters change
    useEffect(() => {
        changePage(1);
    }, [period, filters]);

    // Reload data when parameters change
    useEffect(() => {
        const time = new Date().getTime();
        setLoading(true);
        setData(null);

        apiCall().then(data => {
            setData(data);
            setLoading(false);
            setResponseTime(new Date().getTime() - time);
        });
    }, [period, filters, page]);

    return {
        data,
        page,
        loading,
        responseTime,
        changePage
    };
}