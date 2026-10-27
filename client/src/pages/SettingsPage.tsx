import React, { useState } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Grid,
  TextField,
  Button,
  Chip,
  Alert,
  Divider,
} from '@mui/material';
import LockResetIcon from '@mui/icons-material/LockReset';
import AccountCircleIcon from '@mui/icons-material/AccountCircle';
import InfoIcon from '@mui/icons-material/Info';
import { useAuth } from '../context/AuthContext';
import api from '../api/client';
import { ApiResponse } from '../types';

export const SettingsPage: React.FC = () => {
  const { user } = useAuth();

  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');

  const [loading, setLoading] = useState(false);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  const handlePasswordChange = async (e: React.FormEvent) => {
    e.preventDefault();
    setSuccessMsg(null);
    setErrorMsg(null);

    if (newPassword !== confirmPassword) {
      setErrorMsg('New password and confirmation do not match.');
      return;
    }

    if (newPassword.length < 6) {
      setErrorMsg('Password must be at least 6 characters long.');
      return;
    }

    try {
      setLoading(true);
      const res = await api.post<ApiResponse<string>>('/auth/change-password', {
        currentPassword,
        newPassword,
      });

      if (res.data?.success) {
        setSuccessMsg('Your password has been updated successfully.');
        setCurrentPassword('');
        setNewPassword('');
        setConfirmPassword('');
      } else {
        setErrorMsg(res.data?.message || 'Password update failed.');
      }
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Failed to change password.';
      setErrorMsg(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      <Box>
        <Typography variant="h5" sx={{ fontWeight: 700, color: '#0f172a' }}>
          Account & Security Settings
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Manage your security credentials, inspect active committee roles, and view system specifications.
        </Typography>
      </Box>

      <Grid container spacing={3}>
        {/* User Profile Card */}
        <Grid size={{ xs: 12, md: 5 }}>
          <Card sx={{ height: '100%' }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mb: 2 }}>
                <AccountCircleIcon sx={{ fontSize: 32, color: '#1e3a8a' }} />
                <Typography variant="h6" sx={{ fontWeight: 700 }}>
                  User Profile
                </Typography>
              </Box>

              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
                <Box>
                  <Typography variant="caption" color="text.secondary">Full Name</Typography>
                  <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>{user?.fullName}</Typography>
                </Box>
                <Box>
                  <Typography variant="caption" color="text.secondary">Email Address</Typography>
                  <Typography variant="body2">{user?.email}</Typography>
                </Box>
                <Box>
                  <Typography variant="caption" color="text.secondary">Mobile Phone</Typography>
                  <Typography variant="body2">{user?.phoneNumber || 'N/A'}</Typography>
                </Box>
                <Box>
                  <Typography variant="caption" color="text.secondary">Assigned Roles</Typography>
                  <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap', mt: 0.5 }}>
                    {user?.roles?.map((role) => (
                      <Chip key={role} label={role} color="primary" size="small" sx={{ fontWeight: 600 }} />
                    ))}
                  </Box>
                </Box>
                <Box>
                  <Typography variant="caption" color="text.secondary">Last Login (UTC)</Typography>
                  <Typography variant="body2">{user?.lastLoginAtUtc ? new Date(user.lastLoginAtUtc).toLocaleString() : 'N/A'}</Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* Change Password Card */}
        <Grid size={{ xs: 12, md: 7 }}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mb: 2 }}>
                <LockResetIcon sx={{ fontSize: 32, color: '#d97706' }} />
                <Typography variant="h6" sx={{ fontWeight: 700 }}>
                  Change Password
                </Typography>
              </Box>

              {successMsg && <Alert severity="success" sx={{ mb: 2 }}>{successMsg}</Alert>}
              {errorMsg && <Alert severity="error" sx={{ mb: 2 }}>{errorMsg}</Alert>}

              <Box component="form" onSubmit={handlePasswordChange} sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                <TextField
                  label="Current Password"
                  type="password"
                  required
                  size="small"
                  value={currentPassword}
                  onChange={(e) => setCurrentPassword(e.target.value)}
                />
                <TextField
                  label="New Password"
                  type="password"
                  required
                  size="small"
                  value={newPassword}
                  onChange={(e) => setNewPassword(e.target.value)}
                />
                <TextField
                  label="Confirm New Password"
                  type="password"
                  required
                  size="small"
                  value={confirmPassword}
                  onChange={(e) => setConfirmPassword(e.target.value)}
                />
                <Button
                  type="submit"
                  variant="contained"
                  disabled={loading}
                  sx={{ alignSelf: 'flex-start', bgcolor: '#1e3a8a', mt: 1 }}
                >
                  {loading ? 'Updating...' : 'Update Password'}
                </Button>
              </Box>
            </CardContent>
          </Card>

          {/* Architecture & Tech Stack Card */}
          <Card sx={{ mt: 3 }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mb: 1.5 }}>
                <InfoIcon sx={{ fontSize: 24, color: '#0d9488' }} />
                <Typography variant="subtitle1" sx={{ fontWeight: 700 }}>
                  Deployment & Architecture Specifications
                </Typography>
              </Box>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 1.5 }}>
                Apartment Maintenance Management System is architected under Clean Architecture principles to support 5,000+ concurrent registered users.
              </Typography>
              <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                <Chip label="Backend: ASP.NET Core 10 LTS Web API" size="small" variant="outlined" />
                <Chip label="Persistence: MySQL 8.4+ / EF Core 10" size="small" variant="outlined" />
                <Chip label="Frontend: React 18 + TS + MUI" size="small" variant="outlined" />
                <Chip label="Security: JWT Bearer + BCrypt (Cost 11)" size="small" variant="outlined" />
                <Chip label="Logging: Serilog Structured Logging" size="small" variant="outlined" />
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
};
