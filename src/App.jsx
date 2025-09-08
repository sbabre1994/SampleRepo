import React from 'react';
import NotificationList from './components/NotificationList';
import { notificationData } from './data/notificationData';
import './App.css';

/**
 * Main App component
 * Demonstrates the NotificationList component with placeholder data
 */
function App() {
  return (
    <div className="App">
      <NotificationList notifications={notificationData} />
    </div>
  );
}

export default App;