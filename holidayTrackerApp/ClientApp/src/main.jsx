import React from 'react'
import ReactDOM from 'react-dom/client'
import App, { AuthProvider } from './App.jsx' // AuthProvider'ý App.jsx'ten alýyoruz
import './index.css'

ReactDOM.createRoot(document.getElementById('root')).render(
    <React.StrictMode>
        <AuthProvider> {/* <-- App bileþeni AuthProvider ile sarýlmalý! */}
            <App />
        </AuthProvider>
    </React.StrictMode>,
)