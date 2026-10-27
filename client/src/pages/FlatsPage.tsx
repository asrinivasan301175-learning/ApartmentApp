import React, { useEffect, useState } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Table,
  TableHead,
  TableRow,
  TableCell,
  TableBody,
  Chip,
  Button,
  TextField,
  MenuItem,
  Pagination,
  CircularProgress,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Alert,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import SearchIcon from '@mui/icons-material/Search';
import api from '../api/client';
import { Flat, Tower, ApiResponse, PagedResult } from '../types';
import { useAuth } from '../context/AuthContext';

export const FlatsPage: React.FC = () => {
  const { hasAnyRole } = useAuth();
  const canManage = hasAnyRole(['System Administrator', 'Secretary']);

  const [flats, setFlats] = useState<Flat[]>([]);
  const [towers, setTowers] = useState<Tower[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [selectedTower, setSelectedTower] = useState<string>('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  const [openModal, setOpenModal] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [modalError, setModalError] = useState<string | null>(null);

  const [newFlat, setNewFlat] = useState({
    towerId: 1,
    flatNumber: '',
    floorNumber: 1,
    areaSqFt: 1500,
    areaMeasurementType: 2, // SuperBuiltUpArea
    ownershipStatus: 2, // Unsold
    occupancyStatus: 3, // Vacant
  });

  const fetchFlats = async () => {
    try {
      setLoading(true);
      const params = new URLSearchParams();
      params.append('page', page.toString());
      params.append('pageSize', '10');
      if (search) params.append('search', search);
      if (selectedTower) params.append('towerId', selectedTower);

      const res = await api.get<ApiResponse<PagedResult<Flat>>>(`/flats?${params.toString()}`);
      if (res.data?.success) {
        setFlats(res.data.data.items);
        setTotalPages(res.data.data.totalPages);
        setTotalCount(res.data.data.totalCount);
      }
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const fetchTowers = async () => {
    try {
      const res = await api.get<ApiResponse<Tower[]>>('/towers');
      if (res.data?.success) {
        setTowers(res.data.data);
        if (res.data.data.length > 0) {
          setNewFlat((prev) => ({ ...prev, towerId: res.data.data[0].id }));
        }
      }
    } catch (err) {
      console.error(err);
    }
  };

  useEffect(() => {
    fetchTowers();
  }, []);

  useEffect(() => {
    fetchFlats();
  }, [page, selectedTower]);

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setPage(1);
    fetchFlats();
  };

  const handleCreateFlat = async (e: React.FormEvent) => {
    e.preventDefault();
    setModalError(null);
    try {
      setSubmitting(true);
      const res = await api.post<ApiResponse<Flat>>('/flats', newFlat);
      if (res.data?.success) {
        setOpenModal(false);
        setNewFlat({ ...newFlat, flatNumber: '' });
        fetchFlats();
      }
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Failed to create flat.';
      setModalError(msg);
    } finally {
      setSubmitting(false);
    }
  };

  const getOccupancyColor = (status: string) => {
    switch (status) {
      case 'SelfOccupied': return 'primary';
      case 'TenantOccupied': return 'info';
      case 'Vacant': return 'warning';
      default: return 'default';
    }
  };

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 2 }}>
        <Box>
          <Typography variant="h5" sx={{ fontWeight: 700, color: '#0f172a' }}>
            Flats Directory ({totalCount} total)
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Manage apartment flats, carpet & built-up square footage, ownership history, and occupancy status.
          </Typography>
        </Box>
        {canManage && (
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => setOpenModal(true)} sx={{ bgcolor: '#1e3a8a' }}>
            Register New Flat
          </Button>
        )}
      </Box>

      {/* Filters Bar */}
      <Card sx={{ p: 2 }}>
        <Box component="form" onSubmit={handleSearchSubmit} sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
          <TextField
            size="small"
            placeholder="Search flat number..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            sx={{ minWidth: 220 }}
          />
          <TextField
            select
            size="small"
            label="Filter by Tower"
            value={selectedTower}
            onChange={(e) => { setSelectedTower(e.target.value); setPage(1); }}
            sx={{ minWidth: 200 }}
          >
            <MenuItem value="">All Towers</MenuItem>
            {towers.map((t) => (
              <MenuItem key={t.id} value={t.id.toString()}>
                {t.towerCode} - {t.towerName}
              </MenuItem>
            ))}
          </TextField>
          <Button type="submit" variant="outlined" startIcon={<SearchIcon />}>
            Search
          </Button>
        </Box>
      </Card>

      {/* Flats Table */}
      <Card>
        <CardContent sx={{ p: 0 }}>
          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 5 }}><CircularProgress /></Box>
          ) : (
            <Table sx={{ minWidth: 650 }}>
              <TableHead sx={{ bgcolor: '#f8fafc' }}>
                <TableRow>
                  <TableCell sx={{ fontWeight: 700 }}>Flat #</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Tower</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Floor</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Area (Sq. Ft)</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Occupancy</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Owners / Residents</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Parking Lots</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {flats.map((flat) => (
                  <TableRow key={flat.id} hover>
                    <TableCell sx={{ fontWeight: 600 }}>{flat.flatNumber}</TableCell>
                    <TableCell>{flat.towerCode} ({flat.towerName})</TableCell>
                    <TableCell>Floor {flat.floorNumber}</TableCell>
                    <TableCell>{flat.areaSqFt} sq ft ({flat.areaMeasurementType})</TableCell>
                    <TableCell>
                      <Chip label={flat.occupancyStatus} color={getOccupancyColor(flat.occupancyStatus)} size="small" />
                    </TableCell>
                    <TableCell>
                      {flat.owners.length > 0 ? (
                        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5 }}>
                          {flat.owners.map((o) => (
                            <Typography variant="body2" key={o.residentId} sx={{ fontWeight: 500 }}>
                              {o.fullName} (Owner - {o.ownershipPercentage}%)
                            </Typography>
                          ))}
                        </Box>
                      ) : flat.occupants.length > 0 ? (
                        <Typography variant="body2" color="text.secondary">
                          Tenant: {flat.occupants[0].fullName}
                        </Typography>
                      ) : (
                        <Typography variant="body2" color="text.secondary">None registered</Typography>
                      )}
                    </TableCell>
                    <TableCell>
                      {flat.allocatedParkingLots.length > 0 ? (
                        <Box sx={{ display: 'flex', gap: 0.5 }}>
                          {flat.allocatedParkingLots.map((lot) => (
                            <Chip key={lot} label={lot} size="small" variant="outlined" />
                          ))}
                        </Box>
                      ) : (
                        <Typography variant="caption" color="text.secondary">No slot assigned</Typography>
                      )}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}

          <Box sx={{ display: 'flex', justifyContent: 'flex-end', p: 2 }}>
            <Pagination
              count={totalPages}
              page={page}
              onChange={(_, value) => setPage(value)}
              color="primary"
            />
          </Box>
        </CardContent>
      </Card>

      {/* Register Flat Modal */}
      <Dialog open={openModal} onClose={() => setOpenModal(false)} maxWidth="sm" fullWidth>
        <form onSubmit={handleCreateFlat}>
          <DialogTitle>Register New Flat</DialogTitle>
          <DialogContent sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 1 }}>
            {modalError && <Alert severity="error">{modalError}</Alert>}
            <TextField
              select
              label="Select Tower"
              required
              value={newFlat.towerId}
              onChange={(e) => setNewFlat({ ...newFlat, towerId: parseInt(e.target.value) })}
            >
              {towers.map((t) => (
                <MenuItem key={t.id} value={t.id}>{t.towerCode} - {t.towerName}</MenuItem>
              ))}
            </TextField>
            <TextField
              label="Flat Number (e.g. 101, 204)"
              required
              value={newFlat.flatNumber}
              onChange={(e) => setNewFlat({ ...newFlat, flatNumber: e.target.value })}
            />
            <TextField
              label="Floor Number"
              type="number"
              required
              value={newFlat.floorNumber}
              onChange={(e) => setNewFlat({ ...newFlat, floorNumber: parseInt(e.target.value) || 0 })}
            />
            <TextField
              label="Area in Square Feet"
              type="number"
              required
              value={newFlat.areaSqFt}
              onChange={(e) => setNewFlat({ ...newFlat, areaSqFt: parseFloat(e.target.value) || 0 })}
            />
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setOpenModal(false)}>Cancel</Button>
            <Button type="submit" variant="contained" disabled={submitting}>
              {submitting ? 'Registering...' : 'Register Flat'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </Box>
  );
};
