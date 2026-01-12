import { createRoot } from 'react-dom/client'
import App from './App.tsx'
import './index.css'
import React from 'react';
import { setupGlobalErrorHandlers } from './utils/errorHandlers';
import { setupFetchInterceptor } from './utils/fetchInterceptor';

// Set up global error handlers and fetch interceptor (development only)
if (import.meta.env.MODE === 'development') {
  setupGlobalErrorHandlers();
  setupFetchInterceptor();
}

createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
