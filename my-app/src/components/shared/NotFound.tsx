//ai default not found page
import React from 'react';

const NotFound: React.FC = () => (
    <div style={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        height: '100vh',
        textAlign: 'center'
    }}>
        <h1>404</h1>
        <p>Sorry, the page you are looking for does not exist.</p>
    </div>
);

export default NotFound;