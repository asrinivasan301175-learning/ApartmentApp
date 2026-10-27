import React, { useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  TextField,
  Button,
  Alert,
  CircularProgress,
  Chip,
  Divider,
  InputAdornment,
  IconButton,
} from '@mui/material';
import Visibility from '@mui/icons-material/Visibility';
import VisibilityOff from '@mui/icons-material/VisibilityOff';
import ApartmentIcon from '@mui/icons-material/Apartment';
import LockOutlinedIcon from '@mui/icons-material/LockOutlined';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';

export const Login: React.FC = () => {
  const { login } = useAuth();
  const navigate = useNavigate();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const demoAccounts = [
    { label: 'Admin', email: 'admin@apartment.com', pwd: 'Admin@123456', color: '#1e3a8a' },
    { label: 'President', email: 'president@apartment.com', pwd: 'President@123456', color: '#7c3aed' },
    { label: 'Secretary', email: 'secretary@apartment.com', pwd: 'Secretary@123456', color: '#0284c7' },
    { label: 'Treasurer', email: 'treasurer@apartment.com', pwd: 'Treasurer@123456', color: '#059669' },
    { label: 'Staff', email: 'staff@apartment.com', pwd: 'Staff@123456', color: '#d97706' },
    { label: 'Resident', email: 'resident@apartment.com', pwd: 'Resident@123456', color: '#e11d48' },
  ];

  const handleFillDemo = (acc: typeof demoAccounts[0]) => {
    setEmail(acc.email);
    setPassword(acc.pwd);
    setError(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!email || !password) {
      setError('Please provide email and password.');
      return;
    }

    try {
      setLoading(true);
      await login(email, password);
      navigate('/dashboard');
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Invalid credentials. Please verify your email and password.';
      setError(message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        bgcolor: '#f1f5f9',
        p: 2,
      }}
    >
      <Card sx={{ maxWidth: 460, width: '100%', p: { xs: 2, sm: 3 }, boxShadow: '0 10px 25px -5px rgba(0,0,0,0.1)' }}>
        <CardContent sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
          <Box
            sx={{
              width: 56,
              height: 56,
              borderRadius: '50%',
              bgcolor: '#1e3a8a',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: '#ffffff',
              mb: 1.5,
            }}
          >
            <ApartmentIcon sx={{ fontSize: 32 }} />
          </Box>

          <Typography variant="h5" component="h1" sx={{ fontWeight: 700, color: '#1e3a8a' }}>
            AptMaintenance Pro
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
            Apartment Maintenance & Committee Workflow System
          </Typography>

          {error && (
            <Alert severity="error" sx={{ width: '100%', mb: 2.5 }}>
              {error}
            </Alert>
          )}

          <Box component="form" onSubmit={handleSubmit} sx={{ width: '100%' }}>
            <TextField
              label="Email Address"
              fullWidth
              size="medium"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              sx={{ mb: 2 }}
              required
              autoFocus
            />

            <TextField
              label="Password"
              fullWidth
              size="medium"
              type={showPassword ? 'text' : 'password'}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              sx={{ mb: 3 }}
              required
              slotProps={{
                input: {
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton onClick={() => setShowPassword(!showPassword)} edge="end">
                        {showPassword ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  ),
                },
              }}
            />

            <Button
              type="submit"
              variant="contained"
              fullWidth
              size="large"
              disabled={loading}
              startIcon={loading ? <CircularProgress size={20} color="inherit" /> : <LockOutlinedIcon />}
              sx={{ bgcolor: '#1e3a8a', py: 1.2, fontSize: '1rem', mb: 2 }}
            >
              {loading ? 'Authenticating...' : 'Sign In'}
            </Button>
          </Box>

          <Divider sx={{ width: '100%', my: 2 }}>
            <Typography variant="caption" color="text.secondary">
              ONE-CLICK DEMO PERSONAS
            </Typography>
          </Divider>

          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, justifyContent: 'center' }}>
            {demoAccounts.map((acc) => (
              <Chip
                key={acc.label}
                label={acc.label}
                onClick={() => handleFillDemo(acc)}
                sx={{
                  bgcolor: acc.color,
                  color: '#ffffff',
                  fontWeight: 600,
                  fontSize: '0.75rem',
                  cursor: 'pointer',
                  '&:hover': { opacity: 0.9 },
                }}
              />
            ))}
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
};
