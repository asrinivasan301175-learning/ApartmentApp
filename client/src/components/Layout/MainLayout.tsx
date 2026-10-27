import React, { useState } from 'react';
import { Box, Toolbar, useMediaQuery, useTheme } from '@mui/material';
import { Outlet } from 'react-router-dom';
import { Navbar } from './Navbar';
import { Sidebar } from './Sidebar';

export const MainLayout: React.FC = () => {
  const theme = useTheme();
  const isDesktop = useMediaQuery(theme.breakpoints.up('md'));
  const [mobileOpen, setMobileOpen] = useState(false);

  const handleDrawerToggle = () => {
    setMobileOpen(!mobileOpen);
  };

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh', bgcolor: '#f8fafc' }}>
      <Navbar onToggleSidebar={handleDrawerToggle} />
      
      <Sidebar
        open={isDesktop ? true : mobileOpen}
        onClose={handleDrawerToggle}
        variant={isDesktop ? 'permanent' : 'temporary'}
      />

      <Box
        component="main"
        sx={{
          flexGrow: 1,
          p: { xs: 2, sm: 3, md: 4 },
          width: { sm: `calc(100% - ${isDesktop ? 250 : 0}px)` },
          minHeight: '100vh',
          boxSizing: 'border-box',
        }}
      >
        <Toolbar /> {/* Spacer for fixed appbar */}
        <Outlet />
      </Box>
    </Box>
  );
};
