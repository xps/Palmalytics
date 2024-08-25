import '../styles/components/report-footer.css';
import * as numbers from '../helpers/numbers';

export default function ReportFooter({ samplingFactor, responseTime, rowCount }) {
    return (
        <div className="report-footer">
            <span className="sampling">
                { samplingFactor == 1 ?
                    `This report is based on 100% of your data (no sampling appplied).` :
                    `This report is estimated based on a ${numbers.percent(100 / samplingFactor)} sample of your data.` }
            </span>
            <span className="response-time">
                {numbers.commas(responseTime)}ms { rowCount && ` for ${numbers.compact(rowCount / samplingFactor)} rows` }
            </span>
        </div>
    );
}