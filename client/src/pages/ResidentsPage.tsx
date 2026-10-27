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
  Pagination,
  CircularProgress,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  MenuItem,
  Alert,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import SearchIcon from '@mui/icons-material/Search';
import api from '../api/client';
import { Resident, Flat, ApiResponse, PagedResult } from '../types';
import { useAuth } from '../context/AuthContext';

export const ResidentsPage: React.FC = () => {
  const { hasAnyRole } = useAuth();
  const canManage = hasAnyRole(['System Administrator', 'Secretary']);

  const [residents, setResidents] = useState<Resident[]>([]);
  const [flats, setFlats] = useState<Flat[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  const [openModal, setOpenModal] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [modalError, setModalError] = useState<string | null>(null);

  const [newResident, setNewResident] = useState({
    fullName: '',
    email: '',
    mobileNumber: '',
    residentType: 1, // Owner
    emergencyContactName: '',
    emergencyContactNumber: '',
    correspondenceAddress: '',
    initialFlatId: 0,
  });

  const fetchResidents = async () => {
    try {
      setLoading(true);
      const params = new URLSearchParams();
      params.append('page', page.toString());
      params.append('pageSize', '10');
      if (search) params.append('search', search);

      const res = await api.get<ApiResponse<PagedResult<Resident>>>(`/residents?${params.toString()}`);
      if (res.data?.success) {
        setResidents(res.data.data.items);
        setTotalPages(res.data.data.totalPages);
        setTotalCount(res.data.data.totalCount);
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
      if (res.data?.success && res.data.data.items.length > 0) {
        setFlats(res.data.data.items);
        setNewResident((prev) => ({ ...prev, initialFlatId: res.data.data.items[0].id }));
      }
    } catch (err) {
      console.error(err);
    }
  };

  useEffect(() => {
    fetchFlats();
  }, []);

  useEffect(() => {
    fetchResidents();
  }, [page]);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setPage(1);
    fetchResidents();
  };

  const handleCreateResident = async (e: React.FormEvent) => {
    e.preventDefault();
    setModalError(null);
    try {
      setSubmitting(true);
      const res = await api.post<ApiResponse<Resident>>('/residents', newResident);
      if (res.data?.success) {
        setOpenModal(false);
        setNewResident({
          fullName: '',
          email: '',
          mobileNumber: '',
          residentType: 1,
          emergencyContactName: '',
          emergencyContactNumber: '',
          correspondenceAddress: '',
          initialFlatId: flats[0]?.id || 0,
        });
        fetchResidents();
      }
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Failed to register resident.';
      setModalError(msg);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 2 }}>
        <Box>
          <Typography variant="h5" sx={{ fontWeight: 700, color: '#0f172a' }}>
            Residents & Flat Owners ({totalCount} registered)
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Directory of homeowners, co-owners, tenants, emergency contacts, and flat associations.
          </Typography>
        </Box>
        {canManage && (
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => setOpenModal(true)} sx={{ bgcolor: '#1e3a8a' }}>
            Register Resident
          </Button>
        )}
      </Box>

      {/* Search Bar */}
      <Card sx={{ p: 2 }}>
        <Box component="form" onSubmit={handleSearch} sx={{ display: 'flex', gap: 2 }}>
          <TextField
            size="small"
            placeholder="Search by resident name, email, or mobile..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            sx={{ flexGrow: 1 }}
          />
          <Button type="submit" variant="outlined" startIcon={<SearchIcon />}>
            Search
          </Button>
        </Box>
      </Card>

      {/* Table */}
      <Card>
        <CardContent sx={{ p: 0 }}>
          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 5 }}><CircularProgress /></Box>
          ) : (
            <Table sx={{ minWidth: 650 }}>
              <TableHead sx={{ bgcolor: '#f8fafc' }}>
                <TableRow>
                  <TableCell sx={{ fontWeight: 700 }}>Resident Name</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Type</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Contact Info</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Associated Flat(s)</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Emergency Contact</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Status</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {residents.map((res) => (
                  <TableRow key={res.id} hover>
                    <TableCell sx={{ fontWeight: 600 }}>{res.fullName}</TableCell>
                    <TableCell>
                      <Chip
                        label={res.residentType}
                        color={res.residentType === 'Owner' ? 'primary' : 'secondary'}
                        size="small"
                      />
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">{res.mobileNumber}</Typography>
                      <Typography variant="caption" color="text.secondary">{res.email}</Typography>
                    </TableCell>
                    <TableCell>
                      {res.associatedFlats?.length > 0 ? (
                        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                          {res.associatedFlats.map((flat, i) => (
                            <Chip key={i} label={flat} size="small" variant="outlined" />
                          ))}
                        </Box>
                      ) : (
                        <Typography variant="caption" color="text.secondary">None mapped</Typography>
                      )}
                    </TableCell>
                    <TableCell>
                      {res.emergencyContactName ? (
                        <Box>
                          <Typography variant="body2">{res.emergencyContactName}</Typography>
                          <Typography variant="caption" color="text.secondary">{res.emergencyContactNumber}</Typography>
                        </Box>
                      ) : (
                        '—'
                      )}
                    </TableCell>
                    <TableCell>
                      <Chip label={res.status} color="success" size="small" />
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}

          <Box sx={{ display: 'flex', justifyContent: 'flex-end', p: 2 }}>
            <Pagination count={totalPages} page={page} onChange={(_, v) => setPage(v)} color="primary" />
          </Box>
        </CardContent>
      </Card>

      {/* Register Resident Modal */}
      <Dialog open={openModal} onClose={() => setOpenModal(false)} maxWidth="sm" fullWidth>
        <form onSubmit={handleCreateResident}>
          <DialogTitle>Register Resident</DialogTitle>
          <DialogContent sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 1 }}>
            {modalError && <Alert severity="error">{modalError}</Alert>}
            <TextField
              label="Full Name"
              required
              value={newResident.fullName}
              onChange={(e) => setNewResident({ ...newResident, fullName: e.target.value })}
            />
            <TextField
              label="Email Address"
              required
              type="email"
              value={newResident.email}
              onChange={(e) => setNewResident({ ...newResident, email: e.target.value })}
            />
            <TextField
              label="Mobile Number"
              required
              value={newResident.mobileNumber}
              onChange={(e) => setNewResident({ ...newResident, mobileNumber: e.target.value })}
            />
            <TextField
              select
              label="Resident Type"
              value={newResident.residentType}
              onChange={(e) => setNewResident({ ...newResident, residentType: parseInt(e.target.value) })}
            >
              <MenuItem value={1}>Home Owner</MenuItem>
              <MenuItem value={2}>Tenant</MenuItem>
            </TextField>
            <TextField
              select
              label="Associate Flat"
              value={newResident.initialFlatId}
              onChange={(e) => setNewResident({ ...newResident, initialFlatId: parseInt(e.target.value) })}
            >
              {flats.map((f) => (
                <MenuItem key={f.id} value={f.id}>
                  {f.towerCode} - Flat {f.flatNumber}
                </MenuItem>
              ))}
            </TextField>
            <TextField
              label="Emergency Contact Name"
              value={newResident.emergencyContactName}
              onChange={(e) => setNewResident({ ...newResident, emergencyContactName: e.target.value })}
            />
            <TextField
              label="Emergency Contact Number"
              value={newResident.emergencyContactNumber}
              onChange={(e) => setNewResident({ ...newResident, emergencyContactNumber: e.target.value })}
            />
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setOpenModal(false)}>Cancel</Button>
            <Button type="submit" variant="contained" disabled={submitting}>
              {submitting ? 'Registering...' : 'Confirm Registration'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </Box>
  );
};
