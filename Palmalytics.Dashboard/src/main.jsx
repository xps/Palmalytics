import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App.jsx';
import './styles/site.css';
import './styles/shared/blocks.css';
import './styles/shared/tables.css';

const root = document.getElementById('root');

ReactDOM.createRoot(root).render(
    <React.StrictMode>
        <App />
    </React.StrictMode>
);
