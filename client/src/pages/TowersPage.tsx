import React, { useEffect, useState } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Grid,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Chip,
  CircularProgress,
  Alert,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import ApartmentIcon from '@mui/icons-material/Apartment';
import MeetingRoomIcon from '@mui/icons-material/MeetingRoom';
import api from '../api/client';
import { Tower, ApiResponse } from '../types';
import { useAuth } from '../context/AuthContext';

export const TowersPage: React.FC = () => {
  const { hasAnyRole } = useAuth();
  const canManage = hasAnyRole(['System Administrator', 'Secretary']);

  const [towers, setTowers] = useState<Tower[]>([]);
  const [loading, setLoading] = useState(true);
  const [openModal, setOpenModal] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [newTower, setNewTower] = useState({
    towerCode: '',
    towerName: '',
    numberOfFloors: 14,
    totalFlats: 56,
    description: '',
  });

  const fetchTowers = async () => {
    try {
      setLoading(true);
      const res = await api.get<ApiResponse<Tower[]>>('/towers');
      if (res.data?.success) {
        setTowers(res.data.data);
      }
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTowers();
  }, []);

  const handleCreateTower = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    try {
      setSubmitting(true);
      const res = await api.post<ApiResponse<Tower>>('/towers', newTower);
      if (res.data?.success) {
        setOpenModal(false);
        setNewTower({ towerCode: '', towerName: '', numberOfFloors: 14, totalFlats: 56, description: '' });
        fetchTowers();
      }
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Failed to create tower.';
      setError(msg);
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) return <Box sx={{ display: 'flex', justifyContent: 'center', p: 5 }}><CircularProgress /></Box>;

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Box>
          <Typography variant="h5" sx={{ fontWeight: 700, color: '#0f172a' }}>
            Tower Management
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Configure towers, wings, floor plans, and capacity across the residential society.
          </Typography>
        </Box>
        {canManage && (
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => setOpenModal(true)} sx={{ bgcolor: '#1e3a8a' }}>
            Add New Tower
          </Button>
        )}
      </Box>

      <Grid container spacing={3}>
        {towers.map((tower) => (
          <Grid size={{ xs: 12, md: 4 }} key={tower.id}>
            <Card sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
              <CardContent sx={{ flexGrow: 1 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                  <Chip label={tower.towerCode} color="primary" sx={{ fontWeight: 700 }} />
                  <Chip label={tower.isActive ? 'Active' : 'Inactive'} color={tower.isActive ? 'success' : 'default'} size="small" />
                </Box>
                <Typography variant="h6" sx={{ fontWeight: 700, mb: 1 }}>
                  {tower.towerName}
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                  {tower.description || 'No description provided.'}
                </Typography>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', bgcolor: '#f8fafc', p: 1.5, borderRadius: 2 }}>
                  <Box>
                    <Typography variant="caption" color="text.secondary">Floors</Typography>
                    <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>{tower.numberOfFloors}</Typography>
                  </Box>
                  <Box>
                    <Typography variant="caption" color="text.secondary">Total Flats</Typography>
                    <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>{tower.totalFlats}</Typography>
                  </Box>
                  <Box>
                    <Typography variant="caption" color="text.secondary">Occupied</Typography>
                    <Typography variant="subtitle1" sx={{ fontWeight: 600, color: '#16a34a' }}>{tower.occupiedFlatsCount}</Typography>
                  </Box>
                  <Box>
                    <Typography variant="caption" color="text.secondary">Vacant</Typography>
                    <Typography variant="subtitle1" sx={{ fontWeight: 600, color: '#d97706' }}>{tower.vacantFlatsCount}</Typography>
                  </Box>
                </Box>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      {/* Add Tower Modal */}
      <Dialog open={openModal} onClose={() => setOpenModal(false)} maxWidth="sm" fullWidth>
        <form onSubmit={handleCreateTower}>
          <DialogTitle>Add New Tower</DialogTitle>
          <DialogContent sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 1 }}>
            {error && <Alert severity="error">{error}</Alert>}
            <TextField
              label="Tower Code (e.g. T-D)"
              required
              value={newTower.towerCode}
              onChange={(e) => setNewTower({ ...newTower, towerCode: e.target.value })}
            />
            <TextField
              label="Tower Name (e.g. Tower D - Cedar Wing)"
              required
              value={newTower.towerName}
              onChange={(e) => setNewTower({ ...newTower, towerName: e.target.value })}
            />
            <TextField
              label="Number of Floors"
              type="number"
              required
              value={newTower.numberOfFloors}
              onChange={(e) => setNewTower({ ...newTower, numberOfFloors: parseInt(e.target.value) || 1 })}
            />
            <TextField
              label="Total Flats"
              type="number"
              required
              value={newTower.totalFlats}
              onChange={(e) => setNewTower({ ...newTower, totalFlats: parseInt(e.target.value) || 1 })}
            />
            <TextField
              label="Description"
              multiline
              rows={2}
              value={newTower.description}
              onChange={(e) => setNewTower({ ...newTower, description: e.target.value })}
            />
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setOpenModal(false)}>Cancel</Button>
            <Button type="submit" variant="contained" disabled={submitting}>
              {submitting ? 'Creating...' : 'Create Tower'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </Box>
  );
};
