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
import DescriptionIcon from '@mui/icons-material/Description';
import api from '../api/client';
import { Vendor, VendorCategory, VendorContract, ApiResponse, PagedResult } from '../types';
import { useAuth } from '../context/AuthContext';

export const VendorsPage: React.FC = () => {
  const { hasAnyRole } = useAuth();
  const canManage = hasAnyRole(['System Administrator', 'Secretary', 'Treasurer']);

  const [vendors, setVendors] = useState<Vendor[]>([]);
  const [categories, setCategories] = useState<VendorCategory[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<string>('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  // Contracts view modal
  const [viewContractsModal, setViewContractsModal] = useState(false);
  const [selectedVendorContracts, setSelectedVendorContracts] = useState<VendorContract[]>([]);
  const [selectedVendorName, setSelectedVendorName] = useState('');

  // Add Vendor modal
  const [openAddModal, setOpenAddModal] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [modalError, setModalError] = useState<string | null>(null);

  const [newVendor, setNewVendor] = useState({
    vendorName: '',
    categoryId: 1,
    contactPerson: '',
    mobileNumber: '',
    email: '',
    address: '',
    taxRegistrationNumber: '',
    paymentTerms: 'Net 30 days',
  });

  const fetchVendors = async () => {
    try {
      setLoading(true);
      const params = new URLSearchParams();
      params.append('page', page.toString());
      params.append('pageSize', '10');
      if (search) params.append('search', search);
      if (selectedCategory) params.append('categoryId', selectedCategory);

      const res = await api.get<ApiResponse<PagedResult<Vendor>>>(`/vendors?${params.toString()}`);
      if (res.data?.success) {
        setVendors(res.data.data.items);
        setTotalPages(res.data.data.totalPages);
        setTotalCount(res.data.data.totalCount);
      }
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const fetchCategories = async () => {
    try {
      const res = await api.get<ApiResponse<VendorCategory[]>>('/vendors/categories');
      if (res.data?.success) {
        setCategories(res.data.data);
        if (res.data.data.length > 0) {
          setNewVendor((prev) => ({ ...prev, categoryId: res.data.data[0].id }));
        }
      }
    } catch (err) {
      console.error(err);
    }
  };

  useEffect(() => {
    fetchCategories();
  }, []);

  useEffect(() => {
    fetchVendors();
  }, [page, selectedCategory]);

  const handleOpenContracts = async (vendor: Vendor) => {
    setSelectedVendorName(vendor.vendorName);
    try {
      const res = await api.get<ApiResponse<VendorContract[]>>(`/vendors/${vendor.id}/contracts`);
      if (res.data?.success) {
        setSelectedVendorContracts(res.data.data);
        setViewContractsModal(true);
      }
    } catch (err) {
      console.error(err);
    }
  };

  const handleCreateVendor = async (e: React.FormEvent) => {
    e.preventDefault();
    setModalError(null);
    try {
      setSubmitting(true);
      const res = await api.post<ApiResponse<Vendor>>('/vendors', newVendor);
      if (res.data?.success) {
        setOpenAddModal(false);
        setNewVendor({
          vendorName: '',
          categoryId: categories[0]?.id || 1,
          contactPerson: '',
          mobileNumber: '',
          email: '',
          address: '',
          taxRegistrationNumber: '',
          paymentTerms: 'Net 30 days',
        });
        fetchVendors();
      }
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Failed to register vendor.';
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
            Vendor & Contractor Management ({totalCount} vendors)
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Authorized contractors for lifts, diesel generators, security, firefighting, and annual maintenance contracts.
          </Typography>
        </Box>
        {canManage && (
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => setOpenAddModal(true)} sx={{ bgcolor: '#1e3a8a' }}>
            Add New Vendor
          </Button>
        )}
      </Box>

      {/* Filter / Search Bar */}
      <Card sx={{ p: 2 }}>
        <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
          <TextField
            size="small"
            placeholder="Search vendor name, person, phone..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            sx={{ flexGrow: 1, minWidth: 200 }}
          />
          <TextField
            select
            size="small"
            label="Service Category"
            value={selectedCategory}
            onChange={(e) => { setSelectedCategory(e.target.value); setPage(1); }}
            sx={{ minWidth: 240 }}
          >
            <MenuItem value="">All Categories</MenuItem>
            {categories.map((c) => (
              <MenuItem key={c.id} value={c.id.toString()}>{c.name}</MenuItem>
            ))}
          </TextField>
          <Button variant="outlined" onClick={() => { setPage(1); fetchVendors(); }}>
            Filter
          </Button>
        </Box>
      </Card>

      {/* Vendor Table */}
      <Card>
        <CardContent sx={{ p: 0 }}>
          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 5 }}><CircularProgress /></Box>
          ) : (
            <Table sx={{ minWidth: 650 }}>
              <TableHead sx={{ bgcolor: '#f8fafc' }}>
                <TableRow>
                  <TableCell sx={{ fontWeight: 700 }}>Vendor / Agency</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Category</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Contact Person</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Mobile & Email</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Tax / GST Reg</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Active AMC</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Status</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Contracts</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {vendors.map((v) => (
                  <TableRow key={v.id} hover>
                    <TableCell sx={{ fontWeight: 600 }}>{v.vendorName}</TableCell>
                    <TableCell><Chip label={v.categoryName} size="small" variant="outlined" /></TableCell>
                    <TableCell>{v.contactPerson}</TableCell>
                    <TableCell>
                      <Typography variant="body2">{v.mobileNumber}</Typography>
                      <Typography variant="caption" color="text.secondary">{v.email}</Typography>
                    </TableCell>
                    <TableCell>{v.taxRegistrationNumber || 'N/A'}</TableCell>
                    <TableCell>
                      <Chip
                        label={`${v.activeContractsCount} Active`}
                        color={v.activeContractsCount > 0 ? 'success' : 'default'}
                        size="small"
                      />
                    </TableCell>
                    <TableCell>
                      <Chip label={v.serviceStatus} color="success" size="small" />
                    </TableCell>
                    <TableCell>
                      <Button
                        size="small"
                        startIcon={<DescriptionIcon />}
                        onClick={() => handleOpenContracts(v)}
                      >
                        View AMC
                      </Button>
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

      {/* View Contracts Dialog */}
      <Dialog open={viewContractsModal} onClose={() => setViewContractsModal(false)} maxWidth="md" fullWidth>
        <DialogTitle>AMC Contracts: {selectedVendorName}</DialogTitle>
        <DialogContent sx={{ pt: 1 }}>
          {selectedVendorContracts.length === 0 ? (
            <Typography variant="body2" color="text.secondary" sx={{ py: 3, textAlign: 'center' }}>
              No active or historical contracts recorded for this vendor.
            </Typography>
          ) : (
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell sx={{ fontWeight: 600 }}>Contract Number</TableCell>
                  <TableCell sx={{ fontWeight: 600 }}>Title & Scope</TableCell>
                  <TableCell sx={{ fontWeight: 600 }}>Duration</TableCell>
                  <TableCell sx={{ fontWeight: 600 }}>Contract Value</TableCell>
                  <TableCell sx={{ fontWeight: 600 }}>Status</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {selectedVendorContracts.map((c) => (
                  <TableRow key={c.id}>
                    <TableCell sx={{ fontWeight: 600 }}>{c.contractNumber}</TableCell>
                    <TableCell>
                      <Typography variant="body2" sx={{ fontWeight: 600 }}>{c.contractTitle}</Typography>
                      <Typography variant="caption" color="text.secondary">{c.scopeOfWork}</Typography>
                    </TableCell>
                    <TableCell>
                      {new Date(c.startDate).toLocaleDateString()} to {new Date(c.endDate).toLocaleDateString()}
                    </TableCell>
                    <TableCell sx={{ fontWeight: 600, color: '#16a34a' }}>
                      ₹{c.contractValue.toLocaleString('en-IN')}
                    </TableCell>
                    <TableCell>
                      <Chip label={c.status} color="success" size="small" />
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setViewContractsModal(false)}>Close</Button>
        </DialogActions>
      </Dialog>

      {/* Add Vendor Dialog */}
      <Dialog open={openAddModal} onClose={() => setOpenAddModal(false)} maxWidth="sm" fullWidth>
        <form onSubmit={handleCreateVendor}>
          <DialogTitle>Register New Vendor</DialogTitle>
          <DialogContent sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 1 }}>
            {modalError && <Alert severity="error">{modalError}</Alert>}
            <TextField
              label="Company / Vendor Name"
              required
              value={newVendor.vendorName}
              onChange={(e) => setNewVendor({ ...newVendor, vendorName: e.target.value })}
            />
            <TextField
              select
              label="Service Category"
              value={newVendor.categoryId}
              onChange={(e) => setNewVendor({ ...newVendor, categoryId: parseInt(e.target.value) })}
            >
              {categories.map((c) => (
                <MenuItem key={c.id} value={c.id}>{c.name}</MenuItem>
              ))}
            </TextField>
            <TextField
              label="Primary Contact Person"
              required
              value={newVendor.contactPerson}
              onChange={(e) => setNewVendor({ ...newVendor, contactPerson: e.target.value })}
            />
            <TextField
              label="Mobile Number"
              required
              value={newVendor.mobileNumber}
              onChange={(e) => setNewVendor({ ...newVendor, mobileNumber: e.target.value })}
            />
            <TextField
              label="Email Address"
              required
              type="email"
              value={newVendor.email}
              onChange={(e) => setNewVendor({ ...newVendor, email: e.target.value })}
            />
            <TextField
              label="GST / Tax Registration"
              value={newVendor.taxRegistrationNumber}
              onChange={(e) => setNewVendor({ ...newVendor, taxRegistrationNumber: e.target.value })}
            />
            <TextField
              label="Payment Terms (e.g. Net 30 days)"
              value={newVendor.paymentTerms}
              onChange={(e) => setNewVendor({ ...newVendor, paymentTerms: e.target.value })}
            />
            <TextField
              label="Office Address"
              multiline
              rows={2}
              value={newVendor.address}
              onChange={(e) => setNewVendor({ ...newVendor, address: e.target.value })}
            />
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setOpenAddModal(false)}>Cancel</Button>
            <Button type="submit" variant="contained" disabled={submitting}>
              {submitting ? 'Registering...' : 'Register Vendor'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </Box>
  );
};
