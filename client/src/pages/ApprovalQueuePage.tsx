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
  CircularProgress,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  MenuItem,
  Alert,
} from '@mui/material';
import FactCheckIcon from '@mui/icons-material/FactCheck';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import CancelOutlinedIcon from '@mui/icons-material/CancelOutlined';
import ReplyIcon from '@mui/icons-material/Reply';
import api from '../api/client';
import { PurchaseRequest, ApiResponse } from '../types';
import { useAuth } from '../context/AuthContext';

export const ApprovalQueuePage: React.FC = () => {
  const { user } = useAuth();
  const [pendingQueue, setPendingQueue] = useState<PurchaseRequest[]>([]);
  const [loading, setLoading] = useState(true);

  const [selectedPR, setSelectedPR] = useState<PurchaseRequest | null>(null);
  const [decisionModalOpen, setDecisionModalOpen] = useState(false);
  const [decision, setDecision] = useState<number>(1); // 1 = Approved, 2 = Rejected, 3 = ReturnedForChanges
  const [comments, setComments] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [actionError, setActionError] = useState<string | null>(null);

  const fetchQueue = async () => {
    try {
      setLoading(true);
      const res = await api.get<ApiResponse<PurchaseRequest[]>>('/purchase-requests/my-pending-approvals');
      if (res.data?.success) {
        setPendingQueue(res.data.data);
      }
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchQueue();
  }, []);

  const handleOpenDecision = (pr: PurchaseRequest, defaultDecision: number) => {
    setSelectedPR(pr);
    setDecision(defaultDecision);
    setComments('');
    setActionError(null);
    setDecisionModalOpen(true);
  };

  const handleDecisionSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedPR) return;
    setActionError(null);

    try {
      setSubmitting(true);
      const payload = {
        decision,
        comments,
        rowVersion: selectedPR.rowVersion,
      };

      const res = await api.post<ApiResponse<string>>(`/purchase-requests/${selectedPR.id}/approval-action`, payload);
      if (res.data?.success) {
        setDecisionModalOpen(false);
        fetchQueue();
      }
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Failed to record decision.';
      setActionError(msg);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      <Box>
        <Typography variant="h5" sx={{ fontWeight: 700, color: '#0f172a' }}>
          Committee Approval Queue ({pendingQueue.length} pending)
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Review, approve, or return requests for which your committee office holds current signing authority.
        </Typography>
      </Box>

      {/* Queue Table */}
      <Card>
        <CardContent sx={{ p: 0 }}>
          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 5 }}><CircularProgress /></Box>
          ) : pendingQueue.length === 0 ? (
            <Box sx={{ p: 5, textAlign: 'center' }}>
              <FactCheckIcon sx={{ fontSize: 48, color: '#94a3b8', mb: 1.5 }} />
              <Typography variant="h6" color="text.secondary">All caught up!</Typography>
              <Typography variant="body2" color="text.secondary">
                There are no purchase requests currently pending your approval.
              </Typography>
            </Box>
          ) : (
            <Table sx={{ minWidth: 650 }}>
              <TableHead sx={{ bgcolor: '#f8fafc' }}>
                <TableRow>
                  <TableCell sx={{ fontWeight: 700 }}>PR Number</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Title & Scope</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Requester</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Vendor Quoted</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Total Amount</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Your Office Authority</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {pendingQueue.map((pr) => (
                  <TableRow key={pr.id} hover>
                    <TableCell sx={{ fontWeight: 700 }}>{pr.requestNumber}</TableCell>
                    <TableCell>
                      <Typography variant="body2" sx={{ fontWeight: 600 }}>{pr.title}</Typography>
                      <Typography variant="caption" color="text.secondary">{pr.budgetCategory}</Typography>
                    </TableCell>
                    <TableCell>{pr.requesterName}</TableCell>
                    <TableCell>{pr.vendorName || 'N/A'}</TableCell>
                    <TableCell sx={{ fontWeight: 700, color: '#16a34a' }}>
                      ₹{pr.grandTotal.toLocaleString('en-IN')}
                    </TableCell>
                    <TableCell>
                      <Chip label={pr.currentAssignedRole} color="primary" size="small" sx={{ fontWeight: 600 }} />
                    </TableCell>
                    <TableCell>
                      <Box sx={{ display: 'flex', gap: 1 }}>
                        <Button
                          size="small"
                          color="success"
                          variant="contained"
                          startIcon={<CheckCircleIcon />}
                          onClick={() => handleOpenDecision(pr, 1)}
                        >
                          Approve
                        </Button>
                        <Button
                          size="small"
                          color="warning"
                          variant="outlined"
                          startIcon={<ReplyIcon />}
                          onClick={() => handleOpenDecision(pr, 3)}
                        >
                          Return
                        </Button>
                        <Button
                          size="small"
                          color="error"
                          variant="outlined"
                          startIcon={<CancelOutlinedIcon />}
                          onClick={() => handleOpenDecision(pr, 2)}
                        >
                          Reject
                        </Button>
                      </Box>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      {/* Decision Dialog */}
      <Dialog open={decisionModalOpen} onClose={() => setDecisionModalOpen(false)} maxWidth="sm" fullWidth>
        <form onSubmit={handleDecisionSubmit}>
          <DialogTitle>
            Submit Committee Decision: {selectedPR?.requestNumber}
          </DialogTitle>
          <DialogContent sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 1 }}>
            {actionError && <Alert severity="error">{actionError}</Alert>}
            <Typography variant="body2">
              Request: <strong>{selectedPR?.title}</strong> (Total: ₹{selectedPR?.grandTotal.toLocaleString('en-IN')})
            </Typography>
            <TextField
              select
              label="Decision"
              value={decision}
              onChange={(e) => setDecision(parseInt(e.target.value))}
            >
              <MenuItem value={1}>Approve & Forward to Next Tier</MenuItem>
              <MenuItem value={3}>Return to Requester for Changes</MenuItem>
              <MenuItem value={2}>Reject Request Permanently</MenuItem>
            </TextField>
            <TextField
              label="Comments & Rationale"
              multiline
              rows={3}
              required={decision !== 1}
              value={comments}
              placeholder="Provide context or instructions for this decision..."
              onChange={(e) => setComments(e.target.value)}
            />
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setDecisionModalOpen(false)}>Cancel</Button>
            <Button
              type="submit"
              variant="contained"
              disabled={submitting}
              color={decision === 1 ? 'success' : decision === 2 ? 'error' : 'warning'}
            >
              {submitting ? 'Recording...' : 'Confirm Decision'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </Box>
  );
};
