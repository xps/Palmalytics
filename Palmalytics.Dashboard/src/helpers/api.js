const baseUrl = `${location.origin}/palmalytics/api/`;
console.log("Base URL: ", baseUrl);

export function get(url) {
    return fetch(url)
        .then(response => {
            // Check for success code
            if (!response.ok) {
                throw new Error(`URL ${url} returned status: ${response.status}`);
            }

            // Check that content type is json
            const contentType = response.headers.get("content-type");
            if (!contentType || !contentType.includes("application/json")) {
                throw new Error(`URL ${url} returned content type: ${contentType}, expected application/json`);
            }

            // Parse the JSON response
            return response.json();
        });
}

export function buildUrl(path, parameters, filters, page) {
    var url = new URL(path, baseUrl);

    if (parameters) {
        for (var key of Object.keys(parameters))
            url.searchParams.append(key, parameters[key]);
    }

    if (filters) {
        for (var key of Object.keys(filters))
            url.searchParams.append(key, filters[key]);
    }

    if (page && page != 1) {
        url.searchParams.append("page", page);
    }

    return url.toString();
}

export function getVersion() {
    var url = buildUrl("version");
    console.log("getVersion");
    return get(url);
}

export function getChart(period, interval, property, filters) {
    var url = buildUrl("chart", { period, interval, property }, filters);
    console.log("getChart", period, interval, property, filters, url);
    return get(url);
}

export function getTopData(period, filters) {
    var url = buildUrl("top-data", { period }, filters);
    console.log("getTopData", period, filters, url);
    return get(url);
}

export function getBrowsers(period, filters, page) {
    var url = buildUrl("browsers", { period }, filters, page);
    console.log("getBrowsers", period, filters, page, url);
    return get(url);
}

export function getOperatingSystems(period, filters, page) {
    var url = buildUrl("operating-systems", { period }, filters, page);
    console.log("getOperatingSystems", period, filters, page, url);
    return get(url);
}

export function getReferrers(period, filters, page) {
    var url = buildUrl("referrers", { period }, filters, page);
    console.log("getReferrers", period, filters, page, url);
    return get(url);
}

export function getUtmParameter(parameter, period, filters, page) {
    var url = buildUrl("utm-parameters", { parameter, period }, filters, page);
    console.log("getUtmParameters", parameter, period, filters, page, url);
    return get(url);
}

export function getCountries(period, filters, page) {
    var url = buildUrl("countries", { period }, filters, page);
    console.log("getCountries", period, filters, page, url);
    return get(url);
}

export function getTopPages(period, filters, page) {
    var url = buildUrl("top-pages", { period }, filters, page);
    console.log("getTopPages", period, filters, page, url);
    return get(url);
}

export function getEntryPages(period, filters, page) {
    var url = buildUrl("entry-pages", { period }, filters, page);
    console.log("getEntryPages", period, filters, page, url);
    return get(url);
}

export function getExitPages(period, filters, page) {
    var url = buildUrl("exit-pages", { period }, filters, page);
    console.log("getExitPages", period, filters, page, url);
    return get(url);
}