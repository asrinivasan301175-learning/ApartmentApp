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
  Stepper,
  Step,
  StepLabel,
  IconButton,
  Divider,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import DeleteIcon from '@mui/icons-material/Delete';
import VisibilityIcon from '@mui/icons-material/Visibility';
import api from '../api/client';
import { PurchaseRequest, Vendor, ApiResponse, PagedResult } from '../types';
import { useAuth } from '../context/AuthContext';

export const PurchaseRequestsPage: React.FC = () => {
  const { user, isCommitteeMember } = useAuth();
  const [requests, setRequests] = useState<PurchaseRequest[]>([]);
  const [vendors, setVendors] = useState<Vendor[]>([]);
  const [loading, setLoading] = useState(true);
  const [statusFilter, setStatusFilter] = useState('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  // View Details Modal
  const [selectedPR, setSelectedPR] = useState<PurchaseRequest | null>(null);
  const [detailsModalOpen, setDetailsModalOpen] = useState(false);

  // Create PR Modal
  const [createModalOpen, setCreateModalOpen] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [createError, setCreateError] = useState<string | null>(null);

  const [newPR, setNewPR] = useState({
    title: '',
    description: '',
    vendorId: 0,
    budgetCategory: 'General Maintenance',
    requiredDeliveryDate: new Date(Date.now() + 14 * 86400000).toISOString().split('T')[0],
    taxAndOtherCharges: 0,
    submitImmediately: true,
    items: [
      { itemDescription: '', quantity: 1, unitOfMeasure: 'Nos', estimatedUnitPrice: 1000 },
    ],
  });

  const fetchRequests = async () => {
    try {
      setLoading(true);
      const params = new URLSearchParams();
      params.append('page', page.toString());
      params.append('pageSize', '10');
      if (statusFilter) params.append('status', statusFilter);

      const res = await api.get<ApiResponse<PagedResult<PurchaseRequest>>>(`/purchase-requests?${params.toString()}`);
      if (res.data?.success) {
        setRequests(res.data.data.items);
        setTotalPages(res.data.data.totalPages);
        setTotalCount(res.data.data.totalCount);
      }
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const fetchVendors = async () => {
    try {
      const res = await api.get<ApiResponse<{ items: Vendor[] }>>('/vendors?pageSize=50');
      if (res.data?.success && res.data.data.items.length > 0) {
        setVendors(res.data.data.items);
        setNewPR((prev) => ({ ...prev, vendorId: res.data.data.items[0].id }));
      }
    } catch (err) {
      console.error(err);
    }
  };

  useEffect(() => {
    fetchVendors();
  }, []);

  useEffect(() => {
    fetchRequests();
  }, [page, statusFilter]);

  const handleOpenDetails = async (pr: PurchaseRequest) => {
    try {
      const res = await api.get<ApiResponse<PurchaseRequest>>(`/purchase-requests/${pr.id}`);
      if (res.data?.success) {
        setSelectedPR(res.data.data);
        setDetailsModalOpen(true);
      }
    } catch (err) {
      console.error(err);
    }
  };

  const handleAddItem = () => {
    setNewPR({
      ...newPR,
      items: [...newPR.items, { itemDescription: '', quantity: 1, unitOfMeasure: 'Nos', estimatedUnitPrice: 500 }],
    });
  };

  const handleRemoveItem = (index: number) => {
    if (newPR.items.length === 1) return;
    setNewPR({
      ...newPR,
      items: newPR.items.filter((_, i) => i !== index),
    });
  };

  const handleItemChange = (index: number, field: string, value: string | number) => {
    const updated = [...newPR.items];
    updated[index] = { ...updated[index], [field]: value };
    setNewPR({ ...newPR, items: updated });
  };

  const handleCreateSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setCreateError(null);
    try {
      setSubmitting(true);
      const res = await api.post<ApiResponse<PurchaseRequest>>('/purchase-requests', newPR);
      if (res.data?.success) {
        setCreateModalOpen(false);
        fetchRequests();
      }
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Failed to submit purchase request.';
      setCreateError(msg);
    } finally {
      setSubmitting(false);
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Approved': return 'success';
      case 'PendingApproval': return 'warning';
      case 'Rejected': return 'error';
      case 'ReturnedForChanges': return 'info';
      default: return 'default';
    }
  };

  const calculateActiveStep = (pr: PurchaseRequest) => {
    if (pr.status === 'Approved') return 3;
    if (pr.status === 'Rejected') return pr.currentApprovalStep - 1;
    return pr.currentApprovalStep - 1;
  };

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 2 }}>
        <Box>
          <Typography variant="h5" sx={{ fontWeight: 700, color: '#0f172a' }}>
            Purchase Requests ({totalCount} requests)
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Multi-tier sequential approval workflow: President → Secretary → Treasurer.
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <TextField
            select
            size="small"
            label="Status Filter"
            value={statusFilter}
            onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }}
            sx={{ minWidth: 180 }}
          >
            <MenuItem value="">All Statuses</MenuItem>
            <MenuItem value="1">Draft</MenuItem>
            <MenuItem value="2">Pending Approval</MenuItem>
            <MenuItem value="4">Approved</MenuItem>
            <MenuItem value="5">Rejected</MenuItem>
          </TextField>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => setCreateModalOpen(true)}
            sx={{ bgcolor: '#1e3a8a' }}
          >
            New Purchase Request
          </Button>
        </Box>
      </Box>

      {/* Requests Table */}
      <Card>
        <CardContent sx={{ p: 0 }}>
          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 5 }}><CircularProgress /></Box>
          ) : (
            <Table sx={{ minWidth: 650 }}>
              <TableHead sx={{ bgcolor: '#f8fafc' }}>
                <TableRow>
                  <TableCell sx={{ fontWeight: 700 }}>Request #</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Title & Scope</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Requester</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Vendor Quoted</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Grand Total</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Status</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Current Approver</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {requests.map((pr) => (
                  <TableRow key={pr.id} hover>
                    <TableCell sx={{ fontWeight: 700 }}>{pr.requestNumber}</TableCell>
                    <TableCell>
                      <Typography variant="body2" sx={{ fontWeight: 600 }}>{pr.title}</Typography>
                      <Typography variant="caption" color="text.secondary">{pr.budgetCategory}</Typography>
                    </TableCell>
                    <TableCell>{pr.requesterName}</TableCell>
                    <TableCell>{pr.vendorName || 'Not specified'}</TableCell>
                    <TableCell sx={{ fontWeight: 700, color: '#16a34a' }}>
                      ₹{pr.grandTotal.toLocaleString('en-IN')}
                    </TableCell>
                    <TableCell>
                      <Chip label={pr.status} color={getStatusColor(pr.status)} size="small" sx={{ fontWeight: 600 }} />
                    </TableCell>
                    <TableCell>
                      {pr.currentAssignedRole ? (
                        <Chip label={`Awaiting ${pr.currentAssignedRole}`} size="small" variant="outlined" color="primary" />
                      ) : pr.status === 'Approved' ? (
                        <Chip label="Workflow Completed" size="small" color="success" />
                      ) : (
                        '—'
                      )}
                    </TableCell>
                    <TableCell>
                      <Button
                        size="small"
                        startIcon={<VisibilityIcon />}
                        onClick={() => handleOpenDetails(pr)}
                      >
                        Inspect
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

      {/* Details & Sequential Stepper Modal */}
      <Dialog open={detailsModalOpen} onClose={() => setDetailsModalOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Box>
            <Typography variant="h6" sx={{ fontWeight: 700 }}>
              {selectedPR?.requestNumber}: {selectedPR?.title}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              Submitted on {selectedPR?.createdAtUtc ? new Date(selectedPR.createdAtUtc).toLocaleDateString() : ''} by {selectedPR?.requesterName}
            </Typography>
          </Box>
          <Chip label={selectedPR?.status} color={getStatusColor(selectedPR?.status || '')} sx={{ fontWeight: 700 }} />
        </DialogTitle>
        <DialogContent sx={{ display: 'flex', flexDirection: 'column', gap: 3, pt: 1 }}>
          {/* Stepper */}
          <Box sx={{ bgcolor: '#f8fafc', p: 2.5, borderRadius: 2 }}>
            <Typography variant="subtitle2" sx={{ fontWeight: 700, mb: 2 }}>
              Sequential Committee Approval Chain
            </Typography>
            <Stepper activeStep={selectedPR ? calculateActiveStep(selectedPR) : 0}>
              <Step completed={selectedPR?.approvals.some((a) => a.approverRole === 'President' && a.decision === 'Approved')}>
                <StepLabel>President Approval</StepLabel>
              </Step>
              <Step completed={selectedPR?.approvals.some((a) => a.approverRole === 'Secretary' && a.decision === 'Approved')}>
                <StepLabel>Secretary Approval</StepLabel>
              </Step>
              <Step completed={selectedPR?.approvals.some((a) => a.approverRole === 'Treasurer' && a.decision === 'Approved')}>
                <StepLabel>Treasurer Approval</StepLabel>
              </Step>
            </Stepper>
          </Box>

          {/* Description & Financials */}
          <Box sx={{ display: 'flex', justifyContent: 'space-between', bgcolor: '#f1f5f9', p: 2, borderRadius: 2 }}>
            <Box>
              <Typography variant="caption" color="text.secondary">Budget Category</Typography>
              <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>{selectedPR?.budgetCategory}</Typography>
            </Box>
            <Box>
              <Typography variant="caption" color="text.secondary">Selected Vendor</Typography>
              <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>{selectedPR?.vendorName || 'N/A'}</Typography>
            </Box>
            <Box>
              <Typography variant="caption" color="text.secondary">Item Subtotal</Typography>
              <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>₹{selectedPR?.estimatedTotalAmount?.toLocaleString('en-IN')}</Typography>
            </Box>
            <Box>
              <Typography variant="caption" color="text.secondary">Grand Total (incl. Taxes)</Typography>
              <Typography variant="subtitle1" sx={{ fontWeight: 700, color: '#16a34a' }}>₹{selectedPR?.grandTotal?.toLocaleString('en-IN')}</Typography>
            </Box>
          </Box>

          <Typography variant="body2">{selectedPR?.description}</Typography>

          {/* Line items table */}
          <Box>
            <Typography variant="subtitle2" sx={{ fontWeight: 700, mb: 1 }}>Line Items Breakdown</Typography>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Item Description</TableCell>
                  <TableCell>Qty</TableCell>
                  <TableCell>UoM</TableCell>
                  <TableCell>Unit Price</TableCell>
                  <TableCell>Total Price</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {selectedPR?.items.map((item) => (
                  <TableRow key={item.id}>
                    <TableCell>{item.itemDescription}</TableCell>
                    <TableCell>{item.quantity}</TableCell>
                    <TableCell>{item.unitOfMeasure}</TableCell>
                    <TableCell>₹{item.estimatedUnitPrice.toLocaleString('en-IN')}</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>₹{item.totalEstimatedPrice.toLocaleString('en-IN')}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </Box>

          {/* Committee Decision Audit History */}
          <Box>
            <Typography variant="subtitle2" sx={{ fontWeight: 700, mb: 1 }}>Committee Audit Trail</Typography>
            {selectedPR?.approvals && selectedPR.approvals.length > 0 ? (
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Date</TableCell>
                    <TableCell>Committee Role</TableCell>
                    <TableCell>Approver</TableCell>
                    <TableCell>Decision</TableCell>
                    <TableCell>Comments</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {selectedPR.approvals.map((app) => (
                    <TableRow key={app.id}>
                      <TableCell>{new Date(app.decisionDateUtc).toLocaleString()}</TableCell>
                      <TableCell><Chip label={app.approverRole} size="small" /></TableCell>
                      <TableCell>{app.approverName}</TableCell>
                      <TableCell>
                        <Chip
                          label={app.decision}
                          color={app.decision === 'Approved' ? 'success' : app.decision === 'Rejected' ? 'error' : 'warning'}
                          size="small"
                        />
                      </TableCell>
                      <TableCell>{app.comments || '—'}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            ) : (
              <Typography variant="body2" color="text.secondary">No decisions recorded yet.</Typography>
            )}
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDetailsModalOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>

      {/* Create PR Modal */}
      <Dialog open={createModalOpen} onClose={() => setCreateModalOpen(false)} maxWidth="md" fullWidth>
        <form onSubmit={handleCreateSubmit}>
          <DialogTitle>Create New Purchase Request</DialogTitle>
          <DialogContent sx={{ display: 'flex', flexDirection: 'column', gap: 2.5, pt: 1 }}>
            {createError && <Alert severity="error">{createError}</Alert>}
            <TextField
              label="Request Title (e.g. Lift Annual Service Spare Parts)"
              required
              value={newPR.title}
              onChange={(e) => setNewPR({ ...newPR, title: e.target.value })}
            />
            <TextField
              label="Detailed Scope / Description"
              multiline
              rows={2}
              required
              value={newPR.description}
              onChange={(e) => setNewPR({ ...newPR, description: e.target.value })}
            />
            <Box sx={{ display: 'flex', gap: 2 }}>
              <TextField
                select
                label="Selected Vendor"
                fullWidth
                value={newPR.vendorId}
                onChange={(e) => setNewPR({ ...newPR, vendorId: parseInt(e.target.value) })}
              >
                {vendors.map((v) => (
                  <MenuItem key={v.id} value={v.id}>{v.vendorName}</MenuItem>
                ))}
              </TextField>
              <TextField
                select
                label="Budget Category"
                fullWidth
                value={newPR.budgetCategory}
                onChange={(e) => setNewPR({ ...newPR, budgetCategory: e.target.value })}
              >
                <MenuItem value="General Maintenance">General Maintenance</MenuItem>
                <MenuItem value="Power & Backup Maintenance">Power & Backup Maintenance</MenuItem>
                <MenuItem value="Elevator & Vertical Transport">Elevator & Vertical Transport</MenuItem>
                <MenuItem value="Security & Access Systems">Security & Access Systems</MenuItem>
                <MenuItem value="Fire Safety & Compliance">Fire Safety & Compliance</MenuItem>
              </TextField>
            </Box>

            <Divider />

            {/* Line Items Builder */}
            <Box>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
                <Typography variant="subtitle2" sx={{ fontWeight: 700 }}>Line Items</Typography>
                <Button size="small" startIcon={<AddIcon />} onClick={handleAddItem}>
                  Add Item
                </Button>
              </Box>

              {newPR.items.map((item, idx) => (
                <Box key={idx} sx={{ display: 'flex', gap: 1.5, mb: 1.5, alignItems: 'center' }}>
                  <TextField
                    label="Description"
                    size="small"
                    required
                    sx={{ flexGrow: 2 }}
                    value={item.itemDescription}
                    onChange={(e) => handleItemChange(idx, 'itemDescription', e.target.value)}
                  />
                  <TextField
                    label="Qty"
                    size="small"
                    type="number"
                    required
                    sx={{ width: 80 }}
                    value={item.quantity}
                    onChange={(e) => handleItemChange(idx, 'quantity', parseInt(e.target.value) || 1)}
                  />
                  <TextField
                    label="UoM"
                    size="small"
                    sx={{ width: 90 }}
                    value={item.unitOfMeasure}
                    onChange={(e) => handleItemChange(idx, 'unitOfMeasure', e.target.value)}
                  />
                  <TextField
                    label="Unit Price (₹)"
                    size="small"
                    type="number"
                    required
                    sx={{ width: 130 }}
                    value={item.estimatedUnitPrice}
                    onChange={(e) => handleItemChange(idx, 'estimatedUnitPrice', parseFloat(e.target.value) || 0)}
                  />
                  <IconButton onClick={() => handleRemoveItem(idx)} color="error" disabled={newPR.items.length === 1}>
                    <DeleteIcon fontSize="small" />
                  </IconButton>
                </Box>
              ))}
            </Box>

            <Box sx={{ display: 'flex', gap: 2 }}>
              <TextField
                label="Taxes & Other Charges (₹)"
                type="number"
                fullWidth
                value={newPR.taxAndOtherCharges}
                onChange={(e) => setNewPR({ ...newPR, taxAndOtherCharges: parseFloat(e.target.value) || 0 })}
              />
              <TextField
                label="Required Delivery Date"
                type="date"
                fullWidth
                slotProps={{ inputLabel: { shrink: true } }}
                value={newPR.requiredDeliveryDate}
                onChange={(e) => setNewPR({ ...newPR, requiredDeliveryDate: e.target.value })}
              />
            </Box>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setCreateModalOpen(false)}>Cancel</Button>
            <Button type="submit" variant="contained" disabled={submitting} sx={{ bgcolor: '#1e3a8a' }}>
              {submitting ? 'Submitting...' : 'Submit Request for Approval'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </Box>
  );
};
