import React from 'react';
import {
  Drawer,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Divider,
  Box,
  Typography,
} from '@mui/material';
import DashboardIcon from '@mui/icons-material/Dashboard';
import ApartmentIcon from '@mui/icons-material/Apartment';
import MeetingRoomIcon from '@mui/icons-material/MeetingRoom';
import PeopleIcon from '@mui/icons-material/People';
import LocalParkingIcon from '@mui/icons-material/LocalParking';
import BadgeIcon from '@mui/icons-material/Badge';
import HandshakeIcon from '@mui/icons-material/Handshake';
import BuildIcon from '@mui/icons-material/Build';
import ShoppingCartIcon from '@mui/icons-material/ShoppingCart';
import FactCheckIcon from '@mui/icons-material/FactCheck';
import ReceiptLongIcon from '@mui/icons-material/ReceiptLong';
import SecurityIcon from '@mui/icons-material/Security';
import SettingsIcon from '@mui/icons-material/Settings';
import HowToVoteIcon from '@mui/icons-material/HowToVote';
import { useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';

interface SidebarProps {
  open: boolean;
  onClose: () => void;
  variant: 'permanent' | 'temporary';
}

export const Sidebar: React.FC<SidebarProps> = ({ open, onClose, variant }) => {
  const navigate = useNavigate();
  const location = useLocation();
  const { hasRole, isCommitteeMember } = useAuth();

  const isSelected = (path: string) => location.pathname === path;

  const handleNavigate = (path: string) => {
    navigate(path);
    if (variant === 'temporary') {
      onClose();
    }
  };

  const navItems = [
    { label: 'Dashboard', path: '/dashboard', icon: <DashboardIcon />, visible: true },
    
    // Society Infrastructure
    { label: 'Towers', path: '/towers', icon: <ApartmentIcon />, visible: true },
    { label: 'Flats Directory', path: '/flats', icon: <MeetingRoomIcon />, visible: true },
    { label: 'Residents & Owners', path: '/residents', icon: <PeopleIcon />, visible: true },
    { label: 'Parking Lots', path: '/parking', icon: <LocalParkingIcon />, visible: true },

    // Operations
    { label: 'Employees', path: '/employees', icon: <BadgeIcon />, visible: !hasRole('Resident / Owner') },
    { label: 'Vendors & AMC', path: '/vendors', icon: <HandshakeIcon />, visible: !hasRole('Resident / Owner') },
    { label: 'Assets & Repairs', path: '/assets', icon: <BuildIcon />, visible: true },

    // Financial & Approval Workflows
    { label: 'Purchase Requests', path: '/purchase-requests', icon: <ShoppingCartIcon />, visible: true },
    { label: 'Approval Queue', path: '/approval-queue', icon: <FactCheckIcon />, visible: isCommitteeMember },
    { label: 'Purchase Orders', path: '/purchase-orders', icon: <ReceiptLongIcon />, visible: isCommitteeMember || hasRole('Maintenance Staff') },
    { label: 'Committee Members', path: '/committee', icon: <HowToVoteIcon />, visible: true },

    // Administration
    { label: 'Audit Logs', path: '/audit-logs', icon: <SecurityIcon />, visible: hasRole('System Administrator') },
    { label: 'Settings', path: '/settings', icon: <SettingsIcon />, visible: true },
  ];

  return (
    <Drawer
      variant={variant}
      open={open}
      onClose={onClose}
      sx={{
        width: 250,
        flexShrink: 0,
        '& .MuiDrawer-paper': {
          width: 250,
          boxSizing: 'border-box',
          bgcolor: '#ffffff',
          borderRight: '1px solid #e2e8f0',
          top: { sm: 64, xs: 56 },
          height: { sm: 'calc(100% - 64px)', xs: 'calc(100% - 56px)' },
        },
      }}
    >
      <Box sx={{ overflow: 'auto', py: 1.5 }}>
        <List disablePadding>
          <Box sx={{ px: 2, py: 1 }}>
            <Typography variant="overline" color="text.secondary" sx={{ fontWeight: 700, fontSize: '0.65rem' }}>
              MAIN NAVIGATION
            </Typography>
          </Box>

          {navItems.slice(0, 5).map((item) => item.visible && (
            <ListItemButton
              key={item.path}
              selected={isSelected(item.path)}
              onClick={() => handleNavigate(item.path)}
              sx={{
                mx: 1,
                my: 0.25,
                borderRadius: 1.5,
                '&.Mui-selected': {
                  bgcolor: '#eff6ff',
                  color: '#1e3a8a',
                  fontWeight: 600,
                  '& .MuiListItemIcon-root': { color: '#1e3a8a' },
                },
              }}
            >
              <ListItemIcon sx={{ minWidth: 40, color: '#64748b' }}>{item.icon}</ListItemIcon>
              <ListItemText primary={item.label} slotProps={{ primary: { sx: { fontSize: '0.875rem' } } }} />
            </ListItemButton>
          ))}

          <Divider sx={{ my: 1.5 }} />

          <Box sx={{ px: 2, py: 1 }}>
            <Typography variant="overline" color="text.secondary" sx={{ fontWeight: 700, fontSize: '0.65rem' }}>
              OPERATIONS & ASSETS
            </Typography>
          </Box>

          {navItems.slice(5, 8).map((item) => item.visible && (
            <ListItemButton
              key={item.path}
              selected={isSelected(item.path)}
              onClick={() => handleNavigate(item.path)}
              sx={{
                mx: 1,
                my: 0.25,
                borderRadius: 1.5,
                '&.Mui-selected': {
                  bgcolor: '#eff6ff',
                  color: '#1e3a8a',
                  fontWeight: 600,
                  '& .MuiListItemIcon-root': { color: '#1e3a8a' },
                },
              }}
            >
              <ListItemIcon sx={{ minWidth: 40, color: '#64748b' }}>{item.icon}</ListItemIcon>
              <ListItemText primary={item.label} slotProps={{ primary: { sx: { fontSize: '0.875rem' } } }} />
            </ListItemButton>
          ))}

          <Divider sx={{ my: 1.5 }} />

          <Box sx={{ px: 2, py: 1 }}>
            <Typography variant="overline" color="text.secondary" sx={{ fontWeight: 700, fontSize: '0.65rem' }}>
              APPROVALS & COMMITTEE
            </Typography>
          </Box>

          {navItems.slice(8).map((item) => item.visible && (
            <ListItemButton
              key={item.path}
              selected={isSelected(item.path)}
              onClick={() => handleNavigate(item.path)}
              sx={{
                mx: 1,
                my: 0.25,
                borderRadius: 1.5,
                '&.Mui-selected': {
                  bgcolor: '#eff6ff',
                  color: '#1e3a8a',
                  fontWeight: 600,
                  '& .MuiListItemIcon-root': { color: '#1e3a8a' },
                },
              }}
            >
              <ListItemIcon sx={{ minWidth: 40, color: '#64748b' }}>{item.icon}</ListItemIcon>
              <ListItemText primary={item.label} slotProps={{ primary: { sx: { fontSize: '0.875rem' } } }} />
            </ListItemButton>
          ))}
        </List>
      </Box>
    </Drawer>
  );
};
