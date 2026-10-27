import React from 'react';
import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { Box, CircularProgress, Typography } from '@mui/material';

interface ProtectedRouteProps {
  roles?: string[];
  requireCommittee?: boolean;
}

export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ roles, requireCommittee }) => {
  const { isAuthenticated, isLoading, hasAnyRole, isCommitteeMember } = useAuth();

  if (isLoading) {
    return (
      <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', height: '100vh', gap: 2 }}>
        <CircularProgress />
        <Typography variant="body2" color="text.secondary">Authenticating...</Typography>
      </Box>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (roles && roles.length > 0 && !hasAnyRole(roles)) {
    return <Navigate to="/dashboard" replace />;
  }

  if (requireCommittee && !isCommitteeMember) {
    return <Navigate to="/dashboard" replace />;
  }

  return <Outlet />;
};
