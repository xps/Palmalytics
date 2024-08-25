// Formats a duration in hours, minutes, seconds
export function format(durationInSeconds) {
    var hours = Math.floor(durationInSeconds / 3600);
    var minutes = Math.floor((durationInSeconds % 3600) / 60);
    var seconds = durationInSeconds % 60;

    if (hours > 0)
        return hours + "h " + minutes + "m " + seconds + "s";
    else if (minutes > 0)
        return minutes + "m " + seconds + "s";
    else
        return seconds + "s";
}