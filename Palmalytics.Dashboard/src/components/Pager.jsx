import '../styles/components/pager.css';
import * as Feather from 'react-feather';

export default function Pager({ page, pageCount, onPageChanged }) {
    return pageCount > 0 ?
        <div className="pager">
            <span>
                Page {page} of {pageCount}
            </span>
            <span className="buttons">
                <button className="btn btn-sm btn-outline-primary" disabled={page === 1} onClick={() => onPageChanged(1)} title="First page">
                    <Feather.ChevronsLeft size={14} />
                </button>
                <button className="btn btn-sm btn-outline-primary" disabled={page === 1} onClick={() => onPageChanged(page - 1)} title="Previous page">
                    <Feather.ChevronLeft size={14} />
                </button>
                <button className="btn btn-sm btn-outline-primary" disabled={page === pageCount} onClick={() => onPageChanged(page + 1)} title="Next page">
                    <Feather.ChevronRight size={14} />
                </button>
                <button className="btn btn-sm btn-outline-primary" disabled={page === pageCount} onClick={() => onPageChanged(pageCount)} title="Last page">
                    <Feather.ChevronsRight size={14} />
                </button>
            </span>
        </div> :
        <div className="pager-no-data">
            No data
        </div>;
}