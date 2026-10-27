import React, { useEffect, useState } from 'react';
import {
  Box,
  Grid,
  Card,
  CardContent,
  Typography,
  LinearProgress,
  Chip,
  Button,
  Alert,
  CircularProgress,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Divider,
} from '@mui/material';
import MeetingRoomIcon from '@mui/icons-material/MeetingRoom';
import LocalParkingIcon from '@mui/icons-material/LocalParking';
import HandshakeIcon from '@mui/icons-material/Handshake';
import BuildIcon from '@mui/icons-material/Build';
import PendingActionsIcon from '@mui/icons-material/PendingActions';
import CurrencyRupeeIcon from '@mui/icons-material/CurrencyRupee';
import ArrowForwardIcon from '@mui/icons-material/ArrowForward';
import HistoryIcon from '@mui/icons-material/History';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import WarningAmberIcon from '@mui/icons-material/WarningAmber';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import api from '../api/client';
import { ApiResponse, DashboardMetrics } from '../types';

export const Dashboard: React.FC = () => {
  const { user, isCommitteeMember } = useAuth();
  const navigate = useNavigate();
  const [metrics, setMetrics] = useState<DashboardMetrics | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchMetrics = async () => {
      try {
        const res = await api.get<ApiResponse<DashboardMetrics>>('/dashboard/metrics');
        if (res.data?.success && res.data.data) {
          setMetrics(res.data.data);
        }
      } catch (err) {
        console.error('Failed to load metrics', err);
      } finally {
        setLoading(false);
      }
    };

    fetchMetrics();
  }, []);

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '60vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  const statCards = [
    {
      title: 'Flats Occupancy',
      value: `${metrics?.occupiedFlats || 0} / ${metrics?.totalFlats || 0}`,
      subtitle: `${metrics?.occupancyRatePercentage || 0}% Occupancy (${metrics?.vacantFlats || 0} Vacant)`,
      icon: <MeetingRoomIcon sx={{ fontSize: 32, color: '#1e3a8a' }} />,
      color: '#eff6ff',
      link: '/flats',
    },
    {
      title: 'Parking Allocations',
      value: `${metrics?.allocatedParkingLots || 0} / ${metrics?.totalParkingLots || 0}`,
      subtitle: `${metrics?.availableParkingLots || 0} Parking slots available`,
      icon: <LocalParkingIcon sx={{ fontSize: 32, color: '#0d9488' }} />,
      color: '#f0fdfa',
      link: '/parking',
    },
    {
      title: 'Active Vendors & AMC',
      value: `${metrics?.activeVendors || 0}`,
      subtitle: `${metrics?.contractsExpiringIn30Days || 0} Contracts expiring in 30 days`,
      icon: <HandshakeIcon sx={{ fontSize: 32, color: '#0284c7' }} />,
      color: '#f0f9ff',
      link: '/vendors',
    },
    {
      title: 'Assets & Equipment',
      value: `${metrics?.totalAssets || 0}`,
      subtitle: `${metrics?.assetsDueForMaintenance || 0} Assets due for inspection`,
      icon: <BuildIcon sx={{ fontSize: 32, color: '#d97706' }} />,
      color: '#fffbeb',
      link: '/assets',
    },
    {
      title: 'Pending Purchase Approvals',
      value: `${metrics?.pendingPurchaseRequests || 0}`,
      subtitle: `${metrics?.myPendingApprovalsCount || 0} Awaiting your decision`,
      icon: <PendingActionsIcon sx={{ fontSize: 32, color: '#7c3aed' }} />,
      color: '#f5f3ff',
      link: '/approval-queue',
    },
    {
      title: 'Approved Spend This Month',
      value: `₹${(metrics?.totalApprovedSpendThisMonth || 0).toLocaleString('en-IN')}`,
      subtitle: `${metrics?.approvedPurchaseOrdersThisMonth || 0} Purchase orders issued`,
      icon: <CurrencyRupeeIcon sx={{ fontSize: 32, color: '#16a34a' }} />,
      color: '#f0fdf4',
      link: '/purchase-orders',
    },
  ];

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      {/* Welcome Banner */}
      <Card sx={{ bgcolor: '#1e3a8a', color: '#ffffff', p: 1 }}>
        <CardContent sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 2 }}>
          <Box>
            <Typography variant="h5" sx={{ fontWeight: 700, mb: 0.5 }}>
              Welcome back, {user?.fullName}!
            </Typography>
            <Typography variant="body2" sx={{ opacity: 0.85 }}>
              Apartment Operations Portal • Signed in as{' '}
              <strong>{user?.roles?.join(', ') || 'Authorized User'}</strong>
            </Typography>
          </Box>
          <Box sx={{ display: 'flex', gap: 1.5 }}>
            <Button
              variant="contained"
              size="small"
              onClick={() => navigate('/purchase-requests')}
              sx={{ bgcolor: '#38bdf8', color: '#0f172a', fontWeight: 600, '&:hover': { bgcolor: '#7dd3fc' } }}
            >
              + Create Purchase Request
            </Button>
            {isCommitteeMember && (
              <Button
                variant="outlined"
                size="small"
                onClick={() => navigate('/approval-queue')}
                sx={{ borderColor: '#ffffff', color: '#ffffff', fontWeight: 600 }}
              >
                Review Approval Queue
              </Button>
            )}
          </Box>
        </CardContent>
      </Card>

      {/* Action alert for committee members if pending approvals exist */}
      {isCommitteeMember && (metrics?.myPendingApprovalsCount || 0) > 0 && (
        <Alert
          severity="warning"
          icon={<WarningAmberIcon fontSize="inherit" />}
          action={
            <Button color="inherit" size="small" onClick={() => navigate('/approval-queue')} endIcon={<ArrowForwardIcon />}>
              Open Queue
            </Button>
          }
        >
          You have <strong>{metrics?.myPendingApprovalsCount}</strong> purchase request(s) awaiting your committee approval!
        </Alert>
      )}

      {/* Metric Cards Grid */}
      <Grid container spacing={2.5}>
        {statCards.map((card, idx) => (
          <Grid size={{ xs: 12, sm: 6, md: 4 }} key={idx}>
            <Card
              sx={{
                height: '100%',
                display: 'flex',
                flexDirection: 'column',
                justifyContent: 'space-between',
                transition: 'transform 0.2s',
                '&:hover': { transform: 'translateY(-2px)' },
              }}
            >
              <CardContent>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
                  <Typography variant="subtitle2" color="text.secondary" sx={{ fontWeight: 600 }}>
                    {card.title}
                  </Typography>
                  <Box sx={{ p: 1, borderRadius: 2, bgcolor: card.color }}>
                    {card.icon}
                  </Box>
                </Box>
                <Typography variant="h4" sx={{ fontWeight: 700, mb: 1, color: '#0f172a' }}>
                  {card.value}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {card.subtitle}
                </Typography>
              </CardContent>
              <Box sx={{ p: 1.5, pt: 0, display: 'flex', justifyContent: 'flex-end' }}>
                <Button size="small" onClick={() => navigate(card.link)} endIcon={<ArrowForwardIcon fontSize="small" />}>
                  View Details
                </Button>
              </Box>
            </Card>
          </Grid>
        ))}
      </Grid>

      {/* Secondary Dashboard Sections: Occupancy & Recent Activity */}
      <Grid container spacing={3}>
        {/* Occupancy and Capacity Progress */}
        <Grid size={{ xs: 12, md: 5 }}>
          <Card sx={{ height: '100%', p: 2 }}>
            <CardContent>
              <Typography variant="h6" sx={{ fontWeight: 600, mb: 2 }}>
                Estate Capacity Status
              </Typography>

              <Box sx={{ mb: 3 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                  <Typography variant="body2" sx={{ fontWeight: 500 }}>
                    Residential Flat Occupancy
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {metrics?.occupiedFlats} / {metrics?.totalFlats} ({metrics?.occupancyRatePercentage}%)
                  </Typography>
                </Box>
                <LinearProgress
                  variant="determinate"
                  value={metrics?.occupancyRatePercentage || 0}
                  sx={{ height: 8, borderRadius: 4, bgcolor: '#e2e8f0', '& .MuiLinearProgress-bar': { bgcolor: '#1e3a8a' } }}
                />
              </Box>

              <Box sx={{ mb: 3 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                  <Typography variant="body2" sx={{ fontWeight: 500 }}>
                    Car Parking Utilization
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {metrics?.allocatedParkingLots} / {metrics?.totalParkingLots} (
                    {metrics?.totalParkingLots ? Math.round((metrics.allocatedParkingLots / metrics.totalParkingLots) * 100) : 0}%)
                  </Typography>
                </Box>
                <LinearProgress
                  variant="determinate"
                  value={metrics?.totalParkingLots ? (metrics.allocatedParkingLots / metrics.totalParkingLots) * 100 : 0}
                  sx={{ height: 8, borderRadius: 4, bgcolor: '#e2e8f0', '& .MuiLinearProgress-bar': { bgcolor: '#0d9488' } }}
                />
              </Box>

              <Divider sx={{ my: 2 }} />

              <Box sx={{ display: 'flex', justifyContent: 'space-around', textAlign: 'center' }}>
                <Box>
                  <Typography variant="h5" sx={{ fontWeight: 700, color: '#1e3a8a' }}>
                    {metrics?.totalTowers || 0}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    Active Towers
                  </Typography>
                </Box>
                <Box>
                  <Typography variant="h5" sx={{ fontWeight: 700, color: '#0d9488' }}>
                    {metrics?.totalResidents || 0}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    Registered Residents
                  </Typography>
                </Box>
                <Box>
                  <Typography variant="h5" sx={{ fontWeight: 700, color: '#d97706' }}>
                    {metrics?.totalEmployees || 0}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    Maintenance Staff
                  </Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* Recent Society Activity Feed */}
        <Grid size={{ xs: 12, md: 7 }}>
          <Card sx={{ height: '100%', p: 2 }}>
            <CardContent>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                <Typography variant="h6" sx={{ fontWeight: 600 }}>
                  Recent Society Activity & Audit
                </Typography>
                <HistoryIcon color="action" />
              </Box>

              {metrics?.recentActivities && metrics.recentActivities.length > 0 ? (
                <List disablePadding>
                  {metrics.recentActivities.map((act, index) => (
                    <React.Fragment key={index}>
                      <ListItem alignItems="flex-start" sx={{ px: 0, py: 1 }}>
                        <ListItemIcon sx={{ minWidth: 36, mt: 0.5 }}>
                          <CheckCircleIcon color="success" fontSize="small" />
                        </ListItemIcon>
                        <ListItemText
                          primary={
                            <Typography variant="body2" sx={{ fontWeight: 600 }}>
                              {act.title}
                            </Typography>
                          }
                          secondary={
                            <Typography variant="caption" color="text.secondary" component="span">
                              {act.description} • {new Date(act.timestampUtc).toLocaleDateString()}
                            </Typography>
                          }
                        />
                        <Chip label={act.type} size="small" variant="outlined" sx={{ fontSize: '0.65rem' }} />
                      </ListItem>
                      {index < metrics.recentActivities.length - 1 && <Divider component="li" />}
                    </React.Fragment>
                  ))}
                </List>
              ) : (
                <Typography variant="body2" color="text.secondary" sx={{ py: 3, textAlign: 'center' }}>
                  No recent activities recorded.
                </Typography>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
};
