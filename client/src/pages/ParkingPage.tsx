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
  CircularProgress,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Alert,
} from '@mui/material';
import LocalParkingIcon from '@mui/icons-material/LocalParking';
import AddIcon from '@mui/icons-material/Add';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import HighlightOffIcon from '@mui/icons-material/HighlightOff';
import api from '../api/client';
import { ParkingLot, Flat, ApiResponse } from '../types';
import { useAuth } from '../context/AuthContext';

export const ParkingPage: React.FC = () => {
  const { hasAnyRole } = useAuth();
  const canManage = hasAnyRole(['System Administrator', 'Secretary']);

  const [lots, setLots] = useState<ParkingLot[]>([]);
  const [flats, setFlats] = useState<Flat[]>([]);
  const [loading, setLoading] = useState(true);
  const [filterStatus, setFilterStatus] = useState<string>('');

  const [openAllocateModal, setOpenAllocateModal] = useState(false);
  const [selectedLot, setSelectedLot] = useState<ParkingLot | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [allocationForm, setAllocationForm] = useState({
    flatId: 0,
    vehicleNumber: '',
    vehicleType: 'Sedan',
  });

  const fetchLots = async () => {
    try {
      setLoading(true);
      const params = new URLSearchParams();
      if (filterStatus) params.append('status', filterStatus);

      const res = await api.get<ApiResponse<ParkingLot[]>>(`/parking/lots?${params.toString()}`);
      if (res.data?.success) {
        setLots(res.data.data);
      }
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const fetchFlats = async () => {
    try {
      const res = await api.get<ApiResponse<{ items: Flat[] }>>('/flats?pageSize=100');
      if (res.data?.success) {
        setFlats(res.data.data.items);
        if (res.data.data.items.length > 0) {
          setAllocationForm((prev) => ({ ...prev, flatId: res.data.data.items[0].id }));
        }
      }
    } catch (err) {
      console.error(err);
    }
  };

  useEffect(() => {
    fetchLots();
    fetchFlats();
  }, [filterStatus]);

  const handleOpenAllocate = (lot: ParkingLot) => {
    setSelectedLot(lot);
    setError(null);
    setOpenAllocateModal(true);
  };

  const handleAllocateSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedLot) return;
    setError(null);

    try {
      setSubmitting(true);
      const payload = {
        parkingLotId: selectedLot.id,
        flatId: allocationForm.flatId,
        vehicleNumber: allocationForm.vehicleNumber,
        vehicleType: allocationForm.vehicleType,
      };

      const res = await api.post<ApiResponse<string>>('/parking/allocate', payload);
      if (res.data?.success) {
        setOpenAllocateModal(false);
        fetchLots();
      }
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Allocation failed.';
      setError(msg);
    } finally {
      setSubmitting(false);
    }
  };

  const handleRelease = async (lot: ParkingLot) => {
    if (!window.confirm(`Are you sure you want to release parking space ${lot.lotNumber}?`)) return;
    try {
      // Find active allocation id or lot release
      const res = await api.get<ApiResponse<{ id: number }[]>>(`/parking/history?lotId=${lot.id}`);
      if (res.data?.success && res.data.data.length > 0) {
        const activeAlloc = res.data.data[0];
        await api.post(`/parking/release/${activeAlloc.id}`);
        fetchLots();
      }
    } catch (err) {
      console.error(err);
    }
  };

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 2 }}>
        <Box>
          <Typography variant="h5" sx={{ fontWeight: 700, color: '#0f172a' }}>
            Car Parking Management
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Master inventory of basement & surface parking spaces, vehicle allocation, and vacancy tracking.
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 1.5 }}>
          <TextField
            select
            size="small"
            label="Status Filter"
            value={filterStatus}
            onChange={(e) => setFilterStatus(e.target.value)}
            sx={{ minWidth: 160 }}
          >
            <MenuItem value="">All Statuses</MenuItem>
            <MenuItem value="1">Available</MenuItem>
            <MenuItem value="2">Allocated</MenuItem>
          </TextField>
        </Box>
      </Box>

      {/* Parking Lots Table */}
      <Card>
        <CardContent sx={{ p: 0 }}>
          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 5 }}><CircularProgress /></Box>
          ) : (
            <Table sx={{ minWidth: 650 }}>
              <TableHead sx={{ bgcolor: '#f8fafc' }}>
                <TableRow>
                  <TableCell sx={{ fontWeight: 700 }}>Lot #</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Location / Tower</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Type</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Level</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Status</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Allocated Flat & Resident</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Vehicle Info</TableCell>
                  {canManage && <TableCell sx={{ fontWeight: 700 }}>Actions</TableCell>}
                </TableRow>
              </TableHead>
              <TableBody>
                {lots.map((lot) => {
                  const isAllocated = lot.status === 'Allocated';
                  return (
                    <TableRow key={lot.id} hover>
                      <TableCell sx={{ fontWeight: 700 }}>{lot.lotNumber}</TableCell>
                      <TableCell>{lot.towerName || 'General'}</TableCell>
                      <TableCell>{lot.parkingType}</TableCell>
                      <TableCell>{lot.floorOrBasementLevel}</TableCell>
                      <TableCell>
                        <Chip
                          label={lot.status}
                          color={isAllocated ? 'primary' : 'success'}
                          size="small"
                          sx={{ fontWeight: 600 }}
                        />
                      </TableCell>
                      <TableCell>
                        {isAllocated ? (
                          <Box>
                            <Typography variant="body2" sx={{ fontWeight: 600 }}>
                              Flat {lot.allocatedFlatNumber}
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                              {lot.allocatedResidentName || 'Resident assigned'}
                            </Typography>
                          </Box>
                        ) : (
                          <Typography variant="caption" color="text.secondary">
                            Vacant slot
                          </Typography>
                        )}
                      </TableCell>
                      <TableCell>
                        {lot.vehicleNumber ? (
                          <Chip label={lot.vehicleNumber} size="small" variant="outlined" />
                        ) : (
                          '—'
                        )}
                      </TableCell>
                      {canManage && (
                        <TableCell>
                          {!isAllocated ? (
                            <Button
                              size="small"
                              variant="outlined"
                              startIcon={<AddIcon />}
                              onClick={() => handleOpenAllocate(lot)}
                            >
                              Allocate
                            </Button>
                          ) : (
                            <Button
                              size="small"
                              color="error"
                              variant="outlined"
                              startIcon={<HighlightOffIcon />}
                              onClick={() => handleRelease(lot)}
                            >
                              Release
                            </Button>
                          )}
                        </TableCell>
                      )}
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      {/* Allocation Modal */}
      <Dialog open={openAllocateModal} onClose={() => setOpenAllocateModal(false)} maxWidth="sm" fullWidth>
        <form onSubmit={handleAllocateSubmit}>
          <DialogTitle>Allocate Parking Space: {selectedLot?.lotNumber}</DialogTitle>
          <DialogContent sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 1 }}>
            {error && <Alert severity="error">{error}</Alert>}
            <TextField
              select
              label="Select Flat"
              required
              value={allocationForm.flatId}
              onChange={(e) => setAllocationForm({ ...allocationForm, flatId: parseInt(e.target.value) })}
            >
              {flats.map((f) => (
                <MenuItem key={f.id} value={f.id}>
                  {f.towerCode} - Flat {f.flatNumber}
                </MenuItem>
              ))}
            </TextField>
            <TextField
              label="Vehicle Registration Number (e.g. MH-02-AB-1234)"
              required
              value={allocationForm.vehicleNumber}
              onChange={(e) => setAllocationForm({ ...allocationForm, vehicleNumber: e.target.value })}
            />
            <TextField
              label="Vehicle Model / Type (e.g. Honda City / SUV)"
              value={allocationForm.vehicleType}
              onChange={(e) => setAllocationForm({ ...allocationForm, vehicleType: e.target.value })}
            />
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setOpenAllocateModal(false)}>Cancel</Button>
            <Button type="submit" variant="contained" disabled={submitting}>
              {submitting ? 'Allocating...' : 'Confirm Allocation'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </Box>
  );
};
