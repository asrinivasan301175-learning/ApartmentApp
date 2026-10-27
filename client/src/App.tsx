import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { ThemeProvider, CssBaseline } from '@mui/material';
import { theme } from './theme/theme';
import { AuthProvider } from './context/AuthContext';
import { ProtectedRoute } from './components/Layout/ProtectedRoute';
import { MainLayout } from './components/Layout/MainLayout';
import { Login } from './pages/Login';
import { Dashboard } from './pages/Dashboard';
import { TowersPage } from './pages/TowersPage';
import { FlatsPage } from './pages/FlatsPage';
import { ResidentsPage } from './pages/ResidentsPage';
import { ParkingPage } from './pages/ParkingPage';
import { EmployeesPage } from './pages/EmployeesPage';
import { VendorsPage } from './pages/VendorsPage';
import { AssetsPage } from './pages/AssetsPage';
import { PurchaseRequestsPage } from './pages/PurchaseRequestsPage';
import { ApprovalQueuePage } from './pages/ApprovalQueuePage';
import { PurchaseOrdersPage } from './pages/PurchaseOrdersPage';
import { CommitteePage } from './pages/CommitteePage';
import { AuditLogsPage } from './pages/AuditLogsPage';
import { SettingsPage } from './pages/SettingsPage';

export const App: React.FC = () => {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <AuthProvider>
        <BrowserRouter>
          <Routes>
            <Route path="/login" element={<Login />} />

            {/* Protected Routes wrapped in MainLayout */}
            <Route element={<ProtectedRoute />}>
              <Route element={<MainLayout />}>
                <Route path="/" element={<Navigate to="/dashboard" replace />} />
                <Route path="/dashboard" element={<Dashboard />} />
                <Route path="/towers" element={<TowersPage />} />
                <Route path="/flats" element={<FlatsPage />} />
                <Route path="/residents" element={<ResidentsPage />} />
                <Route path="/parking" element={<ParkingPage />} />
                <Route path="/employees" element={<EmployeesPage />} />
                <Route path="/vendors" element={<VendorsPage />} />
                <Route path="/assets" element={<AssetsPage />} />
                <Route path="/purchase-requests" element={<PurchaseRequestsPage />} />
                <Route path="/purchase-orders" element={<PurchaseOrdersPage />} />
                <Route path="/committee" element={<CommitteePage />} />
                <Route path="/settings" element={<SettingsPage />} />

                {/* Committee Only Route */}
                <Route element={<ProtectedRoute requireCommittee />}>
                  <Route path="/approval-queue" element={<ApprovalQueuePage />} />
                </Route>

                {/* Admin Only Route */}
                <Route element={<ProtectedRoute roles={['System Administrator']} />}>
                  <Route path="/audit-logs" element={<AuditLogsPage />} />
                </Route>
              </Route>
            </Route>

            {/* Fallback */}
            <Route path="*" element={<Navigate to="/dashboard" replace />} />
          </Routes>
        </BrowserRouter>
      </AuthProvider>
    </ThemeProvider>
  );
};

export default App;
