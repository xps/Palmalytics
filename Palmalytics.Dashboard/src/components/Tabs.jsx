import '../styles/components/tabs.css';
import { useState, Children } from 'react';

export function Tabs({ children, height }) {
    const [activeTabIndex, setActiveTabIndex] = useState(0);
    const childrenArray = Children.toArray(children);

    return (
        <div className="tabs">
            <div className="tab-headers">
                { childrenArray.map((child, index) =>
                    <button key={index} className={ 'tab-header ' + (activeTabIndex == index ? 'active' : '')} onClick={() => setActiveTabIndex(index)} disabled={activeTabIndex == index}>
                        {child.props.name}
                    </button>
                )}
            </div>
            <div className="block tab-content shadow" style={{ minHeight: `${height}px` }}>
                {childrenArray[activeTabIndex]}
            </div>
        </div>
    );
};

export function Tab({ children }) {
    return children;
};